using Cody.Core.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;
using System;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Connector
{
    public class AgentConnector
    {
        private IAgentProcess agentProcess;
        private JsonRpc jsonRpc;
        private IAgentClient agentClient;
        private AgentConnectorOptions options;
        private ILog log;

        public AgentConnector(AgentConnectorOptions connectorOptions, ILog log)
        {
            if (connectorOptions == null) throw new ArgumentNullException(nameof(connectorOptions));

            options = connectorOptions;
            this.log = log;
        }

        public bool IsConnected { get; private set; }

        public async Task Connect()
        {
            if (IsConnected) return;

            if (options.Port != null)
            {
                agentProcess = await RemoteAgent.Connect(options, OnAgentExit);
            }
            else
            {
                agentProcess = AgentProcess.Start(options.AgentDirectory, options.Debug, log, OnAgentExit);
            }
            log.Info("The agent process has started.");

            var jsonMessageFormatter = new JsonMessageFormatter();
            jsonMessageFormatter.JsonSerializer.ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            jsonMessageFormatter.JsonSerializer.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));

            var handler = new HeaderDelimitedMessageHandler(agentProcess.SendingStream, agentProcess.ReceivingStream, jsonMessageFormatter);
            jsonRpc = new JsonRpc(handler);

            if (options.NotificationsTarget != null) jsonRpc.AddLocalRpcTarget(options.NotificationsTarget);
            agentClient = jsonRpc.Attach<IAgentClient>();

            jsonRpc.StartListening();
            IsConnected = true;

            if (options.AfterConnection != null) await options.AfterConnection(agentClient);

            // Makes sure the notifications target is set after the connection is established.
            options.NotificationsTarget.SetAgentClient(agentClient);

            log.Info("A connection with the agent has been established.");
        }

        private void OnAgentExit(int exitCode)
        {
            DisconnectInternal();
            if (exitCode == 0) log.Info("The agent's process has ended.");
            else log.Error($"The agent process unexpectedly ended with code {exitCode}.");

            if (options.RestartAgentOnFailure && exitCode != 0)
            {
                log.Info("Restarting the agent.");
                // TODO: Make RemoteAgent wait until there's an agent to reconnect to before failing.
                Connect().Wait();
            }
        }

        public void Disconnect()
        {
            if (!IsConnected) return;

            DisconnectInternal();
        }

        private void DisconnectInternal()
        {
            if (options.BeforeDisconnection != null) options.BeforeDisconnection(agentClient);

            jsonRpc.Dispose();
            agentProcess.Dispose();

            jsonRpc = null;
            agentProcess = null;

            IsConnected = false;
            log.Info("The connection with the agent has been terminated.");
        }

        public IAgentClient CreateClient()
        {
            if (!IsConnected) Connect();

            return agentClient;
        }
    }
}

using Cody.Core.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;

namespace Cody.VisualStudio.Client
{
    public class AgentClient
    {
        private AgentClientOptions options;
        private ILog log;
        private ILog agentLog;
        private IAgentConnector connector;
        private JsonRpc jsonRpc;

        public AgentClient(AgentClientOptions options, ILog log, ILog agentLog)
        {
            this.options = options;
            this.log = log;
            this.agentLog = agentLog;
        }

        public bool IsConnected { get; private set; }

        public void Start()
        {
            if (IsConnected) return;

            connector = CreateConnector();
            connector.ErrorReceived += OnErrorReceived;
            connector.Disconnected += OnAgentDisconnected;

            connector.Connect(options);

            var jsonMessageFormatter = new AgentJsonMessageFormatter(agentLog);
            jsonMessageFormatter.TraceSentMessages = options.Debug;
            jsonMessageFormatter.JsonSerializer.ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            jsonMessageFormatter.JsonSerializer.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            var handler = new HeaderDelimitedMessageHandler(connector.SendingStream, connector.ReceivingStream, jsonMessageFormatter);

            jsonRpc = new JsonRpc(handler);
            jsonRpc.Disconnected += OnDisconnected;

            foreach (var target in options.CallbackHandlers)
            {
                var methods = NameTransformer.GetCallbackMethods(target.GetType());
                foreach (var method in methods) jsonRpc.AddLocalRpcMethod(method.Key, target, method.Value);
            }

            jsonRpc.StartListening();
            IsConnected = true;
            log.Info("A connection with the agent has been established.");
        }

        private void OnDisconnected(object sender, JsonRpcDisconnectedEventArgs e)
        {
            log.Error($"Agent disconnected due to {e.Description} (reason: {e.Reason})", e.Exception);
        }

        public T CreateAgentService<T>() where T : class
        {
            var proxyOptions = new JsonRpcProxyOptions { MethodNameTransform = NameTransformer.CreateTransformer<T>() };
            return jsonRpc.Attach<T>(proxyOptions);
        }

        private void OnErrorReceived(object sender, string error)
        {
            agentLog.Error(error);
        }

        private IAgentConnector CreateConnector()
        {
            IAgentConnector connector;
            if (options.ConnectToRemoteAgent)
            {
                connector = new RemoteAgentConnector();
                log.Info("Remote agent connector created");
            }
            else
            {
                connector = new AgentProcessConnector();
                log.Info("Process agent connector created");
            }

            return connector;
        }

        private void OnAgentDisconnected(object sender, int exitCode)
        {
            DisconnectInternal();
            if (exitCode == 0) log.Info("The agent's connection has ended.");
            else log.Error($"The agent connection unexpectedly ended with code {exitCode}.");

            if (options.RestartAgentOnFailure && exitCode != 0)
            {
                log.Info("Restarting the agent.");

                Start();
            }
        }

        public void Stop()
        {
            if (!IsConnected) return;

            DisconnectInternal();
        }

        private void DisconnectInternal()
        {
            jsonRpc.Dispose();
            connector.ErrorReceived -= OnErrorReceived;
            connector.Disconnected -= OnAgentDisconnected;
            connector.Disconnect();

            jsonRpc = null;
            connector = null;

            IsConnected = false;
            log.Info("The connection with the agent has been terminated.");
        }
    }
}

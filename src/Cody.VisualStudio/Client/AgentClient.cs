using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Logging;
using Cody.Core.Trace;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;
using System;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Client
{
    public class AgentClient : IAgentProxy
    {
        private static TraceLogger trace = new TraceLogger(nameof(AgentClient));

        private AgentClientOptions options;
        private ILog log;
        private ILog agentLog;
        private IAgentConnector connector;
        private JsonRpc jsonRpc;
        private IAgentService _proxy;

        public event EventHandler<ServerInfo> OnInitialized;

        public AgentClient(AgentClientOptions options, ILog log, ILog agentLog)
        {
            this.options = options;
            this.log = log;
            this.agentLog = agentLog;
        }

        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }

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

            jsonRpc = new TraceJsonRpc(handler);
            jsonRpc.Disconnected += OnDisconnected;

            foreach (var target in options.CallbackHandlers)
            {
                var methods = NameTransformer.GetCallbackMethods(target.GetType());
                foreach (var method in methods) jsonRpc.AddLocalRpcMethod(method.Key, target, method.Value);
            }

            jsonRpc.StartListening();
        }

        public async Task<IAgentService> Initialize(ClientInfo clientInfo)
        {
            CreateAgentService();

            var initialize = await _proxy.Initialize(clientInfo);
            IsInitialized = true;
            log.Info("Agent initialized.");

            OnInitialized?.Invoke(this, initialize);

            return _proxy;
        }

        private void OnDisconnected(object sender, JsonRpcDisconnectedEventArgs e)
        {
            log.Error($"Agent disconnected due to {e.Description} (reason: {e.Reason})", e.Exception);
        }

        private void CreateAgentService()
        {
            var proxyOptions = new JsonRpcProxyOptions { MethodNameTransform = NameTransformer.CreateTransformer<IAgentService>() };
            _proxy = jsonRpc.Attach<IAgentService>(proxyOptions);

            IsConnected = true;
            log.Info("A connection with the agent has been established.");
        }

        private void OnErrorReceived(object sender, string error)
        {
            agentLog.Debug(error);
            trace.TraceEvent("AgentErrorOutput", error);
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

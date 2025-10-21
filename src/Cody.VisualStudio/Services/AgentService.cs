using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Logging;
using Cody.VisualStudio.Client;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;
using Cody.Core.Infrastructure;

namespace Cody.VisualStudio.Services
{
    public class AgentService: IAgentService, IDisposable
    {
        private readonly AgentClient _agentClient;
        private readonly Func<ClientInfo> _getClientConfig;
        private readonly Action _onAgentInitialized;

        private readonly ILog _logger;


        private IAgentApi _agent;

        public AgentService(
            AgentClient agentClient, 
            Func<ClientInfo> getClientConfig, 
            Action  onAgentInitialized,
            ILog logger)
        {
            _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
            _getClientConfig = getClientConfig ?? throw new ArgumentNullException(nameof(getClientConfig));
            _onAgentInitialized = onAgentInitialized;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _agentClient.OnInitialized += OnAgentClientInitialized;
            _agentClient.AgentDisconnected += OnAgentDisconnected;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.Info("Starting agent initialization...");
                
                _agentClient.Start();

                var clientConfig = _getClientConfig();
                _agent = await _agentClient.Initialize(clientConfig);

                _onAgentInitialized?.Invoke();

                _logger.Info("Agent initialization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Agent initialization failed.", ex);
            }
        }

        private async Task RestartAsync()
        {
            _logger.Info("Restarting agent...");
            
            try
            {
                // Stop the current agent if it's running
                if (_agentClient.IsConnected)
                {
                    _agentClient.Stop();
                }

                // Clear the current agent reference
                _agent = null;

                // Reinitialize the agent
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Agent restart failed.", ex);
            }
        }

        public void Stop()
        {
            _logger.Info("Stopping agent service...");
            
            if (_agentClient != null)
            {
                _agentClient.OnInitialized -= OnAgentClientInitialized;
                _agentClient.AgentDisconnected -= OnAgentDisconnected;
                _agentClient.Stop();
            }

            _agent = null;
        }

        private void OnAgentClientInitialized(object sender, ServerInfo serverInfo)
        {
            _logger.Info($"Agent client initialized with server: {serverInfo}");
        }

        private void OnAgentDisconnected(object sender, int exitCode)
        {
            _logger.Info($"Agent disconnected with exit code: {exitCode}");
            
            // Clear the current agent reference
            _agent = null;

            if (exitCode != 0 && !VsShellUtilities.ShutdownToken.IsCancellationRequested)
            {
                _logger.Info("Agent disconnected unexpectedly. Restarting...");
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await RestartAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Failed to restart agent after disconnection.", ex);
                    }
                });
            }
        }

        public IAgentApi Get()
        {
            return _agent;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}

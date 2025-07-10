using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cody.Core.Logging;

namespace Cody.VisualStudio.Client
{
    public class RemoteAgentConnector : IAgentConnector
    {
        private readonly ILog _logger;
        private TcpClient client;

        private TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

        public RemoteAgentConnector(ILog logger)
        {
            _logger = logger;
        }

        public event EventHandler<int> Disconnected;
        public event EventHandler<string> ErrorReceived;

        public void Connect(AgentClientOptions options)
        {
            var timeout = DateTime.UtcNow.Add(_defaultTimeout);
            while (DateTime.UtcNow < timeout)
            {
                try
                {
                    _logger.Debug($"Try to connect to port:{options.RemoteAgentPort} ...");

                    client = new TcpClient();
                    client.Connect(IPAddress.Loopback, options.RemoteAgentPort);
                    return;
                }
                catch
                {
                    client.Close();
                    client.Dispose();

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
            
            _logger.Error($"Failed to connect to remote agent within {_defaultTimeout.TotalMinutes} minutes timeout");
        }

        public void Disconnect()
        {
            if (client != null && client.Connected)
            {
                _logger.Debug("Disconnecting from remote agent...");
                client.Close();
                client = null;
                _logger.Info("Successfully disconnected from remote agent");

                Disconnected?.Invoke(this, 0);
            }
            else
            {
                _logger.Debug("Disconnect called but client is not connected");
            }
        }

        public Stream SendingStream => client?.GetStream();
        public Stream ReceivingStream => client?.GetStream();
    }
}

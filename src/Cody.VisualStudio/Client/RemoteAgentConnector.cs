using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Cody.Core.Logging;

namespace Cody.VisualStudio.Client
{
    public class RemoteAgentConnector : IAgentConnector
    {
        private readonly ILog _logger;
        private TcpClient client;

        public RemoteAgentConnector(ILog logger)
        {
            _logger = logger;
        }

        public event EventHandler<int> Disconnected;
        public event EventHandler<string> ErrorReceived;

        public void Connect(AgentClientOptions options)
        {
            while (true)
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
        }

        public void Disconnect()
        {
            if (client != null && client.Connected)
            {
                client.Close();
                client = null;
                Disconnected?.Invoke(this, 0);
            }
        }

        public Stream SendingStream => client?.GetStream();
        public Stream ReceivingStream => client?.GetStream();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Client
{
    public class RemoteAgentConnector : IAgentConnector
    {
        private TcpClient client;

        public event EventHandler<int> Disconnected;
        public event EventHandler<string> ErrorReceived;

        public void Connect(AgentClientProviderOptions options)
        {
            client = new TcpClient();
            client.Connect(IPAddress.Loopback, options.RemoteAgentPort);
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

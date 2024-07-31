using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Connector
{
    class RemoteAgent : IAgentProcess
    {
        internal static async Task<RemoteAgent> Connect(AgentConnectorOptions options, Action<int> onExit)
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, options.Port ?? 3113);
            return new RemoteAgent(client, onExit);
        }

        private TcpClient client;
        private Action<int> onExit;

        private RemoteAgent(TcpClient client, Action<int> onExit) {
            this.client = client;
            this.onExit = onExit;

            // TODO: Wrap the returned streams and if they fail to read or write, call the onExit callback.
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public bool IsConnected {
            get {
                return this.client.Connected;
            }
        }

        public Stream SendingStream => client.GetStream();
        public Stream ReceivingStream => client.GetStream();
    }
}

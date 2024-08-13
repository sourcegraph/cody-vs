using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Client
{
    public interface IAgentConnector
    {
        void Connect(AgentClientOptions options);

        void Disconnect();

        event EventHandler<int> Disconnected;

        event EventHandler<string> ErrorReceived;

        Stream SendingStream { get; }
        Stream ReceivingStream { get; }
    }
}

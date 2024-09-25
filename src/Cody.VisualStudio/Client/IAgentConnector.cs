using System;
using System.IO;

namespace Cody.VisualStudio.Client
{
    public interface IAgentConnector
    {
        void Connect(AgentClientProviderOptions options);

        void Disconnect();

        event EventHandler<int> Disconnected;

        event EventHandler<string> ErrorReceived;

        Stream SendingStream { get; }
        Stream ReceivingStream { get; }
    }
}

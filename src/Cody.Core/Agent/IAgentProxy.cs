using System;
using System.Threading.Tasks;
using Cody.Core.Agent.Protocol;

namespace Cody.Core.Agent
{
    public interface IAgentProxy
    {
        bool IsConnected { get; }
        bool IsInitialized { get; }
        void Start();
        Task<IAgentService> Initialize(ClientInfo clientInfo);
        event EventHandler<ServerInfo> OnInitialized;
    }
}

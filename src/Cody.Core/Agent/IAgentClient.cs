using Cody.Core.Agent.Protocol;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public interface IAgentClient
    {
        [JsonRpcMethod("initialize")]
        Task<ServerInfo> Initialize(ClientInfo clientInfo);

        [JsonRpcMethod("graphql/getCurrentUserCodySubscription")]
        Task<CurrentUserCodySubscription> GetCurrentUserCodySubscription();

        [JsonRpcMethod("initialized")]
        void Initialized();



        [JsonRpcMethod("webview/resolveWebviewView", UseSingleObjectParameterDeserialization = true)]
        Task ResolveWebviewView(ResolveWebviewViewParams paramValue);

        [JsonRpcMethod("webview/receiveMessageStringEncoded")]
        Task ReceiveMessageStringEncoded(ReceiveMessageStringEncodedParams paramValue);
    }
}

public class ResolveWebviewViewParams
{
    public string ViewId { get; set; }
    public string WebviewHandle { get; set; }
}

public class ReceiveMessageStringEncodedParams
{ 
    public string Id { get; set; }
    public string MessageStringEncoded { get; set; }
}
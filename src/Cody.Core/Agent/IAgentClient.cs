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



        [JsonRpcMethod("webview/resolveWebviewView")]
        Task ResolveWebviewView(string viewId, string webviewHandle);

        [JsonRpcMethod("webview/receiveMessageStringEncoded")]
        Task ReceiveMessageStringEncoded(string id, string messageStringEncoded);



        [JsonRpcMethod("textDocument/didOpen", UseSingleObjectParameterDeserialization = true)]
        void DidOpen(ProtocolTextDocument docState);

        [JsonRpcMethod("textDocument/didChange", UseSingleObjectParameterDeserialization = true)]
        void DidChange(ProtocolTextDocument docState);

        [JsonRpcMethod("textDocument/didFocus")]
        void DidFocus(string uri);

        [JsonRpcMethod("textDocument/didSave")]
        void DidSave(string uri);

        [JsonRpcMethod("textDocument/didClose", UseSingleObjectParameterDeserialization = true)]
        void DidClose(ProtocolTextDocument docState);
    }
}

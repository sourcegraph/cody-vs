using Cody.Core.Agent.Protocol;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public interface IAgentService
    {
        [AgentMethod("initialize")]
        Task<ServerInfo> Initialize(ClientInfo clientInfo);

        [AgentMethod("graphql/getCurrentUserCodySubscription")]
        Task<CurrentUserCodySubscription> GetCurrentUserCodySubscription();

        [AgentMethod("initialized")]
        void Initialized();

        [AgentMethod("git/codebaseName")]
        Task<string> GetGitCodebaseName(string url);

        [AgentMethod("webview/resolveWebviewView")]
        Task ResolveWebviewView(ResolveWebviewViewParams paramValue);

        [AgentMethod("webview/receiveMessageStringEncoded")]
        Task ReceiveMessageStringEncoded(ReceiveMessageStringEncodedParams paramValue);

        [AgentMethod("env/openExternal")]
        Task OpenExternal(string url);

        [AgentMethod("textDocument/didOpen")]
        void DidOpen(ProtocolTextDocument docState);

        [AgentMethod("textDocument/didChange")]
        void DidChange(ProtocolTextDocument docState);

        [AgentMethod("textDocument/didFocus")]
        void DidFocus(string uri);

        [AgentMethod("textDocument/didSave")]
        void DidSave(string uri);

        [AgentMethod("textDocument/didClose")]
        void DidClose(ProtocolTextDocument docState);

        [AgentMethod("chat/new")]
        Task<string> NewChat();

        [AgentMethod("chat/sidebar/new")]
        Task<ChatPanelInfo> NewSidebarChat();

        [AgentMethod("chat/web/new")]
        Task<ChatPanelInfo> NewEditorChat();
    }
}


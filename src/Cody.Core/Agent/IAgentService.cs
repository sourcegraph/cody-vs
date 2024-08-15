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
        Task<string> GetGitCodebaseName(CodyFilePath path);

        [AgentMethod("webview/resolveWebviewView")]
        Task ResolveWebviewView(ResolveWebviewViewParams paramValue);

        [AgentMethod("webview/receiveMessageStringEncoded")]
        Task ReceiveMessageStringEncoded(ReceiveMessageStringEncodedParams paramValue);

        [AgentMethod("extensionConfiguration/change")]
        Task<AuthStatus> ConfigurationChange(ExtensionConfiguration configuration);


        [AgentMethod("textDocument/didOpen")]
        void DidOpen(ProtocolTextDocument docState);

        [AgentMethod("textDocument/didChange")]
        void DidChange(ProtocolTextDocument docState);

        [AgentMethod("textDocument/didFocus")]
        void DidFocus(CodyFilePath path);

        [AgentMethod("textDocument/didSave")]
        void DidSave(CodyFilePath path);

        [AgentMethod("textDocument/didClose")]
        void DidClose(ProtocolTextDocument docState);

        [AgentMethod("chat/new")]
        Task<string> NewChat();

        [AgentMethod("chat/sidebar/new")]
        Task<ChatPanelInfo> NewSidebarChat();

        [AgentMethod("chat/web/new")]
        Task<ChatPanelInfo> NewEditorChat();

        [AgentMethod("workspaceFolder/didChange")]
        Task WorkspaceFolderDidChange(CodyFilePath path);
    }
}

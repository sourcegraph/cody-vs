using Cody.Core.Agent.Protocol;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    //---------------------------------------------------------
    // For notifications return type MUST be void!
    //---------------------------------------------------------
    public interface IAgentService
    {
        [AgentCall("initialize")]
        Task<ServerInfo> Initialize(ClientInfo clientInfo);

        [AgentCall("graphql/getCurrentUserCodySubscription")]
        Task<CurrentUserCodySubscription> GetCurrentUserCodySubscription();

        [AgentCall("initialized")]
        void Initialized();

        [AgentCall("git/codebaseName")]
        Task<string> GetGitCodebaseName(CodyFilePath path);

        [AgentCall("webview/resolveWebviewView")]
        Task ResolveWebviewView(ResolveWebviewViewParams paramValue);

        [AgentCall("webview/receiveMessageStringEncoded")]
        Task ReceiveMessageStringEncoded(ReceiveMessageStringEncodedParams paramValue);

        [AgentCall("extensionConfiguration/change")]
        Task<AuthStatus> ConfigurationChange(ExtensionConfiguration configuration);

        [AgentCall("textDocument/didOpen")]
        void DidOpen(ProtocolTextDocument docState);

        [AgentCall("textDocument/didChange")]
        void DidChange(ProtocolTextDocument docState);

        [AgentCall("textDocument/didFocus")]
        void DidFocus(CodyFilePath path);

        [AgentCall("textDocument/didSave")]
        void DidSave(CodyFilePath path);

        [AgentCall("textDocument/didClose")]
        void DidClose(ProtocolTextDocument docState);

        [AgentCall("chat/new")]
        Task<string> NewChat();

        [AgentCall("chat/sidebar/new")]
        Task<ChatPanelInfo> NewSidebarChat();

        [AgentCall("chat/web/new")]
        Task<ChatPanelInfo> NewEditorChat();

        [AgentCall("workspaceFolder/didChange")]
        void WorkspaceFolderDidChange(WorkspaceFolderDidChangeEvent uris);

        //---------------------------------------------------------
        // For notifications return type MUST be void!
        //---------------------------------------------------------
    }
}

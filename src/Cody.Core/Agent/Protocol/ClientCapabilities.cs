using System.Runtime.Serialization;

namespace Cody.Core.Agent.Protocol
{
    public class ClientCapabilities
    {
        public Capability? Authentication { get; set; }
        public CompletionsCapability? Completions { get; set; }
        public Capability? Autoedit { get; set; }
        public AutoeditInlineDiffCapability? AutoeditInlineDiff { get; set; }
        public AutoeditAsideDiffCapability? AutoeditAsideDiff { get; set; }
        public Capability? AutoeditSuggestToEnroll { get; set; }
        public ChatCapability? Chat { get; set; }
        public Capability? Git { get; set; }
        public Capability? ProgressBars { get; set; }
        public Capability? Edit { get; set; }
        public Capability? EditWorkspace { get; set; }
        public Capability? UntitledDocuments { get; set; }
        public Capability? ShowDocument { get; set; }
        public Capability? CodeLenses { get; set; }
        public ShowWindowMessageCapability? ShowWindowMessage { get; set; }
        public Capability? Ignore { get; set; }
        public Capability? CodeActions { get; set; }
        public Capability? AccountSwitchingInWebview { get; set; }
        public Capability? CodeCopyOnlyAction { get; set; }

        public Capability? Shell { get; set; }
        public WebviewMessagesCapability? WebviewMessages { get; set; }
        public GlobalStateCapability? GlobalState { get; set; }
        public SecretsCapability? Secrets { get; set; }
        public WebviewCapability? Webview { get; set; }
        public WebviewCapabilities WebviewNativeConfig { get; set; }


    }

    public enum AutoeditInlineDiffCapability
    {
        None,
        [EnumMember(Value = "insertions-only")]
        InsertionsOnly,
        [EnumMember(Value = "deletions-only")]
        DeletionsOnly,
        [EnumMember(Value = "insertions-and-deletions")]
        InsertionsAndDeletions
    }

    public enum AutoeditAsideDiffCapability
    {
        None,
        Image,
        Diff
    }

    public enum Capability
    {
        None,
        Enabled
    }

    public enum CompletionsCapability
    {
        None
    }

    public enum ChatCapability
    {
        None,
        Streaming
    }

    public enum ShowWindowMessageCapability
    {
        Notification,
        Request
    }

    public enum WebviewCapability
    {
        Agentic,
        Native
    }

    public enum WebviewMessagesCapability
    {
        [EnumMember(Value = "object-encoded")]
        ObjectEncoded,
        [EnumMember(Value = "string-encoded")]
        StringEncoded,
    }

    public enum GlobalStateCapability
    {
        Stateless,
        [EnumMember(Value = "server-managed")]
        ServerManaged,
        [EnumMember(Value = "client-managed")]
        ClientManaged
    }

    public enum SecretsCapability
    {
        Stateless,
        [EnumMember(Value = "client-managed")]
        ClientManaged
    }


}

using Cody.Core.Agent.Protocol;
using Cody.Core.Workspace;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class NotificationHandlers : INotificationHandler
    {
        private readonly WebviewMessageHandler _messageFilter;
        private readonly IUserSettingsService _settingsService;
        private readonly IFileService _fileService;
        private readonly ILog _logger;

        public IAgentService agentClient;
        private readonly TaskCompletionSource<bool> agentClientReady = new TaskCompletionSource<bool>();

        public delegate Task PostWebMessageAsJsonDelegate(string message);
        public PostWebMessageAsJsonDelegate PostWebMessageAsJson { get; set; }

        public event EventHandler<SetHtmlEvent> OnSetHtmlEvent;
        public event EventHandler OnOptionsPageShowRequest;
        public event EventHandler<AgentResponseEvent> OnPostMessageEvent;

        public NotificationHandlers(IUserSettingsService settingsService, ILog logger, IFileService fileService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _logger = logger;
            _messageFilter = new WebviewMessageHandler(settingsService, fileService, () => OnOptionsPageShowRequest?.Invoke(this, EventArgs.Empty));
        }

        public void SetAgentClient(IAgentService client)
        {
            agentClient = client;
            agentClientReady.SetResult(true);
        }

        // Send a message to the host from webview.
        public async Task SendWebviewMessage(string handle, string message)
        {
            bool handled = _messageFilter.HandleMessage(message);
            if (!handled)
            {
                await agentClient.ReceiveMessageStringEncoded(new ReceiveMessageStringEncodedParams
                {
                    Id = handle,
                    MessageStringEncoded = message
                });
            }
        }

        [AgentNotification("debug/message")]
        public void Debug(string channel, string message)
        {
            _logger.Debug(message, channel);
        }

        [AgentNotification("webview/registerWebview")]
        public void RegisterWebview(string handle)
        {
            _logger.Debug(handle, "RegisterWebview");
        }

        [AgentNotification("webview/registerWebviewViewProvider")]
        public async Task RegisterWebviewViewProvider(string viewId, bool retainContextWhenHidden)
        {
            _logger.Debug(viewId, "RegisterWebviewViewProvider");
            agentClientReady.Task.Wait();
            await agentClient.ResolveWebviewView(new ResolveWebviewViewParams
            {
                // cody.chat for sidebar view, or cody.editorPanel for editor panel
                ViewId = viewId,
                // TODO: Create dynmically when we support editor panel
                WebviewHandle = "visual-studio-sidebar",
            });
        }

        [AgentNotification("webview/createWebviewPanel", deserializeToSingleObject: true)]
        public void CreateWebviewPanel(CreateWebviewPanelParams panelParams)
        {
            _logger.Debug(panelParams.ToString(), "CreateWebviewPanel");
        }

        [AgentNotification("webview/setOptions")]
        public void SetOptions(string handle, DefiniteWebviewOptions options)
        {
            if (options.EnableCommandUris is bool enableCmd)
            {
                _logger.Debug(handle, "SetOptions");
            }
            else if (options.EnableCommandUris is JArray jArray)
            {
                var uris = jArray.ToObject<string[]>();
            }
        }

        [AgentNotification("webview/setHtml")]
        public void SetHtml(string handle, string html)
        {
            OnSetHtmlEvent?.Invoke(this, new SetHtmlEvent() { Handle = handle, Html = html });
        }

        [AgentNotification("webview/PostMessage")]
        public void PostMessage(string handle, string message)
        {
            PostMessageStringEncoded(handle, message);
        }

        [AgentNotification("webview/postMessageStringEncoded")]
        public void PostMessageStringEncoded(string id, string stringEncodedMessage)
        {
            _logger.Debug(stringEncodedMessage, "PostMessageStringEncoded");
            PostWebMessageAsJson?.Invoke(stringEncodedMessage);
        }

        [AgentNotification("webview/didDisposeNative")]
        public void DidDisposeNative(string handle)
        {
            _logger.Debug(handle, "DidDisposeNative");
        }

        [AgentNotification("webview/dispose")]
        public void Dispose(string handle)
        {
            _logger.Debug(handle, "Dispose");
        }

        [AgentNotification("webview/reveal")]
        public void Reveal(string handle, int viewColumn, bool preserveFocus)
        {
            _logger.Debug(handle, "Reveal");
        }

        [AgentNotification("webview/setTitle")]
        public void SetTitle(string handle, string title)
        {
            _logger.Debug(title, "SetTitle");
        }

        [AgentNotification("webview/setIconPath")]
        public void SetIconPath(string handle, string iconPathUri)
        {
            _logger.Debug(iconPathUri, "SetIconPath");
        }

        [AgentNotification("window/didChangeContext")]
        public void WindowDidChangeContext(string key, string value)
        {
            _logger.Debug(value, $@"WindowDidChangeContext Key - {key}");

            // Check the value to see if Cody is activated or deactivated
            // Deactivated: value = "false", meaning user is no longer authenticated.
            // In this case, we can send Agent a request to get the latest user AuthStatus to
            // confirm if the user is logged out or not.
            if (key == "cody.activated")
            {
                var isAuthenticated = value == "true";
                _logger.Debug(isAuthenticated.ToString(), "User is authenticated");
            }
        }

        [AgentNotification("extensionConfiguration/didChange", deserializeToSingleObject: true)]
        public void ExtensionConfigDidChange(ExtensionConfiguration config)
        {
            _logger.Debug(config.ToString(), "didChange");
        }

        [AgentNotification("ignore/didChange")]
        public void IgnoreDidChange()
        {
            _logger.Debug("Changed", "IgnoreDidChange");
        }

        // REQUEST FROM AGENT - NOT WORKING
        [AgentNotification("textDocument/show")]
        public Task<bool> ShowTextDocument(TextDocumentShowParams param)
        {
            var path = new Uri(param.Uri).ToString();
            return Task.FromResult(_fileService.OpenFileInEditor(path));
        }

        [AgentNotification("env/openExternal")]
        public Task<bool> OpenExternalLink(CodyFilePath path)
        {
            // Open the URL in the default browser
            System.Diagnostics.Process.Start(path.Uri);
            return Task.FromResult(true);

        }

        [AgentNotification("window/showSaveDialog")]
        public Task<string> ShowSaveDialog(SaveDialogOptionsParams paramValues)
        {
            return Task.FromResult("Not Yet Implemented");
        }
    }
}

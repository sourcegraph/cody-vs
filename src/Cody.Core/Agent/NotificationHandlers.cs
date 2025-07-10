using Cody.Core.Agent.Protocol;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Cody.Core.Trace;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class NotificationHandlers : INotificationHandler
    {
        private static TraceLogger trace = new TraceLogger(nameof(NotificationHandlers));

        private readonly WebviewMessageHandler _messageFilter;
        private readonly ISecretStorageService _secretStorage;
        private readonly ILog _logger;

        public IAgentService agentClient;

        public delegate Task PostWebMessageAsJsonDelegate(string message);
        public PostWebMessageAsJsonDelegate PostWebMessageAsJson { get; set; }

        public event EventHandler<SetHtmlEvent> OnSetHtmlEvent;

        public event EventHandler<ProtocolAuthStatus> AuthorizationDetailsChanged;
        public event EventHandler<string> OnRegisterWebViewRequest;
        public event EventHandler OnOptionsPageShowRequest;
        public event EventHandler OnFocusSidebarRequest;

        public event EventHandler<AgentResponseEvent> OnPostMessageEvent;

        public NotificationHandlers(ILog logger, IDocumentService documentService, ISecretStorageService secretStorage)
        {
            _secretStorage = secretStorage;
            _logger = logger;
            _messageFilter = new WebviewMessageHandler(documentService, () => OnOptionsPageShowRequest?.Invoke(this, EventArgs.Empty));
        }

        public void SetAgentClient(IAgentService client)
        {
            agentClient = client;
        }

        // Send a message to the host from webview.
        public async Task SendWebviewMessage(string handle, string message)
        {
            bool handled = _messageFilter.HandleMessage(message);
            if (!handled)
                await agentClient.ReceiveMessageStringEncoded(new ReceiveMessageStringEncodedParams
                {
                    Id = handle,
                    MessageStringEncoded = message
                });
        }

        [AgentCallback("debug/message")]
        public void Debug(string channel, string message, string level)
        {
            //_logger.Debug($"[{channel} {message}]");
            trace.TraceEvent("AgentDebug", message);
        }

        [AgentCallback("webview/registerWebview")]
        public void RegisterWebview(string handle)
        {
            _logger.Debug(handle);
        }

        [AgentCallback("webview/registerWebviewViewProvider")]
        public void RegisterWebviewViewProvider(string viewId, bool retainContextWhenHidden)
        {
            _logger.Debug(viewId);
            OnRegisterWebViewRequest?.Invoke(this, viewId);
        }

        [AgentCallback("webview/createWebviewPanel", deserializeToSingleObject: true)]
        public void CreateWebviewPanel(CreateWebviewPanelParams panelParams)
        {
            _logger.Debug(panelParams.ToString());
        }

        [AgentCallback("webview/setOptions")]
        public void SetOptions(string handle, DefiniteWebviewOptions options)
        {
            if (options.EnableCommandUris is bool enableCmd)
            {
                _logger.Debug(handle);
            }
            else if (options.EnableCommandUris is JArray jArray)
            {
                var uris = jArray.ToObject<string[]>();
            }
        }

        [AgentCallback("webview/setHtml")]
        public void SetHtml(string handle, string html)
        {
            OnSetHtmlEvent?.Invoke(this, new SetHtmlEvent() { Handle = handle, Html = html });
        }

        [AgentCallback("webview/PostMessage")]
        public void PostMessage(string handle, string message)
        {
            PostMessageStringEncoded(handle, message);
        }

        [AgentCallback("webview/postMessageStringEncoded")]
        public void PostMessageStringEncoded(string id, string stringEncodedMessage)
        {
            _logger.Debug(stringEncodedMessage);
            PostWebMessageAsJson?.Invoke(stringEncodedMessage);
        }

        [AgentCallback("webview/didDisposeNative")]
        public void DidDisposeNative(string handle)
        {
            _logger.Debug(handle);
        }

        [AgentCallback("webview/dispose")]
        public void Dispose(string handle)
        {
            _logger.Debug(handle);
        }

        [AgentCallback("webview/reveal")]
        public void Reveal(string handle, int viewColumn, bool preserveFocus)
        {
            _logger.Debug(handle);
        }

        [AgentCallback("webview/setTitle")]
        public void SetTitle(string handle, string title)
        {
            _logger.Debug(title);
        }

        [AgentCallback("webview/setIconPath")]
        public void SetIconPath(string handle, string iconPathUri)
        {
            _logger.Debug(iconPathUri);
        }

        [AgentCallback("window/didChangeContext")]
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

        [AgentCallback("extensionConfiguration/didChange", deserializeToSingleObject: true)]
        public void ExtensionConfigDidChange(ExtensionConfiguration config)
        {
            _logger.Debug(config.ToString());
        }

        [AgentCallback("ignore/didChange")]
        public void IgnoreDidChange()
        {
            _logger.Debug("Changed");
        }

        [AgentCallback("env/openExternal")]
        public Task<bool> OpenExternalLink(CodyFilePath path)
        {
            // Open the URL in the default browser
            System.Diagnostics.Process.Start(path.Uri);
            return Task.FromResult(true);

        }

        [AgentCallback("window/showSaveDialog")]
        public Task<string> ShowSaveDialog(SaveDialogOptionsParams paramValues)
        {
            return Task.FromResult("Not Yet Implemented");
        }

        [AgentCallback("secrets/get")]
        public Task<string> SecretGet(string key)
        {
            _logger.Debug(key, $@"SecretGet - {key}");
            return Task.FromResult(_secretStorage.Get(key));
        }

        [AgentCallback("secrets/store")]
        public void SecretStore(string key, string value)
        {
            _logger.Debug(key, $@"SecretStore - {key}");
            _secretStorage.Set(key, value);
        }

        [AgentCallback("secrets/delete")]
        public void SecretDelete(string key)
        {
            _logger.Debug(key, $@"SecretDelete - {key}");
            _secretStorage.Delete(key);
        }

        [AgentCallback("window/focusSidebar")]
        public void FocusSidebar(object param)
        {
            OnFocusSidebarRequest?.Invoke(this, EventArgs.Empty);
        }

        [AgentCallback("authStatus/didUpdate", deserializeToSingleObject: true)]
        public void AuthStatusDidUpdate(ProtocolAuthStatus authStatus)
        {
            _logger.Debug($"Pending validation: {authStatus.PendingValidation}");

            if (authStatus.PendingValidation)
                return;

            _logger.Debug($"Authenticated: {authStatus.Authenticated}");

            AuthorizationDetailsChanged?.Invoke(this, authStatus);
        }
    }
}

using Cody.Core.Agent.Protocol;
using Cody.Core.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Cody.Core.Infrastructure;

namespace Cody.Core.Agent
{
    public class WebviewNotificationHandlers
    {
        private readonly ILog _logger;
        public IAgentService _agentService;
        private WebviewMessageHandler _messageFilter;

        public WebviewNotificationHandlers(ILog logger)
        {
            _logger = logger;
            _messageFilter = new WebviewMessageHandler(() => OnOptionsPageShowRequest?.Invoke(this, EventArgs.Empty), _logger);
        }

        public event EventHandler<string> OnRegisterWebViewRequest;
        public event EventHandler<SetHtmlEvent> OnSetHtmlEvent;
        public event EventHandler OnOptionsPageShowRequest;
        public event EventHandler<string> PostWebMessageAsJson;

        public void SetAgentClient(IAgentService agentService)
        {
            _agentService = agentService;
        }

        // Send a message to the host from webview.
        public async Task SendWebviewMessage(string handle, string message)
        {
            try
            {
                bool handled = _messageFilter.HandleMessage(message);
                if (_agentService.Get() == null)
                {
                    _logger.Error("There is no agent listening.");
                }

                if (!handled && _agentService.Get() != null)
                    await _agentService.Get().ReceiveMessageStringEncoded(new ReceiveMessageStringEncodedParams
                    {
                        Id = handle,
                        MessageStringEncoded = message
                    });
            }
            catch (Exception ex)
            {
                _logger.Error("Sending message to the agent failed.", ex);
            }
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
            PostWebMessageAsJson?.Invoke(this, stringEncodedMessage);
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
    }
}

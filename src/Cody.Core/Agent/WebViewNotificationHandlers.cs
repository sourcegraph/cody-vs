using Cody.Core.Agent.Protocol;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class WebViewNotificationHandlers : IInjectAgentClient
    {
        private ICodyWebView codyWebView;
        private WebviewCommandsHandler webviewWebMessageHandler;
        private ILog logger;
        private TaskCompletionSource<bool> agentInitialized = new TaskCompletionSource<bool>();

        public IAgentClient AgentClient { set; private get; }

        private const string SidebarHandle = "visual-studio-sidebar";


        public WebViewNotificationHandlers(ICodyWebView codyWebView, WebviewCommandsHandler webviewWebMessageHandler, ILog logger)
        {
            this.codyWebView = codyWebView;
            this.webviewWebMessageHandler = webviewWebMessageHandler;
            this.logger = logger;

            codyWebView.SendWebMessage += OnWebViewSendWebMessage;
        }

        private async void OnWebViewSendWebMessage(object sender, string message)
        {
            bool handled = webviewWebMessageHandler.HandleMessage(message);
            if (!handled)
            {
                await AgentClient?.ReceiveMessageStringEncoded(new ReceiveMessageStringEncodedParams
                {
                    Id = SidebarHandle,
                    MessageStringEncoded = message
                });
            }
        }

        public void SetAgentInitialized() => agentInitialized.TrySetResult(true);

        [AgentCallback("webview/registerWebviewViewProvider")]
        public async Task RegisterWebviewViewProvider(string viewId, bool retainContextWhenHidden)
        {
            await agentInitialized.Task;

            await AgentClient.ResolveWebviewView(new ResolveWebviewViewParams
            {
                ViewId = viewId,
                WebviewHandle = SidebarHandle
            });
        }

        [AgentCallback("webview/setOptions")]
        public void SetOptions(string handle, DefiniteWebviewOptions options)
        {
            if (options.EnableCommandUris is bool enableCmd)
            {
                logger.Debug(handle);
            }
            else if (options.EnableCommandUris is JArray jArray)
            {
                var uris = jArray.ToObject<string[]>();
            }
        }

        [AgentCallback("webview/setHtml")]
        public async Task SetHtml(string handle, string html)
        {
            await codyWebView.WaitUntilWebViewReady();

            await AgentClient.ReceiveMessageStringEncoded(new ReceiveMessageStringEncodedParams
            {
                Id = handle,  //"visual-studio-sidebar"
                MessageStringEncoded = "{\"command\":\"ready\"}"
            });

            await AgentClient.ReceiveMessageStringEncoded(new ReceiveMessageStringEncodedParams
            {
                Id = handle,
                MessageStringEncoded = "{\"command\":\"initialized\"}"
            });
        }

        [AgentCallback("webview/postMessageStringEncoded")]
        public void PostMessageStringEncoded(string id, string stringEncodedMessage)
        {
            codyWebView.PostWebMessage(stringEncodedMessage);
        }
    }
}

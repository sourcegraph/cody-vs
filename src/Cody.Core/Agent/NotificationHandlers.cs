using Cody.Core.Agent.Connector;
using Cody.Core.Agent.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class NotificationHandlers
    {
        public NotificationHandlers()
        {
        }

        public event EventHandler<SetHtmlEvent> OnSetHtmlEvent;
        public event EventHandler<AgentResponseEvent> OnPostMessageEvent;

        public IAgentClient agentClient;

        public void SetAgentClient(IAgentClient agentClient) => this.agentClient = agentClient;

        public async Task SendWebviewMessage(string handle, string message)
        {
           await agentClient.ReceiveMessageStringEncoded(new ReceiveMessageStringEncodedParams
           {
               Id = handle,
               MessageStringEncoded = message
           });
        }

        [JsonRpcMethod("debug/message")]
        public void Debug(string channel, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, "Agent Debug");
        }
        
        [JsonRpcMethod("webview/registerWebview")]
        public void RegisterWebview(string handle)
        {
            System.Diagnostics.Debug.WriteLine(handle, "Agent registerWebview");
        }

        [JsonRpcMethod("webview/registerWebviewViewProvider")]
        public  void RegisterWebviewViewProvider(string viewId, bool retainContextWhenHidden)
        {
            System.Diagnostics.Debug.WriteLine(viewId, "Agent registerWebviewViewProvider");
            agentClient.ResolveWebviewView(new ResolveWebviewViewParams
            {
                ViewId = "cody.chat",
                WebviewHandle = "visual-studio-program",
            }).Wait();
        }

        [JsonRpcMethod("webview/createWebviewPanel", UseSingleObjectParameterDeserialization = true)]
        public void CreateWebviewPanel(CreateWebviewPanelParams panelParams)
        {
            System.Diagnostics.Debug.WriteLine(panelParams, "Agent createWebviewPanel");
        }

        [JsonRpcMethod("webview/setOptions")]
        public void SetOptions(string handle, DefiniteWebviewOptions options)
        {
            if(options.EnableCommandUris is bool enableCmd)
            {

            }
            else if(options.EnableCommandUris is JArray jArray)
            {
                var uris = jArray.ToObject<string[]>();
            }
        }

        [JsonRpcMethod("webview/setHtml")]
        public void SetHtml(string handle, string html)
        {
            System.Diagnostics.Debug.WriteLine(html, "Agent setHtml");
            OnSetHtmlEvent?.Invoke(this, new SetHtmlEvent() {Handle = handle, Html = html});
        }

        [JsonRpcMethod("webview/PostMessage")]
        public void PostMessage(string handle, string message)
        {
            PostMessageStringEncoded(handle, message);
        }

        [JsonRpcMethod("webview/postMessageStringEncoded")]
        public void PostMessageStringEncoded(string id, string stringEncodedMessage)
        {
            System.Diagnostics.Debug.WriteLine(stringEncodedMessage, "Agent postMessageStringEncoded");
            // TODO send message to Webview2Dev 

            OnPostMessageEvent.Invoke(this, new AgentResponseEvent() { Id = id, StringEncodedMessage = stringEncodedMessage});
        }

        [JsonRpcMethod("webview/didDisposeNative")]
        public void DidDisposeNative(string handle)
        {
            ;
        }

        [JsonRpcMethod("extensionConfiguration/didChange")]
        public void ExtensionConfigDidChange(ExtensionConfiguration config)
        {
            System.Diagnostics.Debug.WriteLine(config, "Agent didChange");
        }

        [JsonRpcMethod("webview/dispose")]
        public void Dispose(string handle)
        {
            System.Diagnostics.Debug.WriteLine(handle, "Agent dispose");
        }

        [JsonRpcMethod("webview/reveal")]
        public void Reveal(string handle, int viewColumn, bool preserveFocus)
        {
            System.Diagnostics.Debug.WriteLine(handle, "Agent reveal");
        }

        [JsonRpcMethod("webview/setTitle")]
        public void SetTitle(string handle, string title)
        {
            System.Diagnostics.Debug.WriteLine(title, "Agent setTitle");
        }

        [JsonRpcMethod("webview/setIconPath")]
        public void SetIconPath(string handle, string iconPathUri)
        {
            System.Diagnostics.Debug.WriteLine(iconPathUri, "Agent setIconPath");
        }

        [JsonRpcMethod("webview/createWebviewPanel")]
        public void CreateWebviewPanel(string handle, string viewType, string title, ShowOptions showOptions, bool enableScripts, bool enableForms, bool enableCommandUris, bool enableFindWidget, bool retainContextWhenHidden)
        {
            System.Diagnostics.Debug.WriteLine(title, "Agent createWebviewPanel");
        }

    }
}

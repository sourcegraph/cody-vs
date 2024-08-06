using Cody.Core.Agent.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System;
using EnvDTE;
using EnvDTE80;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class NotificationHandlers
    {
        public NotificationHandlers()
        {
        }
        public delegate Task PostWebMessageAsJsonDelegate(string message);
        public PostWebMessageAsJsonDelegate PostWebMessageAsJson { get; set; }

        public event EventHandler<SetHtmlEvent> OnSetHtmlEvent;
        public event EventHandler<AgentResponseEvent> OnPostMessageEvent;

        public IAgentClient agentClient;

        public void SetAgentClient(IAgentClient agentClient) => this.agentClient = agentClient;

        // Send a message to the host from webview.
        public async Task SendWebviewMessage(string handle, string message)
        {
            // Turn message into a JSON object
            JObject json = JObject.Parse(message);

            var command = json["command"].ToString();
            if (command.Equals("links"))
            {
                // if the is links, open the link in the default browser
                string link = json["value"].ToString();
                System.Diagnostics.Process.Start(link);
                return;
            }
            else if (command.Equals("command"))
            {
                string id = json["id"].ToString();
                if (id.Equals("cody.status-bar.interacted"))
                {
                    DTE2 dte = (DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");
                    dte.ExecuteCommand("Tools.Options", "Cody.General");
                    return;
                }
                // cody.auth.signin, cody.auth.signout, cody.auth.account
                // await agentClient.SignOut();
            }

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
        public void RegisterWebviewViewProvider(string viewId, bool retainContextWhenHidden)
        {
            System.Diagnostics.Debug.WriteLine(viewId, "Agent registerWebviewViewProvider");
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
            PostWebMessageAsJson?.Invoke(stringEncodedMessage);
            System.Diagnostics.Debug.WriteLine(stringEncodedMessage, "Agent postMessageStringEncoded");
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

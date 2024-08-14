using Cody.Core.Agent.Protocol;
using EnvDTE80;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class NotificationHandlers : INotificationHandler
    {
        public NotificationHandlers()
        {
        }

        public delegate Task PostWebMessageAsJsonDelegate(string message);
        public PostWebMessageAsJsonDelegate PostWebMessageAsJson { get; set; }

        public event EventHandler<SetHtmlEvent> OnSetHtmlEvent;
        public event EventHandler<AgentResponseEvent> OnPostMessageEvent;

        public IAgentService agentClient;

        private TaskCompletionSource<bool> agentClientReady = new TaskCompletionSource<bool>();


        public void SetAgentClient(IAgentService client)
        {
            agentClient = client;
            agentClientReady.SetResult(true);
        }

        // Send a message to the host from webview.
        public async Task SendWebviewMessage(string handle, string message)
        {
            try
            {
                var json = JObject.Parse(message);
                var command = json["command"]?.ToString();
                if (command == "command")
                {
                    var id = json["id"]?.ToString();
                    if (id == "cody.status-bar.interacted" || id?.StartsWith("cody.auth.signin") == true)
                    {
                        var dte = (DTE2)Marshal.GetActiveObject("VisualStudio.DTE");
                        dte.ExecuteCommand("Tools.Options", "Cody.General");
                        return;
                    }
                }
            }
            catch
            {
                // Ignore
            }
            await agentClient.ReceiveMessageStringEncoded(new ReceiveMessageStringEncodedParams
            {
                Id = handle,
                MessageStringEncoded = message
            });
        }

        [AgentNotification("debug/message")]
        public void Debug(string channel, string message)
        {
            DebugLog(message, "Debug");
        }

        [AgentNotification("webview/registerWebview")]
        public void RegisterWebview(string handle)
        {
            DebugLog(handle, "RegisterWebview");
        }

        [AgentNotification("webview/registerWebviewViewProvider")]
        public async Task RegisterWebviewViewProvider(string viewId, bool retainContextWhenHidden)
        {
            DebugLog(viewId, "RegisterWebviewViewProvider");
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
            DebugLog(panelParams.ToString(), "CreateWebviewPanel");
        }

        [AgentNotification("webview/setOptions")]
        public void SetOptions(string handle, DefiniteWebviewOptions options)
        {
            if (options.EnableCommandUris is bool enableCmd)
            {
                DebugLog(handle, "SetOptions");
            }
            else if (options.EnableCommandUris is JArray jArray)
            {
                var uris = jArray.ToObject<string[]>();
            }
        }

        [AgentNotification("webview/setHtml")]
        public void SetHtml(string handle, string html)
        {
            DebugLog(html, "SetHtml");
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
            DebugLog(stringEncodedMessage, "PostMessageStringEncoded");
            PostWebMessageAsJson?.Invoke(stringEncodedMessage);
        }

        [AgentNotification("webview/didDisposeNative")]
        public void DidDisposeNative(string handle)
        {
            DebugLog(handle, "DidDisposeNative");
        }

        [AgentNotification("webview/dispose")]
        public void Dispose(string handle)
        {
            DebugLog(handle, "Dispose");
        }

        [AgentNotification("webview/reveal")]
        public void Reveal(string handle, int viewColumn, bool preserveFocus)
        {
            DebugLog(handle, "Reveal");
        }

        [AgentNotification("webview/setTitle")]
        public void SetTitle(string handle, string title)
        {
            DebugLog(title, "SetTitle");
        }

        [AgentNotification("webview/setIconPath")]
        public void SetIconPath(string handle, string iconPathUri)
        {
            DebugLog(iconPathUri, "SetIconPath");
        }

        [AgentNotification("window/didChangeContext")]
        public void WindowDidChangeContext(string key, string value)
        {
            DebugLog(value, $@"WindowDidChangeContext Key - {key}");

            // Check the value to see if Cody is activated or deactivated
            // Deactivated: value = "false", meaning user is no longer authenticated.
            // In this case, we can send Agent a request to get the latest user AuthStatus to
            // confirm if the user is logged out or not.
            if (key == "cody.activated")
            {
                var isAuthenticated = value == "true";
                DebugLog(isAuthenticated.ToString(), "User is authenticated");
            }
        }

        [AgentNotification("extensionConfiguration/didChange", deserializeToSingleObject: true)]
        public void ExtensionConfigDidChange(ExtensionConfiguration config)
        {
            DebugLog(config.ToString(), "didChange");
        }

        public void DebugLog(string message, string origin)
        {
            // Log the message to the debug console when in debug mode.
#if DEBUG
            System.Diagnostics.Debug.WriteLine(message, $@"Agent Notified {origin}");
#endif
        }
    }
}

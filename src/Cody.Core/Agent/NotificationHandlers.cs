using Cody.Core.Agent.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class NotificationHandlers
    {
        [JsonRpcMethod("debug/message")]
        public void Debug(string channel, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, "Agent");
        }


        [JsonRpcMethod("webview/registerWebviewViewProvider")]
        public void RegisterWebviewViewProvider(string viewId, bool retainContextWhenHidden)
        {

        }

        [JsonRpcMethod("webview/createWebviewPanel", UseSingleObjectParameterDeserialization = true)]
        public void CreateWebviewPanel(CreateWebviewPanelParams panelParams)
        {

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

        }

        [JsonRpcMethod("webview/postMessageStringEncoded")]
        public void PostMessageStringEncoded(string id, string stringEncodedMessage)
        {

        }
    }
}

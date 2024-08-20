using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Cody.Core.Agent;

namespace Cody.UI.Controls
{
    public class WebviewController
    {
        private CoreWebView2 _webview;

        private string _colorThemeScript;

        private ICommand _sendMessageCommand;

        public NotificationHandlers NotificationHandlers;

        public WebviewController()
        {
        }

        public async Task<CoreWebView2> InitializeWebView(CoreWebView2 webView, ICommand sendMessageCommand)
        {
            _webview = webView;
            _sendMessageCommand = sendMessageCommand;

            SetupVirtualHostMapping();
            await ApplyVsCodeApiScript();
            SetupEventHandlers();
            ConfigureWebView();
            SetupResourceHandling();

            return webView;
        }

        private void ConfigureWebView()
        {
            _webview.Settings.AreDefaultScriptDialogsEnabled = true;
            _webview.Settings.IsWebMessageEnabled = true;
            _webview.Settings.AreDefaultScriptDialogsEnabled = true;
            _webview.Settings.AreHostObjectsAllowed = true;
            _webview.Settings.IsScriptEnabled = true;
            _webview.Settings.AreBrowserAcceleratorKeysEnabled = true;
            _webview.Settings.IsGeneralAutofillEnabled = true;
            // Enable below settings only in DEBUG mode.
            _webview.Settings.AreDefaultContextMenusEnabled = false;
            _webview.Settings.AreDevToolsEnabled = false;
            _webview.Settings.IsStatusBarEnabled = false;
#if DEBUG
            _webview.Settings.AreDefaultContextMenusEnabled = true;
            _webview.Settings.AreDevToolsEnabled = true;
#endif
        }

        private void SetupEventHandlers()
        {
            _webview.DOMContentLoaded += CoreWebView2OnDOMContentLoaded;
            _webview.WebMessageReceived += HandleWebViewMessage;
        }

        private void SetupResourceHandling()
        {
            string AppOrigin = "https://cody.vs";
            _webview.AddWebResourceRequestedFilter($"{AppOrigin}*", CoreWebView2WebResourceContext.All);
            _webview.WebResourceRequested += HandleWebResourceRequest;
        }

        private void HandleWebResourceRequest(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");
            var uri = new Uri(e.Request.Uri);
            var filePath = Path.Combine(agentDir, uri.AbsolutePath.TrimStart('/')).Replace("\\", "/");

            if (System.IO.File.Exists(filePath))
            {
                var response = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(filePath);
                e.Response = _webview.Environment.CreateWebResourceResponse(
                    new MemoryStream(response), 200, "OK", contentType);
            }
        }

        private void SetupVirtualHostMapping()
        {
            string agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent", "webviews");
            _webview.SetVirtualHostNameToFolderMapping("cody.vs", agentDir, CoreWebView2HostResourceAccessKind.Allow);
        }

        private string GetContentType(string filePath)
        {
            if (filePath.EndsWith(".js")) return "Content-Type: text/javascript";
            if (filePath.EndsWith(".css")) return "Content-Type: text/css";
            return "Content-Type: text/html";
        }

        private async void CoreWebView2OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            await ApplyThemingScript();
        }

        private void HandleWebViewMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Handle message sent from webview to agent.
            var message = e.TryGetWebMessageAsString();

            // TODO: Get token from message if message has a token.
            // IMPORTANT: Do not log the token to the console in production.
            System.Diagnostics.Debug.WriteLine(message, "Agent HandleWebViewMessage");

            // Dispatch the message to the ViewModel using the Command
            Application.Current.Dispatcher.Invoke(() => _sendMessageCommand?.Execute(message));
        }

        public async Task PostWebMessageAsJson(string message)
        {
            // From agent to webview.
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _webview.PostWebMessageAsJson(message);
                System.Diagnostics.Debug.WriteLine(message, "Agent PostWebMessageAsJson");
            });
        }

        private async Task ApplyVsCodeApiScript()
        {
            await _webview.AddScriptToExecuteOnDocumentCreatedAsync(VsCodeApi);
        }

        private async Task ApplyThemingScript()
        {
            await _webview.ExecuteScriptWithResultAsync(GetThemeScript(_colorThemeScript));
        }

        public void SetThemeScript(string colorThemeScript)
        {
            _colorThemeScript = colorThemeScript;
            // We might want to apply the theme immediately if the webview is already loaded.
            // if (_webview.CoreWebView2.IsDocumentOpen) {
            //     _ = ApplyThemingScript(); 
            // }
        }

        public void SetHtml(string html)
        {
            // NOTE: Serving the returned html doesn't work in Visual Studio,
            // as it doesn't allow access to the local storage _webview?.NavigateToString(html);

            _webview.Navigate("https://cody.vs/index.html");
        }

        private static readonly string VsCodeApi = @"
            globalThis.acquireVsCodeApi = (function() {
                let acquired = false;
                let state = undefined;

                window.chrome.webview.addEventListener('message', e => {
                    const event = new CustomEvent('message');
                    event.data = e.data;
                    window.dispatchEvent(event)
                });

                return () => {
                    if (acquired && !false) {
                        throw new Error('An instance of the VS Code API has already been acquired');
                    }
                    acquired = true;
                    return Object.freeze({
                        postMessage: function(message) {
                            window.chrome.webview.postMessage(JSON.stringify(message));
                        },
                        setState: function(newState) {
                            if (state === newState) {
                                return;
                            }
                            state = newState;
                            console.log(`Set State: ${JSON.stringify(newState)}`);
                            return newState;
                        },
                        getState: function() {
                            return state;
                        },
                        onMessage: callback => {
                            window.chrome.webview.addEventListener('message', e => callback(e.data));
                        }
                    });
                };
            })();
        ";

        private static string GetThemeScript(string colorTheme) => $@"
            document.documentElement.dataset.ide = 'VisualStudio';

            {colorTheme}
        ";
    }
}

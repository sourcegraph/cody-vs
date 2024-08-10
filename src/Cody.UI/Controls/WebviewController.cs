using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Reflection;

namespace Cody.UI.Controls
{
    public class WebviewController
    {
        private CoreWebView2 _webview;

        public CoreWebView2 GetWebview => _webview;

        public string colorThemeScript;

        public event EventHandler<string> WebViewMessageReceived;

        public WebviewController()
        {
        }

        public async Task<CoreWebView2> InitializeWebView(CoreWebView2 webView)
        {
            _webview = webView;

            SetupVirtualHostMapping();
            await ApplyVsCodeApiScript();
            SetupEventHandlers();
            ConfigureWebView();
            SetupResourceHandling();

            webView.OpenDevToolsWindow();

            return webView;
        }

        private async Task<CoreWebView2Environment> CreateWebView2Environment()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cody");
            var options = new CoreWebView2EnvironmentOptions
            {
#if DEBUG
                AdditionalBrowserArguments = "--remote-debugging-port=9222 --disable-web-security --allow-file-access-from-files",
                AllowSingleSignOnUsingOSPrimaryAccount = true,
#endif
            };
            return await CoreWebView2Environment.CreateAsync(null, appData, options);
        }

        private void ConfigureWebView()
        {
            _webview.Settings.AreDefaultScriptDialogsEnabled = true;
            _webview.Settings.IsWebMessageEnabled = true;
            _webview.Settings.AreDefaultScriptDialogsEnabled = true;
            _webview.Settings.AreHostObjectsAllowed = true;
            _webview.Settings.IsScriptEnabled = true;
            _webview.Settings.AreBrowserAcceleratorKeysEnabled = true;
            _webview.Settings.AreDevToolsEnabled = true;
            _webview.Settings.IsGeneralAutofillEnabled = true;
        }

        private void SetupEventHandlers()
        {
            _webview.DOMContentLoaded += CoreWebView2OnDOMContentLoaded;
            _webview.NavigationCompleted += CoreWebView2OnNavigationCompleted;
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


        private async void CoreWebView2OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await ApplyThemingScript();
        }

        private async void CoreWebView2OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            await ApplyThemingScript();
        }

        private void HandleWebViewMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            System.Diagnostics.Debug.WriteLine(message, "Agent HandleWebViewMessage");
            WebViewMessageReceived.Invoke(this, message);
        }

        public async Task PostWebMessageAsJson(string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine(message, "Agent PostWebMessageAsJson");
                _webview.PostWebMessageAsJson(message);
                await _webview.ExecuteScriptWithResultAsync(GetPostMessageScript(message));
            });
        }

        private async Task ApplyVsCodeApiScript()
        {
            await _webview.AddScriptToExecuteOnDocumentCreatedAsync(GetVsCodeApiScript());
        }

        private async Task ApplyThemingScript()
        {
            await _webview.ExecuteScriptWithResultAsync(GetThemeScript(colorThemeScript));
        }

        public void SetHtml(string html)
        {
            // NOTE: Serving the returned html doesn't work in Visual Studio,
            // as it doesn't allow access to the local storage _webview?.NavigateToString(html);

            _webview.Navigate("https://cody.vs/index.html");
        }

        private static string GetVsCodeApiScript() => @"
            globalThis.acquireVsCodeApi = (function() {
                let acquired = false;
                let state = undefined;
                return () => {
                    if (acquired && !false) {
                        throw new Error('An instance of the VS Code API has already been acquired');
                    }
                    acquired = true;
                    return Object.freeze({
                        postMessage: function(message) {
                            console.log(`do-postMessage: ${JSON.stringify(message)}`);
                            window.chrome.webview.postMessage(JSON.stringify(message));
                        },
                        setState: function(newState) {
                            state = newState;
                            console.log(`do-setState: ${JSON.stringify(newState)}`);
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

        private static string GetPostMessageScript(string message) => $@"
            (() => {{
                const event = new CustomEvent('message');
                console.log('PostWebMessageAsJson', {message});
                event.data = {message};
                window.dispatchEvent(event);
            }})()
        ";
    }
}

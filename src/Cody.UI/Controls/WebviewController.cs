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

        private string _html;

        private const string AppOrigin = "https://file.sourcegraphstatic.com";

        public event EventHandler<string> WebViewMessageReceived;

        public WebviewController()
        {
        }

        public async Task<CoreWebView2> InitializeWebView(CoreWebView2 webView)
        {
            webView.NavigateToString("");

            _webview = webView;

            await ApplyVsCodeApiScript();

            SetupEventHandlers();
            ConfigureWebView();
            SetupResourceHandling();

            webView.NavigateToString(_html);
            webView.OpenDevToolsWindow();

            _webview = webView;

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

        private string GetContentType(string filePath)
        {
            if (filePath.EndsWith(".js")) return "Content-Type: text/javascript";
            if (filePath.EndsWith(".css")) return "Content-Type: text/css";
            return "Content-Type: text/html";
        }


        private async void CoreWebView2OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await ApplyInjectionScript();
        }

        private async void CoreWebView2OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            await ApplyInjectionScript();
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

        private async Task ApplyInjectionScript()
        {
            await _webview.ExecuteScriptWithResultAsync(GetDocInjectionScript());
        }

        public void SetHtml(string html)
        {
            _html = html;
            _webview?.NavigateToString(_html);

            var agentIndexFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent", "webviews", "index.html");
            _webview?.Navigate(agentIndexFile);
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

        // TODO: Get this from user theme settings.
        private static string GetDocInjectionScript() => @"
            document.documentElement.dataset.ide = 'VisualStudio';

            const rootStyle = document.documentElement.style;
            rootStyle.setProperty('--vscode-font-family', 'monospace');
            rootStyle.setProperty('--vscode-sideBar-foreground', '#000000');
            rootStyle.setProperty('--vscode-sideBar-background', '#ffffff');
            rootStyle.setProperty('--vscode-editor-font-size', '14px');
            rootStyle.setProperty('--vscode-dropdown-background', '#ffffff');
            rootStyle.setProperty('--vscode-dropdown-foreground', '#000000');
            rootStyle.setProperty('--vscode-input-background', '#ffffff');
            rootStyle.setProperty('--vscode-input-foreground', '#000000');
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

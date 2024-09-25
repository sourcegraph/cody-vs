using Cody.Core.Infrastructure;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cody.UI.Controls
{
    public class CodyWebView : WebView2, ICodyWebView
    {
        private string colorTheme;
        private TaskCompletionSource<bool> webViewReady = new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> chatReady = new TaskCompletionSource<bool>();

        public CodyWebView(string colorTheme)
        {
            Initialized += OnInitialized;
            this.colorTheme = colorTheme;
        }

        private async void OnInitialized(object sender, EventArgs e)
        {
            try
            {
                DefaultBackgroundColor = System.Drawing.Color.Transparent;

                var env = await CreateWebView2Environment();
                await EnsureCoreWebView2Async(env);

                string agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent", "webviews");
                var core = CoreWebView2;
                core.SetVirtualHostNameToFolderMapping("cody.vs", agentDir, CoreWebView2HostResourceAccessKind.Allow);

                core.Settings.AreDefaultScriptDialogsEnabled = true;
                core.Settings.IsWebMessageEnabled = true;
                core.Settings.AreDefaultScriptDialogsEnabled = true;
                core.Settings.AreHostObjectsAllowed = true;
                core.Settings.IsScriptEnabled = true;
                core.Settings.AreBrowserAcceleratorKeysEnabled = true;
                core.Settings.IsGeneralAutofillEnabled = true;
                core.Settings.AreDefaultContextMenusEnabled = false;
                core.Settings.AreDevToolsEnabled = false;
                core.Settings.IsStatusBarEnabled = false;
#if DEBUG
                core.Settings.AreDefaultContextMenusEnabled = true;
                core.Settings.AreDevToolsEnabled = true;
#endif
                core.WebMessageReceived += OnWebMessageReceived;
                core.DOMContentLoaded += OnDOMContentLoaded;

                await core.AddScriptToExecuteOnDocumentCreatedAsync(VsSetupScript);
                core.Navigate("https://cody.vs/index.html");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("CodyWebView initialization error");
            }
        }

        private async void OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            try
            {
                await CoreWebView2.ExecuteScriptAsync(GetThemeScript(colorTheme));
                webViewReady.TrySetResult(true);
            }
            catch { }
        }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            SendWebMessage?.Invoke(this, message);
        }

        public event EventHandler<string> SendWebMessage;

        public void PostWebMessage(string message)
        {
            Dispatcher.Invoke(() => CoreWebView2.PostWebMessageAsJson(message));

            if (message.StartsWith("{\"type\":\"rpc/response\"") && message.EndsWith("\"streamEvent\":\"complete\"}}"))
                chatReady.TrySetResult(true);
        }

        public Task WaitUntilWebViewReady() => webViewReady.Task;

        public Task WaitUntilChatReady() => chatReady.Task;

        public async Task ChangeColorTheme(string colorTheme)
        {
            this.colorTheme = colorTheme;
            await CoreWebView2.ExecuteScriptAsync(GetThemeScript(colorTheme));
        }

        private async Task<CoreWebView2Environment> CreateWebView2Environment()
        {

            var codyDir = "Cody";
#if DEBUG
            codyDir = "Cody\\Debug";
#endif
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), codyDir);
            var options = new CoreWebView2EnvironmentOptions
            {
#if DEBUG
                AdditionalBrowserArguments =
                    "--remote-debugging-port=9222 --disable-web-security --allow-file-access-from-files",
                AllowSingleSignOnUsingOSPrimaryAccount = true,
#endif
            };

            var webView2 = await CoreWebView2Environment.CreateAsync(null, appData, options);
            return webView2;
        }

        private static readonly string VsSetupScript = @"
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
            (function() {{
                document.documentElement.dataset.ide = 'VisualStudio';
                document.documentElement.style = '';
                {colorTheme}
            }})();
        ";
    }
}

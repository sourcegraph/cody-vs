using Cody.Core.Common;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Cody.UI.Controls
{
    public class WebviewController
    {
        private CoreWebView2 _webview;

        private string _colorThemeScript;

        private ICommand _sendMessageCommand;

        private ILog _logger;

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
            _webview.Settings.AreDefaultContextMenusEnabled = Configuration.IsDebug;
            _webview.Settings.AreDevToolsEnabled = Configuration.IsDebug;
            _webview.Settings.IsStatusBarEnabled = false;
        }

        private void SetupEventHandlers()
        {
            _webview.DOMContentLoaded += CoreWebView2OnDOMContentLoaded;
            _webview.WebMessageReceived += HandleWebViewMessage;
            _webview.NewWindowRequested += OnNewWindowRequested;
        }

        private void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            VsShellUtilities.OpenSystemBrowser(e.Uri);
            e.Handled = true;
        }

        private void SetupVirtualHostMapping()
        {
            string agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent", "webviews");
            _webview.SetVirtualHostNameToFolderMapping("cody.vs", agentDir, CoreWebView2HostResourceAccessKind.Allow);
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
            try
            {

                // From agent to webview.
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _webview.PostWebMessageAsJson(message);
                    System.Diagnostics.Debug.WriteLine(message, "Agent PostWebMessageAsJson");
                });
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine(message, "Agent PostWebMessageAsJson task canceled");
            }
            catch (InvalidOperationException ex) when (ex.InnerException is COMException)
            {
                //CoreWebView2 members cannot be accessed after the WebView2 control is disposed.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            catch (AggregateException ex) when (ex.InnerException is InvalidOperationException inex)
            {
                System.Diagnostics.Debug.WriteLine(inex.Message, "AggregateException in PostWebMessageAsJson");
            }
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
        }

        public async void OnThemeChanged(object sender, IColorThemeChangedEvent e)
        {
            try
            {
                if (_webview == null) return;
                string updatedScript = e.ThemingScript;
                if (updatedScript != _colorThemeScript)
                {
                    _logger.Debug("Applying VS theme change to WebView ...");

                    SetThemeScript(updatedScript);
                    await ApplyThemingScript();

                    _logger.Debug("Theme change applied.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Applying theme change to WebView failed.", ex);
            }
        }

        public void SetHtml(string html)
        {
            // NOTE: Serving the returned html doesn't work in Visual Studio,
            // as it doesn't allow access to the local storage _webview?.NavigateToString(html);

            try
            {
                _webview.Navigate("https://cody.vs/index.html");
            }
            catch (InvalidOperationException ex) when (ex.InnerException is COMException)
            {
                //CoreWebView2 members cannot be accessed after the WebView2 control is disposed.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
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
            (function() {{
                document.documentElement.dataset.ide = 'VisualStudio';
                document.documentElement.style = '';
                {colorTheme}
            }})();
        ";

        public void SetLogger(ILog logger)
        {
            _logger = logger;
        }
    }
}

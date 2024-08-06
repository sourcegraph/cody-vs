using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Windows.Input;
using Cody.Core.Agent;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace Cody.UI.Controls
{
    /// <summary>
    /// Interaction logic for WebView2Dev.xaml
    /// </summary>
    public partial class WebView2Dev : UserControl
    {
        private bool _isWebView2Initialized;

        public static CoreWebView2 _webview;
        public static string _html;

        public static CoreWebView2 GetWebview => _webview;

        public event EventHandler<string> WebViewMessageReceived;

        // Receive message from webview and send it to the agent.
        private void HandleWebViewMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();
            System.Diagnostics.Debug.WriteLine(message, "Agent HandleWebViewMessage");

            SendMessage.Execute(message);
        }

        // Send message to webview
        public static async Task PostWebMessageAsJson(string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine(message, "Agent PostWebMessageAsJson");

                string script = $@"
                    (() => {{
                        const event = new CustomEvent('message');
                        console.log('PostMessageCallback', {message});
                        event.data = {message};
                        window.dispatchEvent(event);
                    }})()
                ";

                _webview.PostWebMessageAsJson(message);

                await _webview.ExecuteScriptAsync(script);
            });
        }

        string _vsCodeAPIScript = @"
            globalThis.acquireVsCodeApi = (function() {{
                let acquired = false;
                let state = undefined;

                return () => {{
                    if (acquired && !false) {
                        throw new Error('An instance of the VS Code API has already been acquired');
                    }
                    acquired = true;
                    return Object.freeze({
                        postMessage: function(message, transfer) {
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
                }};
            }})();
        ";

        // TODO: Get color theme from Visual Studio then send it to the webview.
        string _cspScript = $@"
            document.documentElement.dataset.ide = 'VisualStudio';

            const rootStyle = document.documentElement.style;
            rootStyle.setProperty('--vscode-font-family', 'monospace');
            rootStyle.setProperty('--vscode-sideBar-foreground', '#000000');
            rootStyle.setProperty('--vscode-sideBar-background', '#ffffff');
            rootStyle.setProperty('--vscode-editor-font-size', '14px');
        ";

        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register("Html", typeof(string), typeof(WebView2Dev),
                new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var html = e.NewValue as string;
            _html = html;
            _webview?.NavigateToString(html);

            var agentIndexFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent", "webviews", "index.html");
            _webview.Navigate(agentIndexFile);
        }

        public string Html
        {
            get => (string)GetValue(HtmlProperty);
            set => SetValue(HtmlProperty, value);
        }

        public static readonly DependencyProperty PostMessageProperty =
            DependencyProperty.Register("PostMessage", typeof(AgentResponseEvent), typeof(WebView2Dev),
                new PropertyMetadata(null, PostMessageCallback));

        private static async void PostMessageCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var message = e.NewValue as AgentResponseEvent;

            System.Diagnostics.Debug.WriteLine(message.StringEncodedMessage, "Agent PostMessageCallback");

            string script = $@"
                (() => {{
                    const event = new CustomEvent('message');
                    console.log('PostMessageCallback', {message.StringEncodedMessage});
                    event.data = {message.StringEncodedMessage};
                    window.dispatchEvent(event);
                }})()
            ";

            _webview.PostWebMessageAsJson(message.StringEncodedMessage);

            await _webview.ExecuteScriptAsync(script);
        }

        public string PostMessage
        {
            get => (string)GetValue(PostMessageProperty);
            set => SetValue(PostMessageProperty, value);
        }

        public static readonly DependencyProperty SendMessageProperty =
            DependencyProperty.Register(
                "SendMessage",
                typeof(ICommand),
                typeof(WebView2Dev),
                new UIPropertyMetadata(null));
        public ICommand SendMessage
        {
            get { return (ICommand)GetValue(SendMessageProperty); }
            set { SetValue(SendMessageProperty, value); }
        }

        public WebView2Dev()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private void InitWebView2(object sender, RoutedEventArgs e)
        {
            InitializeWebView();
        }

        public static async Task<CoreWebView2> InitializeAsync()
        {
            var webView2Dev = new WebView2Dev();
            await webView2Dev.InitializeWebView();
            return _webview;
        }

        private async Task<CoreWebView2> InitializeWebView()
        {
            try
            {
                if (_webview != null)
                    return _webview;

                webView.CoreWebView2InitializationCompleted += WebViewOnCoreWebView2InitializationCompleted;

                var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Cody");

                var env = await CoreWebView2Environment.CreateAsync(null, appData,
                    new CoreWebView2EnvironmentOptions(null, null, null, false,
                        new List<CoreWebView2CustomSchemeRegistration> { new CoreWebView2CustomSchemeRegistration("") }) // https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/winrt/microsoft_web_webview2_core/corewebview2customschemeregistration?view=webview2-winrt-1.0.1369-prerelease
                    {
#if DEBUG
                        AdditionalBrowserArguments = "--remote-debugging-port=9222 --disable-web-security --allow-file-access-from-files",
                        AllowSingleSignOnUsingOSPrimaryAccount = true,
#endif

                    }

                    );
                await webView.EnsureCoreWebView2Async(env);

                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_vsCodeAPIScript);
                webView.CoreWebView2.NavigateToString("");

                webView.CoreWebView2.DOMContentLoaded += CoreWebView2OnDOMContentLoaded;
                webView.CoreWebView2.NavigationCompleted += CoreWebView2OnNavigationCompleted;
                webView.CoreWebView2.WebMessageReceived += HandleWebViewMessage;


                webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                webView.CoreWebView2.Settings.AreHostObjectsAllowed = true;
                webView.CoreWebView2.Settings.IsScriptEnabled = true;
                webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
                _webview = webView.CoreWebView2;

                webView.CoreWebView2.OpenDevToolsWindow();



                var AppOrigin = "https://file.sourcegraphstatic.com";

                webView.CoreWebView2.AddWebResourceRequestedFilter($"{AppOrigin}*", CoreWebView2WebResourceContext.All);

                webView.CoreWebView2.WebResourceRequested += (s, eventArgs) =>
                {
                    // Replace appOrigin with the file system path to the folder containing the web assets
                    var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");
                    var requestUri = eventArgs.Request.Uri;
                    var uri = new Uri(requestUri);
                    var path = uri.AbsolutePath;
                    var filePath = Path.Combine(agentDir, path.TrimStart('/')).Replace("\\", "/");

                    var contentType = "Content-Type: text/html";
                    if (filePath.EndsWith(".js"))
                    {
                        contentType = "Content-Type: text/javascript";
                    }
                    else if (filePath.EndsWith(".css"))
                    {
                        contentType = "Content-Type: text/css";
                    }

                    if (System.IO.File.Exists(filePath))
                    {
                        var response = System.IO.File.ReadAllBytes(filePath);
                        eventArgs.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(
                            new MemoryStream(response), 200, "OK", contentType);
                    }
                };


                _isWebView2Initialized = true;
                webView.CoreWebView2.NavigateToString(_html);
                return webView.CoreWebView2;
            }
            catch (Exception ex)
            {
                return webView.CoreWebView2;
            }
        }

        private async void CoreWebView2OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await webView.CoreWebView2.ExecuteScriptWithResultAsync(_cspScript);
        }

        private async void CoreWebView2OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            await webView.CoreWebView2.ExecuteScriptWithResultAsync(_cspScript);
        }

        private void WebViewOnCoreWebView2InitializationCompleted(object sender,
            CoreWebView2InitializationCompletedEventArgs e)
        {
            Debug.WriteLine(e.IsSuccess ? "WebView2 initialized." : "WebView2 initialized failed!");
        }
    }
}

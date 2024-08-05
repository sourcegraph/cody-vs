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

        public event EventHandler<string> WebViewMessageReceived;

        private void HandleWebViewMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();

            SendMessage.Execute(message);
        }

        private Task SendMessageToWebview(string message)
        {
            string script = $@"
                (() => {{
                        const event = new CustomEvent('message');
                        console.log('SendMessageToWebview', {message});
                        event.data = {message};
                        window.dispatchEvent(event);
                }})()
            ";
             webView.CoreWebView2.ExecuteScriptAsync(script);

            // Send the message to the webview
            webView.CoreWebView2.PostWebMessageAsJson(message);

            return Task.CompletedTask;
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
        private static readonly string _cspScript = $@"
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

        private static async void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var html = e.NewValue as string;

            await _webview.ExecuteScriptWithResultAsync(_cspScript);

            _webview.NavigateToString(html);
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

            InitializeAsync();
        }

        private void InitWebView2(object sender, RoutedEventArgs e)
        {
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                if (_isWebView2Initialized)
                    return;

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

                webView.CoreWebView2.NavigateToString("");

                _webview = webView.CoreWebView2;

                webView.CoreWebView2.OpenDevToolsWindow();

                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_vsCodeAPIScript);

                webView.CoreWebView2.DOMContentLoaded += CoreWebView2OnDOMContentLoaded;
                webView.CoreWebView2.NavigationCompleted += CoreWebView2OnNavigationCompleted;
                webView.CoreWebView2.WebMessageReceived += HandleWebViewMessage;


                webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                webView.CoreWebView2.Settings.AreHostObjectsAllowed = true;
                webView.CoreWebView2.Settings.IsScriptEnabled = true;
                webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void CoreWebView2OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                string script = @"
                                console.log(globalThis, 'globalThis'); // Should print the global object
                                globalThis.myGlobalVar = 'Hello from globalThis!';
                 ";

                await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void CoreWebView2OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            ;
        }

        private void WebViewOnCoreWebView2InitializationCompleted(object sender,
            CoreWebView2InitializationCompletedEventArgs e)
        {
            Debug.WriteLine(e.IsSuccess ? "WebView2 initialized." : "WebView2 initialized failed!");
        }



        private async void CallJsButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isWebView2Initialized)
                await InitializeAsync();

            webView.ExecuteScriptAsync($"alert('The current date&time is {DateTime.Now:f}')");

            SendMessage.Execute("Message from WebView!");
        }

        private async void Go2MSCopilotButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isWebView2Initialized)
                await InitializeAsync();

           webView.CoreWebView2.NavigateToString("");
            webView.CoreWebView2.DOMContentLoaded += CoreWebView2OnDOMContentLoaded;
                webView.CoreWebView2.NavigationCompleted += CoreWebView2OnNavigationCompleted; //CoreWebView2OnNavigationCompleted;

                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_vsCodeAPIScript);
            await webView.CoreWebView2.ExecuteScriptAsync("console.log('[VSIX]' + document.location);");
        }

        private async void DevToolsOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isWebView2Initialized)
                await InitializeAsync();

            webView.CoreWebView2.OpenDevToolsWindow();
        }
    }
}

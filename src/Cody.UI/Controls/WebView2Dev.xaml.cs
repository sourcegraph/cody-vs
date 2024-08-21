using Cody.Core.Agent;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cody.Core.Logging;

namespace Cody.UI.Controls
{
    /// <summary>
    /// Interaction logic for WebView2Dev.xaml
    /// </summary>
    public partial class WebView2Dev : UserControl
    {
        private static readonly WebviewController _controller = new WebviewController();

        public WebView2Dev()
        {
            InitializeComponent();

            Debug.WriteLine("InitializeComponent", "WebView2Dev");
        }

        private async void InitWebView2(object sender, RoutedEventArgs e)
        {
            await InitializeWebView();
        }

        public static WebviewController InitializeController(string themeScript)
        {
            _controller.SetThemeScript(themeScript);
            return _controller;
        }

        private async Task InitializeWebView()
        {
            try
            {
                if (IsInitialized)
                {
                    Logger?.Debug("Already initialized.");
                    return;
                }

                Logger?.Debug("Initializing ...");

                var env = await CreateWebView2Environment();
                await webView.EnsureCoreWebView2Async(env);
                await _controller.InitializeWebView(webView.CoreWebView2, SendMessage);

                IsInitialized = true;

                Logger?.Debug("Done.");
                Debug.WriteLine("InitializeWebView", "WebView2Dev");
            }
            catch (Exception ex)
            {

                Logger?.Error("Failed.", ex);
            }
            
        }

        private async Task<CoreWebView2Environment> CreateWebView2Environment()
        {

            try
            {
                Logger?.Debug("Initializing ...");

                var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Cody");
                var options = new CoreWebView2EnvironmentOptions
                {
#if DEBUG
                    AdditionalBrowserArguments =
                        "--remote-debugging-port=9222 --disable-web-security --allow-file-access-from-files",
                    AllowSingleSignOnUsingOSPrimaryAccount = true,
#endif
                };

                var webView2 = await CoreWebView2Environment.CreateAsync(null, appData, options);
                Logger?.Debug("Done.");

                return webView2;
            }
            catch (Exception ex)
            {
                Logger?.Error("Failed", ex);
            }

            return null;
        }

        public static async Task PostWebMessageAsJson(string message)
        {
            await _controller.PostWebMessageAsJson(message);
        }

        public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(
                 "Html", typeof(string), typeof(WebView2Dev),
                 new PropertyMetadata(null, async (d, e) =>
                 {
                     _controller.SetHtml(e.NewValue as string);
                 }));

        public string Html
        {
            get => (string)GetValue(HtmlProperty);
            set => SetValue(HtmlProperty, value);
        }

        public static readonly DependencyProperty PostMessageProperty = DependencyProperty.Register(
            "PostMessage", typeof(AgentResponseEvent), typeof(WebView2Dev),
            new PropertyMetadata(null, async (d, e) =>
            {
                var message = (e.NewValue as AgentResponseEvent)?.StringEncodedMessage;
                if (!string.IsNullOrEmpty(message)) await _controller.PostWebMessageAsJson(message);
            }));


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
            get => (ICommand)GetValue(SendMessageProperty);
            set => SetValue(SendMessageProperty, value);
        }

        public static readonly DependencyProperty LoggerProperty = DependencyProperty.Register(
            "Logger", typeof(ILog), typeof(WebView2Dev),
            new PropertyMetadata(null, async (d, e) =>
            {
                var logger = e.NewValue as ILog;
                if (logger != null)
                {
                    logger.Debug("Logger set.");
                }
            }));

        public ILog Logger
        {
            get => (ILog)GetValue(LoggerProperty);
            set => SetValue(LoggerProperty, value);
        }

        public static readonly DependencyProperty IsInitializedProperty = DependencyProperty.Register(
            "IsInitialized", typeof(bool), typeof(WebView2Dev),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                )
            );

        public bool IsInitialized
        {
            get => (bool)GetValue(IsInitializedProperty);
            set => SetValue(IsInitializedProperty, value);
        }
    }
}

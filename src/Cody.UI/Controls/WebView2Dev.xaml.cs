using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using System.IO;
using Cody.Core.Logging;

namespace Cody.UI.Controls
{
    /// <summary>
    /// Interaction logic for WebView2Dev.xaml
    /// </summary>
    public partial class WebView2Dev : UserControl
    {
        private bool _isWebView2Initialized;

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
                    new CoreWebView2EnvironmentOptions()
                    {
#if DEBUG
                        AdditionalBrowserArguments = "--remote-debugging-port=9222",
#endif
                    }

                    );
                await webView.EnsureCoreWebView2Async(env);

                webView.Source = new Uri("https://html5test.co");

                _isWebView2Initialized = true;
            }
            catch (Exception ex)
            {
            }
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
        }

        private async void Go2MSCopilotButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isWebView2Initialized)
                await InitializeAsync();

            // Javascript call

            await webView.CoreWebView2.ExecuteScriptAsync("console.log('[VSIX] ' + document.location);");
            await webView.CoreWebView2.ExecuteScriptAsync("document.location.href = 'https://copilot.microsoft.com/';");

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

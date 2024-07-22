using System;
using System.Collections.Generic;
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
                    new CoreWebView2EnvironmentOptions(null, null, null, false, 
                        new List<CoreWebView2CustomSchemeRegistration> { new CoreWebView2CustomSchemeRegistration("") }) // https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/winrt/microsoft_web_webview2_core/corewebview2customschemeregistration?view=webview2-winrt-1.0.1369-prerelease
                    {
#if DEBUG
                        AdditionalBrowserArguments = "--remote-debugging-port=9222 --disable-web-security --allow-file-access-from-files",


#endif
                    }

                    );
                await webView.EnsureCoreWebView2Async(env);
                webView.CoreWebView2.DOMContentLoaded += CoreWebView2OnDOMContentLoaded;

                // load file from disk E:\Sigmaloc\Sourcegraph\cody-vs-clean\src\Cody.VisualStudio\Agent\webviews
                webView.CoreWebView2.Navigate("file:///E:/Sigmaloc/Sourcegraph/cody-vs-clean/src/Cody.VisualStudio/Agent/webviews/index.html");

                //webView.Source = new Uri("https://html5test.co");

                _isWebView2Initialized = true;
            }
            catch (Exception ex)
            {
            }
        }

        private async void CoreWebView2OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            try
            {

                string csp =
                    "default-src 'none'; img-src {cspSource} https: data:; script-src {cspSource}; style-src {cspSource}; font-src data: {cspSource};";
                string cspScript = $@"
            var meta = document.createElement('meta');
            meta.httpEquiv = 'Content-Security-Policy';
            meta.content = '{csp}';
            document.getElementsByTagName('head')[0].appendChild(meta);
        ";

                await webView.CoreWebView2.ExecuteScriptAsync(cspScript);
            }
            catch (Exception ex)
            {
                ;
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


            // load file from disk E:\Sigmaloc\Sourcegraph\cody-vs-clean\src\Cody.VisualStudio\Agent\webviews
            webView.CoreWebView2.Navigate("file:///E:/Sigmaloc/Sourcegraph/cody-vs-clean/src/Cody.VisualStudio/Agent/webviews/index.html");

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

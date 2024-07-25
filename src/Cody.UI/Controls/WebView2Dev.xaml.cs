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

        private string _vsCodeAPIScript = @"

console.log('CodeAPIScript.1'); 

                    globalThis.acquireVsCodeApi = (function() {{
          let acquired = false;
          let state = undefined;
          return () => {{
              if (acquired && !false) {{
                  throw new Error('An instance of the VS Code API has already been acquired');
              }}
              acquired = true;
              return Object.freeze({
								postMessage: function(message, transfer) {
                                    console.log(message);
									//console.log(message + '|' + transfer);
								},
								setState: function(newState) {
									state = newState;
									doPostMessage('do-update-state', JSON.stringify(newState));
									return newState;
								},
								getState: function() {
									return state;
								}
							});
          }};
      }})();
      delete window.parent;
      delete window.top;
      delete window.frameElement;

console.log('CodeAPIScript.2'); 
                ";

        static string _csp =
            "default-src 'none'; img-src {cspSource} https: data:; script-src {cspSource}; style-src {cspSource}; font-src data: {cspSource};";
        string _cspScript = $@"
console.log('csp.1.1'); 
            var meta = document.createElement('meta');
            meta.httpEquiv = 'Content-Security-Policy';
            meta.content = '{_csp}';
            document.getElementsByTagName('head')[0].appendChild(meta);
            
console.log('csp.1.2'); 
        ";

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
                webView.CoreWebView2.NavigationCompleted += CoreWebView2OnNavigationCompleted; //CoreWebView2OnNavigationCompleted;

                //var result = await webView.CoreWebView2.ExecuteScriptWithResultAsync(_vsCodeAPIScript);

                // load file from disk E:\Sigmaloc\Sourcegraph\cody-vs-clean\src\Cody.VisualStudio\Agent\webviews
                //var result = await webView.CoreWebView2.ExecuteScriptWithResultAsync(_vsCodeAPIScript);

                //await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_cspScript);
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_vsCodeAPIScript);
                webView.CoreWebView2.Navigate("file:///E:/Sigmaloc/Sourcegraph/cody-vs-clean/src/Cody.VisualStudio/Agent/webviews/index.html");
                webView.CoreWebView2.OpenDevToolsWindow();

                //webView.Source = new Uri("https://html5test.co");

                _isWebView2Initialized = true;
            }
            catch (Exception ex)
            {
            }
        }

        private async void CoreWebView2OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                string script = @"
                                console.log(globalThis); // Should print the global object
                                globalThis.myGlobalVar = 'Hello from globalThis!';
                                console.log(globalThis.myGlobalVar); // Should print 'Hello from globalThis!'
                 ";

                await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                ;
            }
        }

        private async void CoreWebView2OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            try
            {



                var vsCodeAPIScript = @"
console.log('test2.1'); 

                    globalThis.acquireVsCodeApi = (function() {{
          let acquired = false;
          let state = undefined;
          return () => {{
              if (acquired && !false) {{
                  throw new Error('An instance of the VS Code API has already been acquired');
              }}
              acquired = true;
              
          }};
      }})();
      delete window.parent;
      delete window.top;
      delete window.frameElement;
console.log('test2.2'); 
                ";

                //await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(vsCodeAPIScript);


                //var result = await webView.CoreWebView2.ExecuteScriptWithResultAsync(vsCodeAPIScript);
                ;
                //await webView.CoreWebView2.ExecuteScriptAsync(vsCodeAPIScript);


                var result = await webView.CoreWebView2.ExecuteScriptWithResultAsync(_cspScript);

                ;
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

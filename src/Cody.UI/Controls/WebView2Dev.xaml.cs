﻿using System;
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
            globalThis.acquireVsCodeApi = (function() {{
                let acquired = false;
                let state = undefined;

                const doPostMessage = (message, transfer) => {{
                    window.parent.postMessage(message, transfer);
                }}

                return () => {{
                    if (acquired && !false) {{
                        throw new Error('An instance of the VS Code API has already been acquired');
                    }}
                    acquired = true;
                    return Object.freeze({
                        postMessage: function(message, transfer) {
                            
                            console.assert(!transfer);
                            console.log(JSON.stringify({ message, transfer }), 'post-message');
                            window.parent.postMessage(message, transfer);
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
        ";

        string _cspScript = $@"
            document.documentElement.dataset.ide = 'VisualStudio';

            const rootStyle = document.documentElement.style;
            rootStyle.setProperty('--vscode-font-family', 'monospace');
            rootStyle.setProperty('--vscode-sideBar-foreground', '#000000');
            rootStyle.setProperty('--vscode-sideBar-background', '#ffffff');
            rootStyle.setProperty('--vscode-editor-font-size', '14px');
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
                webView.CoreWebView2.Navigate("file:///C://Users/BeatrixW/Dev/vs/src/Cody.VisualStudio/Agent/webviews/index.html");
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
                                console.log(globalThis, 'globalThis'); // Should print the global object
                                globalThis.myGlobalVar = 'Hello from globalThis!';
                                console.log(globalThis.myGlobalVar); // Should print 'Hello from globalThis!'
                 ";

                await webView.CoreWebView2.ExecuteScriptAsync(script);
                await webView.CoreWebView2.ExecuteScriptWithResultAsync(_cspScript);
            }
            catch (Exception ex)
            {
                ;
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
        }

        private async void Go2MSCopilotButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isWebView2Initialized)
                await InitializeAsync();

            // Javascript call


            // load file from disk E:\Sigmaloc\Sourcegraph\cody-vs-clean\src\Cody.VisualStudio\Agent\webviews
            webView.CoreWebView2.Navigate("file:///C://Users/BeatrixW/Dev/vs/src/Cody.VisualStudio/Agent/webviews/index.html");
            
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

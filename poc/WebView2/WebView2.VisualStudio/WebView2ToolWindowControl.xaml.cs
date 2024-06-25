using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace WebView2.VisualStudio
{
    /// <summary>
    /// Interaction logic for WebView2ToolWindowControl.
    /// </summary>
    public partial class WebView2ToolWindowControl : UserControl
    {

        private bool _isWebView2Initialized;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2ToolWindowControl"/> class.
        /// </summary>
        public WebView2ToolWindowControl()
        {
            this.InitializeComponent();
        }

        private void InitWebView2(object sender, RoutedEventArgs e)
        {
            InitializeAsync();
        }

        async Task InitializeAsync()
        {
            try
            {
                var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cody");
                var env = await CoreWebView2Environment.CreateAsync(null, appData);
                await webView.EnsureCoreWebView2Async(env);

                _isWebView2Initialized = true;
            }
            catch (Exception ex)
            {
            }
        }

        async void CallJsButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isWebView2Initialized)
                await InitializeAsync();

            webView.ExecuteScriptAsync($"alert('The current date&time is {DateTime.Now:f}')");
        }

        async void Go2MSCopilotButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isWebView2Initialized)
                await InitializeAsync();

            // Javascript call
            
            await webView.CoreWebView2.ExecuteScriptAsync("console.log('[VSIX] ' + document.location);");
            await webView.CoreWebView2.ExecuteScriptAsync("document.location.href = 'https://copilot.microsoft.com/';");

            await webView.CoreWebView2.ExecuteScriptAsync("console.log('[VSIX]' + document.location);");

        }

        async void DevToolsOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isWebView2Initialized)
                await InitializeAsync();

            webView.CoreWebView2.OpenDevToolsWindow();
        }
        
    }
}
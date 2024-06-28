using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using System.IO;

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
        }
        private void InitWebView2(object sender, RoutedEventArgs e)
        {
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cody");
                var env = await CoreWebView2Environment.CreateAsync(null, appData);
                await webView.EnsureCoreWebView2Async(env);

                webView.Source = new Uri("https://html5test.co");

                _isWebView2Initialized = true;
            }
            catch (Exception ex)
            {
            }
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

using Microsoft.Web.WebView2.Wpf;
using System.Windows.Input;

namespace Cody.UI.Controls
{
    public class CodyWebView2 : WebView2CompositionControl
    {
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // prevents WebView2 from handling custom key events (like Home/Enter keys not working in the Cody Chat during prompt editing)
        }
    }
}

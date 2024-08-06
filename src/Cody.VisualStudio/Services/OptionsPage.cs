using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Cody.VisualStudio
{
    public class OptionsPage : DialogPage
    {
        [Category("Authentication")]
        [DisplayName("Access Token")]
        [Description("Paste your access token. To create an access token, go to 'Settings' and then 'Access tokens' on the Sourcegraph instance.")]
        public string TokenKey { get; set; }

        [Category("Authentication")]
        [DisplayName("Sourcegraph URL")]
        [Description("Enter the URL of the Sourcegraph instance. For example, https://sourcegraph.example.com.")]
        public string Endpoint { get; set; }
    }
}
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Cody.VisualStudio
{
    public class OptionsPage : DialogPage
    {
        [Category("Authentication")]
        [DisplayName("Sourcegraph URL")]
        [Description("The URL of the Sourcegraph instance you are connecting to.")]
        public string Endpoint { get; set; }

        [Category("Configurations")]
        [DisplayName("Extension Settings")]
        [Description("[Current Not Supported] Your Cody extension settings in JSON format.")]
        public string Settings { get; set; }
    }
}

using System.Windows.Controls;

namespace Cody.UI.Controls.Options
{
    /// <summary>
    /// Interaction logic for GeneralOptionsControl.xaml
    /// </summary>
    public partial class GeneralOptionsControl : UserControl
    {
        public GeneralOptionsControl()
        {
            InitializeComponent();
        }

        public void ForceBindingsUpdate()
        {
            AccessTokenTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            SourcegraphUrlTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}

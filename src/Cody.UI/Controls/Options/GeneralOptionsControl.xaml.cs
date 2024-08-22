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
            // TextBox binding doesn't work when Visual Studio closes Options window
            // This is a workaround to get bindings updated. The second solution is to use NotifyPropertyChange for every TextBox in the Xaml, but current solution is a little more "clean" - everything is clearly visible in a one place.
            AccessTokenTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            SourcegraphUrlTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}

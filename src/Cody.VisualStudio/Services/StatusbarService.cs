using Cody.Core.Infrastructure;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cody.VisualStudio.Services
{
    public class StatusbarService : IStatusbarService
    {
        private static bool animationIsRunning = false;
        private object icon = (short)Constants.SBAI_General;

        private const string resourcePath = "/Cody.VisualStudio;Component/Resources/";
        private BitmapImage availableIcon = new BitmapImage(new Uri(resourcePath + "SourcegraphWhite.png", UriKind.Relative));
        private BitmapImage unavailableIcon = new BitmapImage(new Uri(resourcePath + "SourcegraphWhiteError.png", UriKind.Relative));
        private BitmapImage waitIcon = new BitmapImage(new Uri(resourcePath + "Wait.png", UriKind.Relative));
        Image codyIcon;
        StackPanel stackPanel;
        TextBlock textBlock;

        public void SetText(string text)
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var statusBar = (IVsStatusbar)Package.GetGlobalService(typeof(SVsStatusbar));
                int frozen;

                statusBar.IsFrozen(out frozen);

                if (frozen != 0) statusBar.FreezeOutput(0);

                statusBar.SetText(text);
                return Task.CompletedTask;
            });


        }

        public void StartProgressAnimation()
        {
            if (animationIsRunning) return;
            if (EnableProgressAnimation(true)) animationIsRunning = true;
        }

        public void StopProgressAnimation()
        {
            if (!animationIsRunning) return;
            if (EnableProgressAnimation(false)) animationIsRunning = false;
        }

        private bool EnableProgressAnimation(bool enable)
        {
            return ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var statusBar = (IVsStatusbar)Package.GetGlobalService(typeof(SVsStatusbar));

                return statusBar.Animation(enable ? 1 : 0, ref icon) == VSConstants.S_OK;
            });
        }

        private void InitializeCodyIcon()
        {
            stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 5, 0),
            };

            codyIcon = new Image()
            {
                MaxHeight = 16,
                MaxWidth = 16,
                Margin = new Thickness(0, 0, 5, 0)
            };

            textBlock = new TextBlock()
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            SetTextBlockForegroundColor();
            VSColorTheme.ThemeChanged += (args) => SetTextBlockForegroundColor();

            stackPanel.Children.Add(codyIcon);
            stackPanel.Children.Add(textBlock);

            DockPanel.SetDock(stackPanel, Dock.Right);
            stackPanel.MouseUp += (sender, args) => CodyStatusIconClicked?.Invoke(codyIcon, EventArgs.Empty);

            var resizeGripControl = Utilities.UIHelper.FindChild<Control>(Application.Current.MainWindow, "ResizeGripControl");
            var dockPanel = resizeGripControl.Parent as DockPanel;
            dockPanel.Children.Insert(2, stackPanel);
        }

        private void SetTextBlockForegroundColor()
        {
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.StatusBarDefaultTextColorKey);
            var textColor = Color.FromArgb(color.A, color.R, color.G, color.B);
            if (textBlock != null) textBlock.Foreground = new SolidColorBrush(textColor);
        }

        public event EventHandler CodyStatusIconClicked;

        public void SetCodyStatus(CodyStatus status, string tooltip = null, string text = null)
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (stackPanel == null) InitializeCodyIcon();

                textBlock.Text = text;
                stackPanel.ToolTip = tooltip;

                switch (status)
                {
                    case CodyStatus.Loading:
                        codyIcon.Source = waitIcon;
                        stackPanel.Visibility = Visibility.Visible;
                        break;
                    case CodyStatus.Available:
                        codyIcon.Source = availableIcon;
                        stackPanel.Visibility = Visibility.Visible;
                        break;
                    case CodyStatus.Unavailable:
                        codyIcon.Source = unavailableIcon;
                        stackPanel.Visibility = Visibility.Visible;
                        break;
                    case CodyStatus.Hide:
                    default:
                        stackPanel.Visibility = Visibility.Hidden;
                        break;
                }

                return Task.CompletedTask;
            });
        }
    }
}

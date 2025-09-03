using Cody.Core.Agent.Protocol;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.UI.ViewModels;
using Cody.UI.Views;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Cody.VisualStudio.Services
{
    public class ToastNotificationService : IToastNotificationService
    {
        public ToastNotificationService(ILog log)
        {
            this.log = log;
        }

        private static ToastView currentView = null;
        private readonly ILog log;

        public async Task<string> ShowNotification(SeverityEnum severity, string message, string details, IEnumerable<string> actions)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (currentView != null) currentView.Close();

                var actionsList = actions != null ? actions.Where(x => !string.IsNullOrEmpty(x)) : new List<string>();

                var result = new TaskCompletionSource<string>();
                currentView = new ToastView();
                var vm = new ToastViewModel(currentView, severity, message, details, actionsList);

                currentView.DataContext = vm;
                currentView.Closed += (sender, e) =>
                {
                    currentView = null;
                    result.SetResult(vm.SelectedAction);
                };

                currentView.Owner = Application.Current.MainWindow;
                currentView.Show();
                currentView.PositionWindow();

                return await result.Task;
            }
            catch (Exception ex)
            {
                log.Error("Show notification error", ex);
                return null;
            }
        }
    }
}

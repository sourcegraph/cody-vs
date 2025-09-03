using Cody.Core.Agent.Protocol;
using Cody.UI.MVVM;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Cody.UI.ViewModels
{
    public class ToastViewModel : NotifyPropertyChangedBase
    {
        private Window window;

        public ToastViewModel(Window window, SeverityEnum severity, string message, string details, IEnumerable<string> actions)
        {
            this.window = window;
            Severity = severity;
            Message = message;
            Details = details;
            Actions = new ObservableCollection<string>(actions);
        }

        public ObservableCollection<string> Actions { get; set; }

        private string message;
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        private string details;
        public string Details
        {
            get => details;
            set => SetProperty(ref details, value);
        }

        private SeverityEnum severity;
        public SeverityEnum Severity
        {
            get => severity;
            set => SetProperty(ref severity, value);
        }

        private DelegateCommand<string> actionCommand;
        public DelegateCommand<string> ActionCommand
        {
            get { return actionCommand = actionCommand ?? new DelegateCommand<string>(OnActionButtonClick); }
        }

        private void OnActionButtonClick(string actionName)
        {
            SelectedAction = actionName;
            window.Close();
        }

        private DelegateCommand closeCommand;
        public DelegateCommand CloseCommand
        {
            get { return closeCommand = closeCommand ?? new DelegateCommand(OnCloseButtonClick); }
        }

        private void OnCloseButtonClick() => window.Close();

        public ImageMoniker Moniker
        {
            get
            {
                switch (severity)
                {
                    case SeverityEnum.Error: return KnownMonikers.StatusError;
                    case SeverityEnum.Warning: return KnownMonikers.StatusWarning;
                    case SeverityEnum.Information:
                    default:
                        return KnownMonikers.StatusInformation;
                }
            }
        }

        public string SelectedAction { get; protected set; }
    }
}

using Cody.Core.Agent;
using Cody.Core.Agent.Connector;
using Cody.Core.Logging;
using Cody.UI.MVVM;

namespace Cody.UI.ViewModels
{
    public class MainViewModel: NotifyPropertyChangedBase
    {
        public readonly NotificationHandlers NotificationHandlers;

        private readonly ILog _logger;

        public MainViewModel(NotificationHandlers notificationHandlers, ILog logger)
        {
            NotificationHandlers = notificationHandlers;
            _logger = logger;

            NotificationHandlers.OnSetHtmlEvent += OnSetHtmlHandler;
            NotificationHandlers.OnWebviewMessageEvent += OnWebviewRequestHandler;

            _logger.Debug("Initialized.");
        }

        private async void OnWebviewRequestHandler(object sender, SetWebviewRequestEvent e)
        {
            NotificationHandlers.SendWebviewMessage(e.Handle, e.Messsage);
        }

        private void OnSetHtmlHandler(object sender, SetHtmlEvent e)
        {
            Html = e.Html;
        }

        private string _html;

        public string Html
        {
            get
            {
                return _html;
            } 
            set
            {
                SetProperty(ref _html, value);
            }
        }
    }
}

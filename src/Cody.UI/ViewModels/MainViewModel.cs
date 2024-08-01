using Cody.Core.Agent;
using Cody.Core.Agent.Connector;
using Cody.Core.Logging;
using Cody.UI.MVVM;
using System.Windows.Input;

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
            NotificationHandlers.OnPostMessageEvent += OnPostMessageHandler;

            _logger.Debug("Initialized.");
        }

        private void OnPostMessageHandler(object sender, AgentResponseEvent e)
        {
            PostMessage = new AgentResponseEvent()
            {
                Id = e.Id, StringEncodedMessage = e.StringEncodedMessage

            };
        }

        private async void OnWebviewRequestHandler(object sender, SetWebviewRequestEvent e)
        {
            NotificationHandlers.SendWebviewMessage(e.Handle, e.Messsage);
        }

        public ICommand WebviewMessageSendCommand
        {
            get { return new DelegateCommand<object>(WebviewSendMessage); }
        }


        private void WebviewSendMessage(object message)
        {
            NotificationHandlers.SendWebviewMessage("visual-studio-cody", (string)message);
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

        private AgentResponseEvent _postMessage;

        public AgentResponseEvent PostMessage
        {
            get
            {
                return _postMessage;
            }
            set
            {
                SetProperty(ref _postMessage, value);
            }
        }
    }
}

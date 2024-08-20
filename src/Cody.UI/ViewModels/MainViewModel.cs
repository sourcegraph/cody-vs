using Cody.Core.Agent;
using Cody.Core.Logging;
using Cody.UI.MVVM;
using System.Windows.Input;
using System.Windows.Media;

namespace Cody.UI.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        public readonly NotificationHandlers NotificationHandlers;

        private readonly ILog _logger;

        public MainViewModel(NotificationHandlers notificationHandlers, ILog logger, Brush textColor)
        {
            NotificationHandlers = notificationHandlers;
            _logger = logger;
            _textColor = textColor;

            NotificationHandlers.OnSetHtmlEvent += OnSetHtmlHandler;
            NotificationHandlers.OnPostMessageEvent += OnPostMessageHandler;

            _logger.Debug("MainViewModel Initialized.");
        }

        private void OnPostMessageHandler(object sender, AgentResponseEvent e)
        {
            PostMessage = new AgentResponseEvent()
            {
                Id = e.Id,
                StringEncodedMessage = e.StringEncodedMessage
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
            NotificationHandlers.SendWebviewMessage("visual-studio-sidebar", (string)message);
        }

        private void OnSetHtmlHandler(object sender, SetHtmlEvent e)
        {
            Html = e.Html;
        }

        private Brush _textColor;
        public Brush TextColor
        {
            get
            {
                return _textColor;
            }
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
                if (SetProperty(ref _html, value))
                {
                    _logger.Debug($"{_html}");
                }
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
                if (SetProperty(ref _postMessage, value))
                {
                    _logger.Debug($"{_postMessage.StringEncodedMessage}");
                }
            }
        }

        public ILog Logger => _logger;
    }
}

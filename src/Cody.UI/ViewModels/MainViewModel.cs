using System;
using System.Threading.Tasks;
using System.Windows;
using Cody.Core.Agent;
using Cody.Core.Logging;
using Cody.UI.MVVM;
using System.Windows.Input;
using Cody.Core.Infrastructure;
using System.Windows.Media;

namespace Cody.UI.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase, IWebChatHost
    {
        private readonly IWebViewsManager _webViewsManager;
        public readonly NotificationHandlers NotificationHandlers;

        private readonly ILog _logger;

        public MainViewModel(IWebViewsManager webViewsManager, NotificationHandlers notificationHandlers, Brush textColor, ILog logger)
        {
            _webViewsManager = webViewsManager;
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
            get { return _textColor; }
        }

        private string _html;

        public string Html
        {
            get { return _html; }
            set
            {
                if (SetProperty(ref _html, value))
                {
                    _logger.Debug($"{_html}");
                }
            }
        }

        private string _isChatLoaded;
        public string IsChatLoaded
        {
            get => _isChatLoaded;
            set
            {
                if (SetProperty(ref _isChatLoaded, value))
                {
                    _logger.Debug($"IsChatLoaded:{_isChatLoaded}");
                }
            }
        }

        private AgentResponseEvent _postMessage;
        public AgentResponseEvent PostMessage
        {
            get { return _postMessage; }
            set
            {
                if (SetProperty(ref _postMessage, value))
                {
                    _logger.Debug($"{_postMessage.StringEncodedMessage}");
                }
            }
        }

        public ILog Logger
        {
            get { return _logger; }
        }

        private bool _isWebViewInitialized;
        public bool IsWebViewInitialized
        {
            get { return _isWebViewInitialized; }
            set
            {
                _isWebViewInitialized = value;
                if (_isWebViewInitialized)
                {
                    _logger.Debug("WebChatHost initialized.");


                    _webViewsManager.Register(this);
                }
            }

        }
    }
}

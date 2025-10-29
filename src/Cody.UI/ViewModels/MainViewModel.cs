using Cody.Core.Agent;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.UI.MVVM;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cody.UI.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase, IWebChatHost
    {
        private readonly IWebViewsManager _webViewsManager;
        public readonly WebviewNotificationHandlers NotificationHandlers;

        private readonly ILog _logger;

        public MainViewModel(IWebViewsManager webViewsManager, WebviewNotificationHandlers notificationHandlers, ILog logger)
        {
            _webViewsManager = webViewsManager;
            NotificationHandlers = notificationHandlers;
            _logger = logger;

            NotificationHandlers.OnSetHtmlEvent += OnSetHtmlHandler;

            _logger.Debug("MainViewModel Initialized.");
        }

        private void OnWebviewRequestHandler(object sender, SetWebviewRequestEvent e)
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
            Html = e;
        }

        private SetHtmlEvent _html;

        public SetHtmlEvent Html
        {
            get { return _html; }
            set
            {
                if (SetProperty(ref _html, value))
                {
                    _logger.Debug($"handle: '{_html.Handle}'");
                    _logger.Debug($"handle: '{_html.Html}'");
                }
            }
        }

        private bool _isChatLoaded;
        public bool IsChatLoaded
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

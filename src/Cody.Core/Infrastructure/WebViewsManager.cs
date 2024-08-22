using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Logging;

namespace Cody.Core.Infrastructure
{
    public interface IWebViewsManager
    {
        void Register(IWebChatHost chatHost);
        void SetAgentService(IAgentService agentService);
    }

    public enum WebViewsEventTypes
    {
        RegisterWebViewRequest,
        WebChatHostInitialized
    }

    public class WebViewsManager : IWebViewsManager, IDisposable
    {
        private readonly IAgentProxy _agentProxy;
        private IAgentService _agentService;
        private readonly INotificationHandler _notificationHandler;
        private readonly ILog _logger;

        private List<IWebChatHost> _chatHosts;

        private readonly BlockingCollection<WebViewsEventTypes> _events; //TODO: when custom editors will be introduced, make it richer, like BlockingCollection<WebViewsEvents>, where WebViewsEvents will be a class

        public WebViewsManager(IAgentProxy agentProxy, INotificationHandler notificationHandler, ILog logger)
        {
            _agentProxy = agentProxy;
            _notificationHandler = notificationHandler;
            _logger = logger;

            _chatHosts = new List<IWebChatHost>();
            _events = new BlockingCollection<WebViewsEventTypes>();

            _notificationHandler.OnRegisterWebViewRequest += OnRegisterWebViewRequestHandler;

            Task.Run(ProcessEvents);
        }

        public void SetAgentService(IAgentService agentService)
        {
            _agentService = agentService;
        }

        private async Task ProcessEvents()
        {
            try
            {
                _logger.Debug("Started ...");
                while (true)
                {
                    var e = _events.Take(); // blocks until an item is available to be removed

                    _logger.Debug($"Processing event '{e}' ...");
                    if (e == WebViewsEventTypes.RegisterWebViewRequest
                        || e == WebViewsEventTypes.WebChatHostInitialized

                        )
                    {
                        // check if there is IWebChatHost available
                        var chatHost = _chatHosts.FirstOrDefault(); // TODO: modify when introducing custom editors
                        if (chatHost != null)
                        {
                            _logger.Debug("Chat Host present.");

                            if (chatHost.IsWebViewInitialized)
                            {
                                _logger.Debug("Chat Host initialized.");

                                try
                                {
                                    if (!_agentProxy.IsConnected)
                                    {
                                        _logger.Debug("Agent not connected.");
                                        continue;
                                    }

                                    await Task.Delay(TimeSpan.FromSeconds(1)); // HACK: TODO: IAgentProxy.IsConnected is not reliable
                                    await _agentService.ResolveWebviewView(new ResolveWebviewViewParams
                                    {
                                        // cody.chat for sidebar view, or cody.editorPanel for editor panel
                                        // TODO support custom editors
                                        ViewId = "cody.chat",
                                        WebviewHandle = "visual-studio-sidebar",
                                    });

                                    _logger.Debug("ResolveWebviewView() called.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error("Calling ResolveWebviewView() failed.", ex);

                                }
                            }
                            else
                            {
                                _logger.Debug($"{chatHost} not initialized.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug("Processing events queue stopped.", ex);
            }
        }

        private void OnRegisterWebViewRequestHandler(object sender, string viewId)
        {
            try
            {
                _events.Add(WebViewsEventTypes.RegisterWebViewRequest);

                _logger.Debug($"Registered WebView:'{viewId}' request.");

            }
            catch (Exception ex)
            {
                _logger.Error("Failed.", ex);
            }

        }

        public void Register(IWebChatHost chatHost)
        {
            if (!chatHost.IsWebViewInitialized)
            {
                // run-time guard
                throw new Exception("IWebChatHost must be initialized before registering!");
            }

            _chatHosts.Add(chatHost);
            _events.Add(WebViewsEventTypes.WebChatHostInitialized);
        }

        public void Dispose()
        {
            _events?.Dispose();
        }
    }
    
}

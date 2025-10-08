using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

    public class WebViewEvent
    {
        public WebViewEvent(WebViewsEventTypes type, string viewId = null)
        {
            Type = type;
            ViewId = viewId;
        }

        public WebViewsEventTypes Type { get; }
        public string ViewId { get; }
    }

    public class WebViewsManager : IWebViewsManager, IDisposable
    {
        private readonly IAgentProxy _agentProxy;
        private IAgentService _agentService;
        private readonly WebviewNotificationHandlers _notificationHandler;
        private readonly ILog _logger;

        private readonly List<IWebChatHost> _chatHosts;
        private readonly List<WebViewEvent> _processedWebViewsRequests;

        private readonly TimeSpan _agentInitializationTimeout = TimeSpan.FromMinutes(1);

        private readonly BlockingCollection<WebViewEvent> _events; //TODO: when custom editors will be introduced, make it richer, like BlockingCollection<WebViewsEvents>, where WebViewsEvents will be a class

        public WebViewsManager(IAgentProxy agentProxy, WebviewNotificationHandlers notificationHandler, ILog logger)
        {
            _agentProxy = agentProxy;
            _notificationHandler = notificationHandler;
            _logger = logger;

            _chatHosts = new List<IWebChatHost>();
            _events = new BlockingCollection<WebViewEvent>();
            _processedWebViewsRequests = new List<WebViewEvent>();

            _agentProxy.AgentDisconnected += OnAgentDisconnected;
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

                    _logger.Debug($"Processing event '{e.Type}' ...");

                    _processedWebViewsRequests.Add(e);
                    _logger.Debug($"{e.Type}:{e.ViewId} added to processed collection.");

                    var isRegisterWebViewRequestProcessed = _processedWebViewsRequests.Exists(w => w.Type == WebViewsEventTypes.RegisterWebViewRequest);
                    var isWebChatHostInitialized = _processedWebViewsRequests.Exists(w => w.Type == WebViewsEventTypes.WebChatHostInitialized);
                    if (isRegisterWebViewRequestProcessed && isWebChatHostInitialized)
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
                                    await WaitForAgentInitialization();
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

        private void OnAgentDisconnected(object sender, int e)
        {
            _agentService = null;
            _processedWebViewsRequests.RemoveAll(r => r.Type == WebViewsEventTypes.RegisterWebViewRequest);

            _logger.Debug("Cleared old agent service reference.");
        }

        private async Task WaitForAgentInitialization()
        {
            var startTime = DateTime.Now;
            while (!_agentProxy.IsInitialized || _agentService == null)
            {
                _logger.Debug("Waiting for Agent initialization ...");
                await Task.Delay(TimeSpan.FromSeconds(1));

                var nowTime = DateTime.Now;
                var currentSpan = nowTime - startTime;
                if (currentSpan >= _agentInitializationTimeout && !Debugger.IsAttached)
                {
                    var message = $"Agent initialization timeout! Waiting for more than {currentSpan.TotalSeconds} s.";
                    _logger.Error(message);

                    throw new Exception(message);
                }
            }

            _logger.Debug($"IsInitialized:{_agentProxy.IsInitialized} AgentService:{_agentService}");
        }

        private void OnRegisterWebViewRequestHandler(object sender, string viewId)
        {
            try
            {
                _events.Add(new WebViewEvent(WebViewsEventTypes.RegisterWebViewRequest, viewId));

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
            _events.Add(new WebViewEvent(WebViewsEventTypes.WebChatHostInitialized));
        }

        public void Dispose()
        {
            _events?.Dispose();
        }
    }

}

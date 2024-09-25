using Cody.Core.Agent.Protocol;
using Cody.Core.Workspace;
using Cody.Core.Logging;
using System;
using System.Threading.Tasks;
using Cody.Core.Infrastructure;

namespace Cody.Core.Agent
{
    public class NotificationHandlers : IInjectAgentClient
    {
        private readonly IFileService _fileService;
        private readonly ISecretStorageService _secretStorage;
        private readonly ILog _logger;

        public IAgentClient AgentClient { set; private get; }

        public event EventHandler OnFocusSidebarRequest;

        public NotificationHandlers(ILog logger, IFileService fileService, ISecretStorageService secretStorage)
        {
            _fileService = fileService;
            _secretStorage = secretStorage;
            _logger = logger;
        }

        [AgentCallback("debug/message")]
        public void Debug(string channel, string message)
        {
            _logger.Debug($"[{channel} {message}]");
        }

        [AgentCallback("window/didChangeContext")]
        public void WindowDidChangeContext(string key, string value)
        {
            _logger.Debug(value, $@"WindowDidChangeContext Key - {key}");

            // Check the value to see if Cody is activated or deactivated
            // Deactivated: value = "false", meaning user is no longer authenticated.
            // In this case, we can send Agent a request to get the latest user AuthStatus to
            // confirm if the user is logged out or not.
            if (key == "cody.activated")
            {
                var isAuthenticated = value == "true";
                _logger.Debug(isAuthenticated.ToString(), "User is authenticated");
            }
        }

        [AgentCallback("extensionConfiguration/didChange", deserializeToSingleObject: true)]
        public void ExtensionConfigDidChange(ExtensionConfiguration config)
        {
            _logger.Debug(config.ToString());
        }

        [AgentCallback("textDocument/show", deserializeToSingleObject: true)]
        public Task<bool> ShowTextDocument(TextDocumentShowParams param)
        {
            var path = new Uri(param.Uri).ToString();
            return Task.FromResult(_fileService.OpenFileInEditor(path));
        }

        [AgentCallback("env/openExternal")]
        public Task<bool> OpenExternalLink(CodyFilePath path)
        {
            // Open the URL in the default browser
            System.Diagnostics.Process.Start(path.Uri);
            return Task.FromResult(true);

        }

        [AgentCallback("window/showSaveDialog")]
        public Task<string> ShowSaveDialog(SaveDialogOptionsParams paramValues)
        {
            return Task.FromResult("Not Yet Implemented");
        }

        [AgentCallback("secrets/get")]
        public Task<string> SecretGet(string key)
        {
            _logger.Debug(key, $@"SecretGet - {key}");
            return Task.FromResult(_secretStorage.Get(key));
        }

        [AgentCallback("secrets/store")]
        public void SecretStore(string key, string value)
        {
            _logger.Debug(key, $@"SecretStore - {key}");
            _secretStorage.Set(key, value);
        }

        [AgentCallback("secrets/delete")]
        public void SecretDelete(string key)
        {
            _logger.Debug(key, $@"SecretDelete - {key}");
            _secretStorage.Delete(key);
        }

        [AgentCallback("window/focusSidebar")]
        public void FocusSidebar(object param)
        {
            OnFocusSidebarRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}

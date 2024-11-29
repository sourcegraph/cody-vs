using Cody.Core.Agent.Protocol;
using Cody.Core.Ide;
using Cody.Core.Inf;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Cody.Core.Infrastructure
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IVersionService _versionService;
        private readonly IVsVersionService _vsVersionService;
        private readonly ISolutionService _solutionService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly ILog _logger;

        public ConfigurationService(IVersionService versionService, IVsVersionService vsVersionService, ISolutionService solutionService, IUserSettingsService userSettingsService, ILog logger)
        {
            _versionService = versionService;
            _vsVersionService = vsVersionService;
            _solutionService = solutionService;
            _userSettingsService = userSettingsService;
            _logger = logger;
        }

        public ClientInfo GetClientInfo()
        {
            var clientInfo = new ClientInfo
            {
                Name = "VisualStudio",
                Version = _versionService.Full.ToString(),
                IdeVersion = _vsVersionService.DisplayVersion,
                WorkspaceRootUri = _solutionService.GetSolutionDirectory(),
                Capabilities = new ClientCapabilities
                {
                    Authentication = Capability.Enabled,
                    Completions = "none",
                    Edit = Capability.None,
                    EditWorkspace = Capability.None,
                    ProgressBars = Capability.Enabled,
                    CodeLenses = Capability.None,
                    ShowDocument = Capability.Enabled,
                    Ignore = Capability.Enabled,
                    UntitledDocuments = Capability.None,
                    Webview = "native",
                    WebviewNativeConfig = new WebviewCapabilities
                    {
                        View = WebviewView.Single,
                        CspSource = "'self' https://cody.vs",
                        WebviewBundleServingPrefix = "https://cody.vs",
                    },
                    WebviewMessages = "string-encoded",
                    GlobalState = "server-managed",
                    Secrets = "client-managed",
                },
                ExtensionConfiguration = GetConfiguration()
            };

            return clientInfo;
        }

        public ExtensionConfiguration GetConfiguration()
        {
            var config = new ExtensionConfiguration
            {
                AnonymousUserID = _userSettingsService.AnonymousUserID,
                ServerEndpoint = _userSettingsService.ServerEndpoint,
                Proxy = null,
                AccessToken = _userSettingsService.AccessToken,
                AutocompleteAdvancedProvider = null,
                Debug = true,
                VerboseDebug = true,
                CustomConfiguration = GetCustomConfiguration()
            };

            return config;
        }

        internal Dictionary<string, object> GetCustomConfiguration()
        {
            var customConfiguration = _userSettingsService.CustomConfiguration;
            try
            {
                var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(customConfiguration);
                return config;
            }
            catch (Exception ex)
            {
                _logger.Error("Deserializing custom configuration failed.", ex);
            }

            return null;
        }

    }
}

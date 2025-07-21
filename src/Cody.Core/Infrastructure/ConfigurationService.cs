using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
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

        public const string CodySuggestionsMode = "cody.suggestions.mode";

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
                WorkspaceRootUri = _solutionService.GetSolutionDirectory().ToUri(),
                Capabilities = new ClientCapabilities
                {
                    Authentication = Capability.Enabled,
                    Completions = CompletionsCapability.None,
                    Autoedit = Capability.Enabled,
                    AutoeditInlineDiff = AutoeditInlineDiffCapability.None,
                    AutoeditAsideDiff = AutoeditAsideDiffCapability.Diff,
                    CodeCopyOnlyAction = Capability.None,
                    Edit = Capability.Enabled,
                    EditWorkspace = Capability.Enabled,
                    ProgressBars = Capability.Enabled,
                    CodeLenses = Capability.None,
                    Shell = Capability.Enabled,
                    ShowDocument = Capability.Enabled,
                    Ignore = Capability.Enabled,
                    ShowWindowMessage = ShowWindowMessageCapability.Request,
                    UntitledDocuments = Capability.None,
                    Webview = WebviewCapability.Native,
                    WebviewNativeConfig = new WebviewCapabilities
                    {
                        View = WebviewView.Single,
                        CspSource = "'self' https://cody.vs",
                        WebviewBundleServingPrefix = "https://cody.vs",
                    },
                    WebviewMessages = WebviewMessagesCapability.StringEncoded,
                    GlobalState = GlobalStateCapability.ServerManaged,
                    Secrets = SecretsCapability.ClientManaged,
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
                Proxy = null,
                AutocompleteAdvancedProvider = null,
                Debug = Configuration.AgentDebug,
                VerboseDebug = Configuration.AgentVerboseDebug,
                CustomConfiguration = GetCustomConfiguration()
            };

            if (_userSettingsService.ForceAccessTokenForUITests)
            {
                _logger.Debug($"Detected {nameof(_userSettingsService.ForceAccessTokenForUITests)}");

#pragma warning disable CS0618 // Type or member is obsolete
                config.ServerEndpoint = _userSettingsService.DefaultServerEndpoint;
                config.AccessToken = _userSettingsService.AccessToken;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            return config;
        }

        internal Dictionary<string, object> GetCustomConfiguration()
        {
            Dictionary<string, object> config = null;
            var customConfiguration = _userSettingsService.CustomConfiguration;
            try
            {
                config = JsonConvert.DeserializeObject<Dictionary<string, object>>(customConfiguration);
            }
            catch (Exception ex)
            {
                try
                {
                    //try to repair invalid json
                    var customConfigurationTrial = "{" + customConfiguration + "}";
                    config = JsonConvert.DeserializeObject<Dictionary<string, object>>(customConfigurationTrial);
                }
                catch { }

                ex.Data.Add("json", customConfiguration);
                _logger.Error("Deserializing custom configuration failed.", ex);
            }

            if (config == null) config = new Dictionary<string, object>();

            if (_userSettingsService.EnableAutoEdit && !config.ContainsKey(CodySuggestionsMode))
            {
                config[CodySuggestionsMode] = "auto-edit";
            }

            return config;
        }

    }
}

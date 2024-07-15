using Cody.Core.AgentProtocol;
using Cody.Core.Ide;
using Cody.Core.Inf;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Cody.VisualStudio.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Connector
{
    public class InitializeCallback
    {
        private readonly IUserSettingsService userSettingsService;
        private readonly IVersionService versionService;
        private readonly IVsVersionService vsVersionService;
        private readonly IStatusbarService statusbarService;
        private readonly ILog log;

        public InitializeCallback(
            IUserSettingsService userSettingsService,
            IVersionService versionService, 
            IVsVersionService vsVersionService, 
            IStatusbarService statusbarService,
            ILog log) 
        {
            this.userSettingsService = userSettingsService;
            this.versionService = versionService;
            this.vsVersionService = vsVersionService;
            this.statusbarService = statusbarService;
            this.log = log;
        }

        public async Task Initialize(IAgentClient client)
        {
            var clientInfo = new ClientInfo
            {
                Name = "VisualStudio",
                Version = versionService.Full,
                IdeVersion = vsVersionService.Version.ToString(),
                //WorkspaceRootUri = new Uri(Path.GetDirectoryName(VS.Solutions.GetCurrentSolution().FullPath)).AbsoluteUri,
                Capabilities = new ClientCapabilities
                {
                    Edit = Capability.Enabled,
                    EditWorkspace = Capability.None,
                    CodeLenses = Capability.None,
                    ShowDocument = Capability.None,
                    Ignore = Capability.Enabled,
                    UntitledDocuments = Capability.Enabled,
                },
                ExtensionConfiguration = new ExtensionConfiguration
                {
                    AnonymousUserID = userSettingsService.AnonymousUserID,
                    ServerEndpoint = userSettingsService.ServerEndpoint,
                    Proxy = null,
                    AccessToken = userSettingsService.AccessToken,
                    AutocompleteAdvancedProvider = null,
                    Debug = false,
                    VerboseDebug = false,
                    Codebase = null,

                }
            };

            var result = await client.Initialize(clientInfo);

            if (result.Authenticated == true)
            {
                client.Initialized();
                log.Info("Agent initialized");

                var subscription = await client.GetCurrentUserCodySubscription();

                statusbarService.SetText($"Hello {result.AuthStatus.DisplayName}. You are using cody {subscription.Plan} plan.");
            }
            else
            {
                log.Warn("Authentication failed. Please check the validity of the access token.");
            }
        }
    }
}

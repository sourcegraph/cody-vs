using Cody.Core.Agent.Protocol;
using Cody.Core.Ide;
using Cody.Core.Inf;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Connector
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
            // TODO: Get the solution directory path that the user is working on.
            var solutionDirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var wf = Path.Combine(solutionDirPath, "source", "repos");

            var clientInfo = new ClientInfo
            {
                Name = "VisualStudio",
                Version = versionService.Full,
                IdeVersion = vsVersionService.Version.ToString(),
                WorkspaceRootUri = new Uri(wf).ToString(),
                Capabilities = new ClientCapabilities
                {
                    Edit = Capability.Enabled,
                    EditWorkspace = Capability.Enabled,
                    CodeLenses = Capability.None,
                    ShowDocument = Capability.Enabled,
                    Ignore = Capability.Enabled,
                    UntitledDocuments = Capability.Enabled,
                    Webview = "native",
                    WebviewNativeConfig = new WebviewCapabilities
                    {
                        CspSource = "'self' https://cody.vs",
                        WebviewBundleServingPrefix = "https://cody.vs",
                    },
                    WebviewMessages = "string-encoded",
                },
                ExtensionConfiguration = new ExtensionConfiguration
                {
                    AnonymousUserID = userSettingsService.AnonymousUserID,
                    ServerEndpoint = userSettingsService.ServerEndpoint,
                    Proxy = null,
                    AccessToken = userSettingsService.AccessToken,
                    AutocompleteAdvancedProvider = null,
                    Debug = true,
                    VerboseDebug = true,
                    // Codebase = "github.com/sourcegraph/cody",

                }
            };

            var result = await client.Initialize(clientInfo);

            if (result.Authenticated == true)
            {
                var subscription = await client.GetCurrentUserCodySubscription();

                statusbarService.SetText($"Hello {result.AuthStatus.DisplayName}. You are using cody {subscription.Plan} plan.");
            }
            else
            {
                log.Warn("Authentication failed. Please check the validity of the access token.");
            }

            client.Initialized();
        }
    }
}

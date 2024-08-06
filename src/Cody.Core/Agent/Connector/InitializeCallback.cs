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
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cody");
            // Get the solution directory path that the user is working on
            var solutionDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var clientInfo = new ClientInfo
            {
                Name = "VisualStudio",
                Version = versionService.Full,
                IdeVersion = vsVersionService.Version.ToString(),
                WorkspaceRootUri = solutionDirectoryPath,
                Capabilities = new ClientCapabilities
                {
                    Edit = Capability.Enabled,
                    EditWorkspace = Capability.Enabled,
                    CodeLenses = Capability.None,
                    ShowDocument = Capability.Enabled,
                    Ignore = Capability.Enabled,
                    UntitledDocuments = Capability.Enabled,
                    Webview = new WebviewCapabilities
                    {
                        Type = "native",
                        CspSource = "'self' https://*.sourcegraphstatic.com",
                        WebviewBundleServingPrefix = "https://file.sourcegraphstatic.com",
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


            log.Info(clientInfo.WorkspaceRootUri);

            var result = await client.Initialize(clientInfo);

            if (result.Authenticated == true)
            {
                client.Initialized();
                
                log.Info(appData);
                               
                var subscription = await client.GetCurrentUserCodySubscription();

                statusbarService.SetText($"Hello {result.AuthStatus.DisplayName}. You are using cody {subscription.Plan} plan.");
            }
            else
            {
                log.Warn("Authentication failed. Please check the validity of the access token.");
            }

            await client.ResolveWebviewView(new ResolveWebviewViewParams
            {
                ViewId = "cody.chat",
                WebviewHandle = "visual-studio",
            });
        }
    }
}

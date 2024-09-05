using Cody.Core.Agent.Protocol;
using Cody.Core.Ide;
using Cody.Core.Inf;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.Core.Settings;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class InitializeCallback
    {
        private readonly IUserSettingsService userSettingsService;
        private readonly IVersionService versionService;
        private readonly IVsVersionService vsVersionService;
        private readonly IStatusbarService statusbarService;
        private readonly ISolutionService solutionService;
        private readonly ILog log;

        public InitializeCallback(
            IUserSettingsService userSettingsService,
            IVersionService versionService,
            IVsVersionService vsVersionService,
            IStatusbarService statusbarService,
            ISolutionService solutionService,
            ILog log)
        {
            this.userSettingsService = userSettingsService;
            this.versionService = versionService;
            this.vsVersionService = vsVersionService;
            this.statusbarService = statusbarService;
            this.solutionService = solutionService;
            this.log = log;
        }

        public async Task Initialize(IAgentService client)
        {
            var clientInfo = new ClientInfo
            {
                Name = "VisualStudio",
                Version = versionService.Full,
                IdeVersion = vsVersionService.Version.ToString(),
                WorkspaceRootUri = solutionService.GetSolutionDirectory(),
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

            var result = await client.Initialize(clientInfo);

            if (result.Authenticated == true)
            {
                statusbarService.SetText($"Hello {result.AuthStatus.DisplayName}! Press Alt + L to open Cody Chat.");
            }
            else
            {
                log.Warn("Authentication failed. Please check the validity of the access token.");
            }

            client.Initialized();
        }

        public ExtensionConfiguration GetConfiguration()
        {
            var config = new ExtensionConfiguration
            {
                AnonymousUserID = userSettingsService.AnonymousUserID,
                ServerEndpoint = userSettingsService.ServerEndpoint,
                Proxy = null,
                AccessToken = userSettingsService.AccessToken,
                AutocompleteAdvancedProvider = null,
                Debug = true,
                VerboseDebug = true,
            };

            return config;
        }
    }
}

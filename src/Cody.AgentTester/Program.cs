using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.VisualStudio.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Cody.VisualStudio.Services;

namespace Cody.AgentTester
{
    internal class Program
    {
        private static AgentClient client;
        private static ConsoleLogger logger = new ConsoleLogger();
        private static ConsoleLogger agentLogger = new ConsoleLogger();
        private static IAgentService agentService;

        static async Task Main(string[] args)
        {
            // Set the env var to 3113 when running with local agent.
            var devPort = Environment.GetEnvironmentVariable("CODY_VS_DEV_PORT");
            var portNumber = int.TryParse(devPort, out int port) ? port : 3113;

            var logger = new Logger();
            var settingsService = new UserSettingsService(new UserSettingsProvider(new FakeSettingsProvider()), logger);
            var options = new AgentClientOptions
            {
                NotificationHandlers = new List<INotificationHandler> { new NotificationHandlers(settingsService, logger) },
                AgentDirectory = "../../../Cody.VisualStudio/Agent",
                RestartAgentOnFailure = true,
                Debug = true,
                ConnectToRemoteAgent = devPort != null,
                RemoteAgentPort = portNumber,
            };

            client = new AgentClient(options, logger, agentLogger);

            client.Start();

            agentService = client.CreateAgentService<IAgentService>();

            await Initialize();

            Console.ReadKey();
        }

        private static async Task Initialize()
        {
            var clientInfo = new ClientInfo
            {
                Name = "VisualStudio",
                Version = "1.0",
                IdeVersion = "1.0",
                WorkspaceRootUri = Directory.GetCurrentDirectory().ToString(),
                Capabilities = new ClientCapabilities
                {
                    Edit = Capability.Enabled,
                    EditWorkspace = Capability.None,
                    CodeLenses = Capability.None,
                    ShowDocument = Capability.None,
                    Ignore = Capability.Enabled,
                    UntitledDocuments = Capability.Enabled,
                    Webview = "native",
                    WebviewNativeConfig = new WebviewCapabilities
                    {
                        View = WebviewView.Single,
                        CspSource = "'self' https://cody.vs",
                        WebviewBundleServingPrefix = "https://cody.vs",
                    },
                    WebviewMessages = "string-encoded",
                    GlobalState = "stateless",
                },
                ExtensionConfiguration = new ExtensionConfiguration
                {
                    AnonymousUserID = Guid.NewGuid().ToString(),
                    ServerEndpoint = "https://sourcegraph.com/",
                    Proxy = null,
                    AccessToken = Environment.GetEnvironmentVariable("SourcegraphCodyToken"),
                    AutocompleteAdvancedProvider = null,
                    Debug = true,
                    VerboseDebug = true,
                    Codebase = "github.com/sourcegraph/cody",

                }
            };

            await agentService.Initialize(clientInfo);

            agentService.Initialized();
        }


    }
}

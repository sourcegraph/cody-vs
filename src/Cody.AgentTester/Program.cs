using Cody.Core.Agent;
using Cody.Core.Agent.Connector;
using Cody.Core.Agent.Protocol;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Cody.AgentTester
{
    internal class Program
    {
        private static AgentConnector connector;
        private static ConsoleLogger logger = new ConsoleLogger();
        private static IAgentClient agentClient;

        static async Task Main(string[] args)
        {
            // Set the env var to 3113 when running with local agent.
            var portNumber = int.TryParse(Environment.GetEnvironmentVariable("CODY_VS_DEV_PORT"), out int port) ? port : (int?)null;

            var options = new AgentConnectorOptions
            {
                NotificationsTarget = new NotificationHandlers();,
                AgentDirectory = "../../../Cody.VisualStudio/Agent",
                RestartAgentOnFailure = true,
                Debug = true,
                Port = portNumber,
            };

            connector = new AgentConnector(options, logger);

            await connector.Connect();

            agentClient = connector.CreateClient();

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
                        CspSource = "'self' https://cody.vs",
                        WebviewBundleServingPrefix = "https://cody.vs",
                    },
                    WebviewMessages = "string-encoded",
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

            await agentClient.Initialize(clientInfo);

            agentClient.Initialized();
        }


    }
}

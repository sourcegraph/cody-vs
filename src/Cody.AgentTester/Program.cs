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
        private static string agentDirectoryPath;

        static async Task Main(string[] args)
        {
            var notifier = new NotificationHandlers();

            // Set the env var to 3113 when running with local agent.
            var port = Environment.GetEnvironmentVariable("CODY_VS_DEV_PORT");
            int? portNumber = null;
            if (port != null)
            {
                portNumber = Convert.ToInt32(port);
            }

            var options = new AgentConnectorOptions
            {
                NotificationsTarget = notifier,
                AgentDirectory = "../../../Cody.VisualStudio/Agent",
                RestartAgentOnFailure = true,
                Debug = true,
                Port = portNumber,
            };

            agentDirectoryPath = Path.GetFullPath(options.AgentDirectory);

            connector = new AgentConnector(options, logger);

            await connector.Connect();

            agentClient = await connector.CreateClient();

            await Initialize();

            Console.ReadKey();
        }

        private static async Task Initialize()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cody");

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
                    AnonymousUserID = Guid.NewGuid().ToString(),
                    ServerEndpoint = "https://sourcegraph.com/",
                    Proxy = null,
                    AccessToken = Environment.GetEnvironmentVariable("SourcegraphCodyToken"),
                    AutocompleteAdvancedProvider = null,
                    Debug = true,
                    VerboseDebug = true,
                    Codebase = null,

                }
            };

            await agentClient.Initialize(clientInfo);

            agentClient.Initialized();
        }


    }
}

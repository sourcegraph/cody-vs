using Cody.Core.Agent.Connector;
using Cody.Core.Agent.Protocol;
using Cody.Core.Inf;
using Cody.Core.Logging;
using Cody.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cody.Core.Agent;

namespace Cody.AgentTester
{
    internal class Program
    {
        private static AgentConnector connector;
        private static ConsoleLogger logger = new ConsoleLogger();
        private static IAgentClient agentClient;

        static async Task Main(string[] args)
        {
            var options = new AgentConnectorOptions
            {
                NotificationsTarget = new NotificationHandlers(),
                AgentDirectory = "../../../Cody.VisualStudio/Agent",
                RestartAgentOnFailure = true,
                Debug = true,
                Port = 3113,
            };

            connector = new AgentConnector(options, logger);

            await connector.Connect();
            agentClient = await connector.CreateClient();

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
                WorkspaceRootUri = "file:///C://Users/BeatrixW/Dev/vs",
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
                        WebviewBundleServingPrefix = "https://file+.sourcegraphstatic.com",
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

            // TODO: Move it to after we receive response for registerWebviewProvider
            // await agentClient.ResolveWebviewView("cody.chat", "native-webview-view-visual-studio");

            //await agentClient.DidDispose("view1");

            ;
        }


    }
}

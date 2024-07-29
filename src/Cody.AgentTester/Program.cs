using Cody.Core.Agent;
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
                Debug = true
            };

            connector = new AgentConnector(options, logger);

            connector.Connect();
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
                    AnonymousUserID = Guid.NewGuid().ToString(),
                    ServerEndpoint = "https://sourcegraph.com/",
                    Proxy = null,
                    AccessToken = Environment.GetEnvironmentVariable("SourcegraphCodyToken"),
                    AutocompleteAdvancedProvider = null,
                    Debug = false,
                    VerboseDebug = false,
                    Codebase = null,

                }
            };

            var result = await agentClient.Initialize(clientInfo);

            agentClient.Initialized();

            await agentClient.ResolveWebviewView("cody.chat", "native-webview-view-cody.chat");
            //await agentClient.RegisterWebViewProvider(new RegisterWebviewViewProviderParams(){ViewId = "cody.chat", RetainContextWhenHidden = true});

            //await agentClient.RegisterWebViewProvider("cody.chat", true);

            //await agentClient.DidDispose("view1");

            ;
        }


    }
}

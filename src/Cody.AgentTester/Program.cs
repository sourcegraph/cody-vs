using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Cody.VisualStudio.Client;
using Cody.VisualStudio.Services;
using Microsoft.VisualStudio.Setup.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Cody.AgentTester
{
    internal class Program
    {
        private static AgentClient client;
        private static readonly ConsoleLogger logger = new ConsoleLogger();
        private static readonly ConsoleLogger agentLogger = new ConsoleLogger();
        private static IAgentService agentService;

        static async Task Main(string[] args)
        {
            AssemblyLoader.Initialize();

            // Set the env var to 3113 when running with local agent.
            var devPort = Environment.GetEnvironmentVariable("CODY_VS_DEV_PORT");
            var portNumber = int.TryParse(devPort, out int port) ? port : 3113;

            var logger = new Logger();
            var secretStorageService = new SecretStorageService(new FakeSecretStorageProvider(), logger);
            var settingsService = new UserSettingsService(new MemorySettingsProvider(), secretStorageService, logger);
            var editorService = new FileService(new FakeServiceProvider(), logger);
            var options = new AgentClientOptions
            {
                CallbackHandlers = new List<object> { new NotificationHandlers(settingsService, logger, editorService, secretStorageService) },
                AgentDirectory = "../../../Cody.VisualStudio/Agent",
                RestartAgentOnFailure = true,
                Debug = true,
                ConnectToRemoteAgent = devPort != null,
                RemoteAgentPort = portNumber,
            };

            client = new AgentClient(options, logger, agentLogger);

            client.Start();

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
                    Authentication = Capability.Enabled,
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
                    Secrets = "stateless",
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
        }


    }
}

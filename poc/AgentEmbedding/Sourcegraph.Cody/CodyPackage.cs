global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SourcegraphCody;
using StreamJsonRpc;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Sourcegraph.Cody
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.CodyString)]
    public sealed class CodyPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            var jsonRpc = StartAgentProcess();

            //await jsonRpc.NotifyAsync("exit");

            var vsVersionService = new VsVersionService();
            var vsVersion = vsVersionService.GetDisplayVersion();
            var editionName = vsVersionService.GetEditionName();

            var clientInfo = new ClientInfo
            {
                Name = "VisualStudio",
                Version = Vsix.Version,
                IdeVersion = vsVersion,
                WorkspaceRootUri = new Uri(Path.GetDirectoryName(VS.Solutions.GetCurrentSolution().FullPath)).AbsoluteUri,
                Capabilities = new ClientCapabilities
                {
                    Edit = Capability.None,
                    EditWorkspace = Capability.None,
                    CodeLenses = Capability.None,
                    ShowDocument = Capability.None,
                    Ignore = Capability.Enabled,
                    UntitledDocuments = Capability.Enabled,
                },
                ExtensionConfiguration = new ExtensionConfiguration
                {
                    AnonymousUserID = "cdb239b6-6444-42fa-816e-0e32fdcf6d6b",
                    ServerEndpoint = "https://sourcegraph.com/",
                    Proxy = null,
                    AccessToken = "sgp_a0d7ccb4f752ea73_8a50e828f41ec18c673ebca5af2564f99a6dd751",
                    AutocompleteAdvancedProvider = null,
                    Debug = false,
                    VerboseDebug = false,
                    Codebase = null,

                }
            };

            var result = await jsonRpc.InvokeAsync<ServerInfo>("initialize", clientInfo);
        }

        private JsonRpc StartAgentProcess()
        {
            var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");

            var process = new Process();
            process.StartInfo.FileName = Path.Combine(agentDir, "node-win-x64.exe");
            process.StartInfo.Arguments = "--inspect --enable-source-maps index.js";
            process.StartInfo.WorkingDirectory = agentDir;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += Process_Exited;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            var result = process.Start();


            var jsonMessageFormatter = new JsonMessageFormatter();
            jsonMessageFormatter.JsonSerializer.ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() };
            jsonMessageFormatter.JsonSerializer.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            var handler = new HeaderDelimitedMessageHandler(process.StandardInput.BaseStream, process.StandardOutput.BaseStream, jsonMessageFormatter);
            var jsonRpc = new JsonRpc(handler, new Target());
            jsonRpc.StartListening();


            //JsonRpc jsonRpc = JsonRpc.Attach(process.StandardInput.BaseStream, process.StandardOutput.BaseStream, new Target());

            //process.BeginErrorReadLine();
            //process.BeginOutputReadLine();
            return jsonRpc;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data, "Agent");
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await VS.MessageBox.ShowWarningAsync("Cody agent process Exited");
            });

            Debug.WriteLine("Process Exited", "Agent");
        }
    }

    public class Target
    {
        [JsonRpcMethod("debug/message", UseSingleObjectParameterDeserialization = true)]
        public void Debug(DebugMessage msg)
        {
            System.Diagnostics.Debug.WriteLine(msg.Message, "Agent notify");
        }

        //[JsonRpcMethod("debug/message")]
        //public void Debug(string channel, string message)
        //{
        //    System.Diagnostics.Debug.WriteLine(message, "Agent notify");
        //}
    }

    public record DebugMessage(string Channel, string Message) { }


    public class ClientInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string? IdeVersion { get; set; }
        public string WorkspaceRootUri { get; set; }
        public ExtensionConfiguration? ExtensionConfiguration { get; set; }
        public ClientCapabilities? Capabilities { get; set; }

    }

    public class ExtensionConfiguration
    {
        public string ServerEndpoint { get; set; }
        public string? Proxy { get; set; }
        public string AccessToken { get; set; }

        public string? AnonymousUserID { get; set; }

        public string? AutocompleteAdvancedProvider { get; set; }

        public string? AutocompleteAdvancedModel { get; set; }

        public bool? Debug { get; set; }

        public bool? VerboseDebug { get; set; }

        public string? Codebase { get; set; }
    }

    public enum Capability
    {
        None,
        Enabled
    }

    public enum ChatCapability
    {
        None,
        Streaming
    }

    public enum ShowWindowMessageCapability
    {
        Notification,
        Request
    }

    public record ClientCapabilities
    {
        public string? Completions { get; set; }
        public ChatCapability? Chat { get; set; }
        public Capability? Git { get; set; }
        public Capability? ProgressBars { get; set; }
        public Capability? Edit { get; set; }
        public Capability? EditWorkspace { get; set; }
        public Capability? UntitledDocuments { get; set; }
        public Capability? ShowDocument { get; set; }
        public Capability? CodeLenses { get; set; }
        public ShowWindowMessageCapability? ShowWindowMessage { get; set; }
        public Capability? Ignore { get; set; }
        public Capability? CodeActions { get; set; }
        public string? WebviewMessages { get; set; }
    }

    public record ServerInfo(
        string Name,
        bool? Authenticated,
        bool? CodyEnabled,
        string? CodyVersion,
        AuthStatus? AuthStatus
        );

    public record AuthStatus(
    string Endpoint,
    bool IsDotCom,
    bool IsLoggedIn,
    bool ShowInvalidAccessTokenError,
    bool Authenticated,
    bool HasVerifiedEmail,
    bool RequiresVerifiedEmail,
    bool SiteHasCodyEnabled,
    string SiteVersion,
    bool UserCanUpgrade,
    string Username,
    string PrimaryEmail,
    string DisplayName,
    string AvatarURL,
    int CodyApiVersion,
    ConfigOverwrites ConfigOverwrites
);

    public record ConfigOverwrites(
        string ChatModel,
        int ChatModelMaxTokens,
        string FastChatModel,
        int FastChatModelMaxTokens,
        string CompletionModel,
        int CompletionModelMaxTokens,
        string Provider,
        bool SmartContextWindow
    );
}
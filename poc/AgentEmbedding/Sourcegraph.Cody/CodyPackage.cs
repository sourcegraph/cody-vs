global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
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

            //await jsonRpc.InvokeAsync("graphql/getRepoIdIfEmbeddingExists");
            await jsonRpc.NotifyAsync("exit");
            //jsonRpc.AddLocalRpcMethod("debug/message", new Action<string>(x => Debug.WriteLine(x, "Agent")));
        }

        private JsonRpc StartAgentProcess()
        {
            var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");

            var process = new Process();
            process.StartInfo.FileName = Path.Combine(agentDir, "node-win-x64.exe");
            process.StartInfo.Arguments = "index.js";
            process.StartInfo.WorkingDirectory = agentDir;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Exited += Process_Exited;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            var result = process.Start();
            

            JsonRpc jsonRpc = JsonRpc.Attach(process.StandardInput.BaseStream, process.StandardOutput.BaseStream, new Target());

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
            Debug.WriteLine("Process Exited", "Agent");
        }
    }

    public class Target
    {
        [JsonRpcMethod("debug/message")]
        public void Debug(DebugMessage msg)
        {
            System.Diagnostics.Debug.WriteLine(msg.message, "Agent Notify");
        }
    }

    public class DebugMessage
    {
        public string channel { get; set; }
        public string message { get; set; }
    }

}
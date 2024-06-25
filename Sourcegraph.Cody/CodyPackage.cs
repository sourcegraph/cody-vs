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

            //await jsonRpc.NotifyAsync("exit");
        }

        private JsonRpc StartAgentProcess()
        {
            var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");

            var process = new Process();
            process.Exited += Process_Exited;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(agentDir, "node-win-x64.exe"),
                Arguments = "index.js",
                WorkingDirectory = agentDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
            };
            
            var result = process.Start();
            

            JsonRpc jsonRpc = JsonRpc.Attach(process.StandardInput.BaseStream, process.StandardOutput.BaseStream);

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

}
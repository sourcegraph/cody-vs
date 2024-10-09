using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Client
{
    public class AgentProcessConnector : IAgentConnector
    {
        private Process process;

        public event EventHandler<int> Disconnected;
        public event EventHandler<string> ErrorReceived;

        public void Connect(AgentClientOptions options)
        {
            var path = Path.Combine(options.AgentDirectory, GetAgentFileName());

            if (!File.Exists(path))
                throw new FileNotFoundException("Agent file not found", path);

            process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = GetAgentArguments(options.Debug);
            process.StartInfo.WorkingDirectory = options.AgentDirectory;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;

            if (options.AcceptNonTrustedCertificates)
                process.StartInfo.EnvironmentVariables["NODE_TLS_REJECT_UNAUTHORIZED"] = "0";

            process.StartInfo.EnvironmentVariables["CODY_AGENT_TRACE_PATH"] = "c:/tmp/vs-agent.log";

            process.Exited += OnProcessExited;
            process.ErrorDataReceived += OnErrorDataReceived;

            process.Start();
            process.BeginErrorReadLine();
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e) => ErrorReceived?.Invoke(this, e.Data);

        private void OnProcessExited(object sender, EventArgs e) => Disconnected?.Invoke(this, process.ExitCode);

        public void Disconnect()
        {
            if (process != null && !process.HasExited) process.Kill();
        }

        public Stream SendingStream => process?.StandardInput?.BaseStream;

        public Stream ReceivingStream => process?.StandardOutput?.BaseStream;

        private string GetAgentFileName()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                return "node-win-arm64.exe";

            return "node-win-x64.exe";
        }

        private string GetAgentArguments(bool debugMode)
        {
            var argList = new List<string>();

            if (debugMode)
            {
                argList.Add("--inspect");
                argList.Add("--enable-source-maps");
            }

            argList.Add("index.js api jsonrpc-stdio");

            var arguments = string.Join(" ", argList);
            return arguments;
        }
    }
}

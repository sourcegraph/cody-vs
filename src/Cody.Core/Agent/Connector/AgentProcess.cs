using Cody.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Connector
{
    public class AgentProcess : IAgentProcess
    {
        private Process process = new Process();
        private string agentDirectory;
        private static string workingDirectory = "../../../../../cody/agent/dist";
        private bool debugMode;
        private ILog logger;
        private Action<int> onExit;

        private AgentProcess(string agentDirectory, bool debugMode, ILog logger, Action<int> onExit)
        {
            this.agentDirectory = agentDirectory;
            this.debugMode = debugMode;
            this.logger = logger;
            this.onExit = onExit;
        }

        public Stream SendingStream => process.StandardInput.BaseStream;

        public Stream ReceivingStream => process.StandardOutput.BaseStream;

        public static AgentProcess Start(string agentDirectory, bool debugMode, ILog logger, Action<int> onExit)
        {

            if (!Directory.Exists(agentDirectory))
                throw new ArgumentException("Directory does not exist", nameof(agentDirectory));
                
            var agentProcess = new AgentProcess(agentDirectory, debugMode, logger, onExit);
            agentProcess.StartInternal();

            return agentProcess;
        }


        private void StartInternal()
        {
            var path = Path.Combine(agentDirectory, GetAgentFileName());

            if (!File.Exists(path))
                throw new FileNotFoundException("Agent file not found", path);

            // Path.GetFullPath(workingDirectory);
            if (Directory.Exists(workingDirectory))
               agentDirectory = agentDirectory;

            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = GetAgentArguments(debugMode);
            process.StartInfo.WorkingDirectory = agentDirectory;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExited;
            process.ErrorDataReceived += OnErrorDataReceived;

            process.Start();
            process.BeginErrorReadLine();
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Error(e.Data, "Agent errors");
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            if (onExit != null) onExit(process.ExitCode);
        }

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

        public void Dispose()
        {
            if (!process.HasExited) process.Kill();

            process.Dispose();
        }

        public bool IsConnected
        {
            get
            {
                return !process.HasExited;
            }
        }
    }
}

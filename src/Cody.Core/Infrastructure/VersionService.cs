using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Cody.Core.Inf
{
    public class VersionService : IVersionService
    {
        private string GetAgentDirectory() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");

        public string CodyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string AgentVersion
        {
            get
            {
                var agentVersionFile = Path.Combine(GetAgentDirectory(), "agent.version");
                if (File.Exists(agentVersionFile)) return File.ReadAllText(agentVersionFile);
                else return null;
            }
        }

        public string NodeVersion
        {
            get
            {
                var nodeFileName = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "node-win-arm64.exe" : "node-win-x64.exe";
                var nodeVersionFile = Path.Combine(GetAgentDirectory(), nodeFileName);
                var versionInfo = FileVersionInfo.GetVersionInfo(nodeVersionFile);
                if (versionInfo != null) return versionInfo.ProductVersion;
                else return null;
            }
        }
    }
}

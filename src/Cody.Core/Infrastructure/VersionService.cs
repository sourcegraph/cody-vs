using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Cody.Core.Inf;
using Cody.Core.Logging;

namespace Cody.Core.Infrastructure
{
    public class VersionService : IVersionService
    {
        private readonly string _agentDirectory;
        private readonly Version _version;
        private readonly Assembly _entryAssembly;

        private readonly ILog _logger;

        public VersionService(ILog logger)
        {
            _logger = logger;
            try
            {
                _entryAssembly = Assembly.GetExecutingAssembly();
                _version = _entryAssembly.GetName().Version;
                _agentDirectory = Path.Combine(Path.GetDirectoryName(_entryAssembly.Location), "Agent");

                Agent = GetAgentVersion();
                Node = GetNodeVersion();

                Full = GetFullVersion();
            }
            catch (Exception ex)
            {
                _logger.Error("Initialization failed.", ex);
            }
        }

        private string GetFullVersion()
        {
            return $"{_version} ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()}) Agent:{Agent} Node:{Node}";
        }

        public DateTime GetDebugBuildDate()
        {
            var buildDate = new DateTime(2000, 01, 01).AddDays(_version.Build).AddSeconds(_version.Revision * 2);
            return buildDate;
        }

        public string Full { get; }
        public string Agent { get; }
        public string Node { get; }
        
        private string GetAgentVersion()
        {
            var agentVersionFile = Path.Combine(_agentDirectory, "agent.version");
            if (File.Exists(agentVersionFile)) return File.ReadAllText(agentVersionFile);

            _logger.Warn("Cannot get Agent version.");

            return null;
        }

        public string GetNodeVersion()
        {
            var nodeFileName = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "node-win-arm64.exe"
                : "node-win-x64.exe";
            var nodeVersionFile = Path.Combine(_agentDirectory, nodeFileName);

            if (File.Exists(nodeVersionFile))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(nodeVersionFile);
                return versionInfo.ProductVersion;
            }

            _logger.Warn("Cannot get Node version.");

            return null;
        }
    }
}

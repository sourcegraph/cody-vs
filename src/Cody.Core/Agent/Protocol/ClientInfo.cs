using System;

namespace Cody.Core.Agent.Protocol
{
    public class ClientInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string IdeVersion { get; set; }
        public string WorkspaceRootUri { get; set; }
        public string GlobalStateDir { get; set; }
        public ExtensionConfiguration ExtensionConfiguration { get; set; }
        public ClientCapabilities Capabilities { get; set; }

    }
}

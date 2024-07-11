using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.AgentProtocol
{
    public class ClientInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string IdeVersion { get; set; }
        public string WorkspaceRootUri { get; set; }
        public ExtensionConfiguration ExtensionConfiguration { get; set; }
        public ClientCapabilities Capabilities { get; set; }

    }
}

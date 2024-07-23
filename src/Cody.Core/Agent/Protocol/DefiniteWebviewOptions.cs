using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class DefiniteWebviewOptions
    {
        public bool EnableScripts { get; set; }

        public bool EnableForms { get; set; }

        public object EnableCommandUris { get; set; }

        public string[] LocalResourceRoots { get; set; }

        public PortMapping[] PortMapping { get; set; }

        public bool EnableFindWidget { get; set; }

        public bool RetainContextWhenHidden { get; set; }
    }


    public class PortMapping
    {
        public int WebviewPort { get; set; }

        public int ExtensionHostPort { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ServerInfo
    {
        public string Name { get; set; }
        public bool? Authenticated { get; set; }
        public bool? CodyEnabled { get; set; }
        public string CodyVersion { get; set; }
        public AuthStatus AuthStatus { get; set; }

    }
}

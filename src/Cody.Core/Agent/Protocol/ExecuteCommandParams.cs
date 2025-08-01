using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ExecuteCommandParams
    {
        public string Command { get; set; }
        public object[] Arguments { get; set; }
    }
}

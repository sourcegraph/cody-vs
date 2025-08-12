using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ModelContextWindow
    {
        public int Input { get; set; }

        public int Output { get; set; }

        public ContextWindow Context { get; set; }
    }

    public class ContextWindow
    {
        public int? User { get; set; }
    }
}

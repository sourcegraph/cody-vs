using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class WriteFileOptions
    {
        public bool? Overwrite { get; set; }

        public bool? IgnoreIfExists { get; set; }
    }
}

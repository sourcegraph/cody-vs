using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ProgressStartParams
    {
        public string Id { get; set; }

        public ProgressOptions Options { get; set; }
    }
}

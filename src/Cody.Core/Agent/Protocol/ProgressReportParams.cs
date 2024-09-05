using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ProgressReportParams
    {
        public string Id { get; set; }

        public string Message { get; set; }

        public int? Increment { get; set; }
    }
}

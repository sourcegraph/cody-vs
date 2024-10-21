using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class CompletionItemInfo
    {
        public int LineCount { get; set; }
        public int CharCount { get; set; }

        public string InsertText { get; set; }

        public string StopReason { get; set; }
    }
}

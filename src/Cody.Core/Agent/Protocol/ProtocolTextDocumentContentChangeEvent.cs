using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ProtocolTextDocumentContentChangeEvent
    {
        public Range Range { get; set; }

        public string Text { get; set; }
    }
}

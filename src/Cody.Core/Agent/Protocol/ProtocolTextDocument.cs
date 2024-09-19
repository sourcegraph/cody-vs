using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ProtocolTextDocument
    {
        public string Uri { get; set; }

        public string Content { get; set; }

        public Range Selection { get; set; }

        public ProtocolTextDocumentContentChangeEvent[] ContentChanges { get; set; }

        public Range VisibleRange { get; set; }
    }
}

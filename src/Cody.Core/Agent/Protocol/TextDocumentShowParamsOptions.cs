using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class TextDocumentShowParamsOptions
    {
        public bool PreserveFocus { get; set; }
        public bool Preview { get; set; }
        public Range Selection { get; set; }
    }
}

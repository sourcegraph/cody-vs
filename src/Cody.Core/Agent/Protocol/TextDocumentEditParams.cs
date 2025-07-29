using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class TextDocumentEditParams
    {
        public string Uri { get; set; }

        public TextEdit[] Edits { get; set; }

        public OptionsParams Options { get; set; }
    }
}

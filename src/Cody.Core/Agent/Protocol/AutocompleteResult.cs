using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class AutocompleteResult
    {
        public AutocompleteItem[] Items { get; set; }

        public CompletionBookkeepingEvent CompletionEvent { get; set; }
    }
}

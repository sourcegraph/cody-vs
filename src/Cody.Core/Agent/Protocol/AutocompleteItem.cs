using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class AutocompleteItem
    {
        public string Id { get; set; }
        public string InsertText { get; set; }
        public Range Range { get; set; }
    }
}

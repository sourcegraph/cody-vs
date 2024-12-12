using System;

namespace Cody.Core.Agent.Protocol
{
    public class AutocompleteItem
    {
        public string Id { get; set; }
        public string InsertText { get; set; }
        public Range Range { get; set; }
    }
}

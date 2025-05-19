using System;
using System.Collections.Generic;

namespace Cody.Core.Agent.Protocol
{
    public class AutocompleteResult
    {
        public AutocompleteItem[] Items { get; set; }

        public AutocompleteItem[] InlineCompletionItems { get; set; }

        public CompletionBookkeepingEvent CompletionEvent { get; set; }
    }
}

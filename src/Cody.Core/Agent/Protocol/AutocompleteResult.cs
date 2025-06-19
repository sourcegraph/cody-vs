using System;
using System.Collections.Generic;

namespace Cody.Core.Agent.Protocol
{
    public class AutocompleteResult
    {
        [Obsolete("Use 'inlineCompletionItems' instead")]
        public AutocompleteItem[] Items { get; set; }

        public AutocompleteItem[] InlineCompletionItems { get; set; }

        public AutocompleteEditItem[] DecoratedEditItems { get; set; }

        public CompletionBookkeepingEvent CompletionEvent { get; set; }
    }
}

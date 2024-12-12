using System;
using System.Collections.Generic;

namespace Cody.Core.Agent.Protocol
{
    public class AutocompleteParams
    {
        public string Uri { get; set; }
        public string FilePath { get; set; }

        public Position Position { get; set; }

        public TriggerKind? TriggerKind { get; set; }

        public SelectedCompletionInfo SelectedCompletionInfo { get; set; }
    }

    public enum TriggerKind
    {
        Automatic,
        Invoke
    }
}

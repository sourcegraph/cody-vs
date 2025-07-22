using System.Collections.Generic;

namespace Cody.Core.Agent.Protocol
{
    public class SaveDialogOptionsParams
    {
        public string DefaultUri { get; set; }
        public string SaveLabel { get; set; }
        public Dictionary<string, string[]> Filters { get; set; }
        public string Title { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class WorkspaceEditEntryMetadata
    {
        public bool NeedsConfirmation { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public string IconPath { get; set; }
    }
}

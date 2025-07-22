using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class WorkspaceEditParams
    {
        public WorkspaceEditOperation[] Operations { get; set; }

        public WorkspaceEditMetadata Metadata { get; set; }
    }
}

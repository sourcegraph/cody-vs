using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class WorkspaceFolderDidChangeEvent
    {
        public List<string> Uris { get; set; }

        public WorkspaceFolderDidChangeEvent()
        {
            Uris = new List<string>();
        }
    }

}

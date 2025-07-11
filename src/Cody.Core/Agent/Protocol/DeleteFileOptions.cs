using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class DeleteFileOptions
    {
        public bool? Recursive { get; set; }
        public bool? IgnoreIfNotExists { get; set; }
    }
}

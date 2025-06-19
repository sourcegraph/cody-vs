using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class AutoeditChanges
    {
        public AutoeditType Type { get; set; }
        public Range Range { get; set; }
        public string Text { get; set; }
    }

    public enum AutoeditType
    {
        Insert,
        Delete
    }
}

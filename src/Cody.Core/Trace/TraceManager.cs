using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Trace
{
    public static class TraceManager
    {
        public static ICollection<TraceListener> Listeners { get; } = new Collection<TraceListener>();

        public static bool Enabled { get; set; }

        public static Func<TraceEvent, bool> Filter { get; set; }
    }
}

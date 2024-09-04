using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ProgressOptions
    {
        public string Title { get; set; }

        public ProgressLocation? Location { get; set; }

        public string LocationViewId { get; set; }

        public bool? Cancellable { get; set; }
    }

    public enum ProgressLocation
    {
        SourceControl,
        Window,
        Notification
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.AgentProtocol
{
    public class CurrentUserCodySubscription
    {
        public string Status { get; set; }
        public string Plan { get; set; }
        public bool ApplyProRateLimits { get; set; }
        public DateTime CurrentPeriodStartAt { get; set; }
        public DateTime CurrentPeriodEndAt { get; set; }
    }
}

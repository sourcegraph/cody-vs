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
        string Plan { get; set; }
        bool ApplyProRateLimits { get; set; }
        DateTime CurrentPeriodStartAt { get; set; }
        DateTime CurrentPeriodEndAt { get; set; }
    }
}

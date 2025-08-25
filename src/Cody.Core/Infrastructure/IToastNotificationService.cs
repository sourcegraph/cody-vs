using Cody.Core.Agent.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Infrastructure
{
    public interface IToastNotificationService
    {
        Task<string> ShowNotification(SeverityEnum severity, string message, string details, IEnumerable<string> actions);
    }
}

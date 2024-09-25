using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public interface IInjectAgentClient
    {
        IAgentClient AgentClient { set; }
    }
}

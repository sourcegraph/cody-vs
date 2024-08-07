using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public interface IAgentClientFactory
    {
        IAgentClient CreateAgentClient();
    }
}

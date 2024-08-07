using Cody.Core.Agent.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class AgentClientFactory : IAgentClientFactory
    {
        private AgentConnector connector;

        public AgentClientFactory(AgentConnector connector)
        {
            this.connector = connector;
        }

        public IAgentClient CreateAgentClient()
        {
            return connector.CreateClient();
        }
    }
}

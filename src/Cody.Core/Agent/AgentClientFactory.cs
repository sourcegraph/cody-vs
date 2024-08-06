using Cody.Core.Agent.Connector;

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
            return connector.CreateClient().Result;
        }
    }
}

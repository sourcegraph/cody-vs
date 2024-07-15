using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Connector
{
    public class AgentConnectorOptions
    {
        public bool Debug { get; set; }
        public bool RestartAgentOnFailure { get; set; }

        public string AgentDirectory { get; set; }

        public object NotificationsTarget { get; set; }

        public Action<IAgentClient> AfterConnection { get; set; }

        public Action<IAgentClient> BeforeDisconnection { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Connector
{
    public class AgentConnectorOptions
    {
        public bool Debug { get; set; }

        public bool RestartAgentOnFailure { get; set; }

        /// <summary>
        /// If non-null, the TCP port to connect to an existing Agent instance on.
        /// </summary>
        public int? Port { get; set; }

        public string AgentDirectory { get; set; }

        public Action<IAgentClient> AfterConnection { get; set; }

        public Action<IAgentClient> BeforeDisconnection { get; set; }

        public NotificationHandlers NotificationsTarget { get; set; }
    }
}

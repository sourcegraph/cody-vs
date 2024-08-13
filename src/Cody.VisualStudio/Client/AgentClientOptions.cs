using Cody.Core.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Client
{
    public class AgentClientOptions
    {
        public bool Debug { get; set; }

        public bool RestartAgentOnFailure { get; set; } = true;

        public bool ConnectToRemoteAgent { get; set; } = false;

        /// <summary>
        /// If non-null, the TCP port to connect to an existing Agent instance on.
        /// </summary>
        public int RemoteAgentPort { get; set; } = 3113;

        public string AgentDirectory { get; set; }

        public List<INotificationHandler> NotificationHandlers { get; set; } = new List<INotificationHandler>();
    }
}

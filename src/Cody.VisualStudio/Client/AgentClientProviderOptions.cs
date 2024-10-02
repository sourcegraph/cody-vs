using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using System;
using System.Collections.Generic;

namespace Cody.VisualStudio.Client
{
    public class AgentClientProviderOptions
    {
        public bool Debug { get; set; }

        public bool ConnectToRemoteAgent { get; set; } = false;

        /// <summary>
        /// If non-null, the TCP port to connect to an existing Agent instance on.
        /// </summary>
        public int RemoteAgentPort { get; set; } = 3113;

        public bool AcceptNonTrustedCertificates { get; set; } = false;

        public string AgentDirectory { get; set; }

        public List<object> CallbackHandlers { get; set; } = new List<object>();

        public ClientInfo ClientInfo { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.AgentProtocol
{
    public class ExtensionConfiguration
    {
        public string ServerEndpoint { get; set; }
        public string Proxy { get; set; }
        public string AccessToken { get; set; }

        public string AnonymousUserID { get; set; }

        public string AutocompleteAdvancedProvider { get; set; }

        public string AutocompleteAdvancedModel { get; set; }

        public bool Debug { get; set; }

        public bool VerboseDebug { get; set; }

        public string Codebase { get; set; }
    }
}

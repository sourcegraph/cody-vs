using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cody.Core.Agent.Protocol
{
    public class ExtensionConfiguration
    {
        [Obsolete("Setting the property is obsolete. The agent supports changing it using UI, and use secret storage.")]
        public string ServerEndpoint { get; set; }
        public string Proxy { get; set; }
        [Obsolete("Setting the property is obsolete. The agent supports changing it using UI, and use secret storage.")]
        public string AccessToken { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; }

        public string AnonymousUserID { get; set; }

        public string AutocompleteAdvancedProvider { get; set; }

        public string AutocompleteAdvancedModel { get; set; }

        public bool Debug { get; set; }

        public bool VerboseDebug { get; set; }

        public string TelemetryClientName { get; set; }

        public string Codebase { get; set; }

        [Obsolete("Use 'customConfigurationJson' instead, it supports nested objects")]
        public Dictionary<string, object> CustomConfiguration { get; set; }

        public string CustomConfigurationJson { get; set; }

        public Dictionary<string, object> BaseGlobalState { get; set; }

        [Obsolete]
        public override string ToString()
        {
            return $"ServerEndpoint:'{ServerEndpoint}' Proxy:'{Proxy}' AccessToken:<TOKEN> AnonymousUserID:'{AnonymousUserID}' AutocompleteAdvancedProvider:'{AutocompleteAdvancedProvider}' AutocompleteAdvancedModel:'{AutocompleteAdvancedModel}' Debug:{Debug} VerboseDebug:{VerboseDebug} Codebase:{Codebase}";
        }
    }
}

namespace Cody.Core.Agent.Protocol
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

        public override string ToString()
        {
            return $"ServerEndpoint:'{ServerEndpoint}' Proxy:'{Proxy}' AccessToken:<TOKEN> AnonymousUserID:'{AnonymousUserID}' AutocompleteAdvancedProvider:'{AutocompleteAdvancedProvider}' AutocompleteAdvancedModel:'{AutocompleteAdvancedModel}' Debug:{Debug} VerboseDebug:{VerboseDebug} Codebase:{Codebase}";
        }
    }
}

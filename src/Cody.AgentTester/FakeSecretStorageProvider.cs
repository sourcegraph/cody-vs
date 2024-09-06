using Microsoft.VisualStudio.Shell.Connected.CredentialStorage;
using System;
using System.Collections.Generic;

namespace Cody.AgentTester
{
    public class FakeSecretStorageProvider : IVsCredentialStorageService
    {
        private Dictionary<IVsCredentialKey, IVsCredential> _credentials = new Dictionary<IVsCredentialKey, IVsCredential>();

        public IVsCredential Add(IVsCredentialKey key, string value)
        {
            var credential = new FakeCredential(value);
            _credentials[key] = credential;
            return credential;
        }
        public IVsCredential Retrieve(IVsCredentialKey key)
        {
            return _credentials[key];
        }
        public IEnumerable<IVsCredential> RetrieveAll(string key)
        {
            throw new NotImplementedException();
        }
        public bool Remove(IVsCredentialKey key)
        {
            return _credentials.Remove(key);
        }
        public IVsCredentialKey CreateCredentialKey(string featureName, string resource, string userName, string type)
        {
            return new FakeCredentialKey(featureName, resource, userName, type);
        }

        private class FakeCredentialKey : IVsCredentialKey
        {
            public string FeatureName { get; set; }
            public string UserName { get; set; }
            public string Type { get; set; }
            public string Resource { get; set; }

            public FakeCredentialKey(string featureName, string resource, string userName, string type)
            {
                FeatureName = featureName;
                UserName = userName;
                Type = type;
                Resource = resource;
            }
        }

        private class FakeCredential : IVsCredential
        {
            public string FeatureName { get; set; }
            public string UserName { get; set; }
            public string Type { get; set; }
            public string Resource { get; set; }

            public string TokenValue { get; set; }

            public bool RefreshTokenValue()
            {
                return true;
            }
            public void SetTokenValue(string tokenValue)
            {
                TokenValue = tokenValue;
            }
            public string GetProperty(string name)
            {
                return name;
            }
            public bool SetProperty(string name, string value)
            {
                return true;
            }
            public FakeCredential(string tokenValue)
            {
                TokenValue = tokenValue;
            }
        }
    }
}

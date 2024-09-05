using Cody.Core.Infrastructure;
using Microsoft.VisualStudio.Shell.Connected.CredentialStorage;

namespace Cody.VisualStudio.Services
{
    public class SecretStorageService : ISecretStorageService
    {
        private readonly IVsCredentialStorageService secretStorageService;
        private readonly string AccessTokenKey = "cody.access-token";
        private readonly string FeatureName = "Cody.VisualStudio";
        private readonly string UserName = "CodyAgent";
        private readonly string Type = "token";

        public SecretStorageService(IVsCredentialStorageService secretStorageService)
        {
            this.secretStorageService = secretStorageService;
        }

        public string Get(string key)
        {
            var credentialKey = this.secretStorageService.CreateCredentialKey(FeatureName, key, UserName, Type);
            var credential = this.secretStorageService.Retrieve(credentialKey);
            return credential?.TokenValue;

        }

        public void Set(string key, string value)
        {
            var credentialKey = this.secretStorageService.CreateCredentialKey(FeatureName, key, UserName, Type);
            this.secretStorageService.Add(credentialKey, value);
        }

        public void Delete(string key)
        {
            var credentialKey = this.secretStorageService.CreateCredentialKey(FeatureName, key, UserName, Type);
            this.secretStorageService.Remove(credentialKey);
        }

        public string AccessToken
        {
            get
            {
                return this.Get(AccessTokenKey);
            }
            set
            {
                this.Set(AccessTokenKey, value);
            }
        }
    }
}

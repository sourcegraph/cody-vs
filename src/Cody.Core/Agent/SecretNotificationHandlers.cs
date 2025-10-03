using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class SecretNotificationHandlers
    {
        private readonly ISecretStorageService secretStorage;
        private readonly ILog logger;

        public SecretNotificationHandlers(ISecretStorageService secretStorage, ILog logger)
        {
            this.secretStorage = secretStorage;
            this.logger = logger;
        }

        [AgentCallback("secrets/get")]
        public Task<string> SecretGet(string key)
        {
            logger.Debug(key, $@"SecretGet - {key}");
            return Task.FromResult(secretStorage.Get(key));
        }

        [AgentCallback("secrets/store")]
        public void SecretStore(string key, string value)
        {
            logger.Debug(key, $@"SecretStore - {key}");
            secretStorage.Set(key, value);
        }

        [AgentCallback("secrets/delete")]
        public void SecretDelete(string key)
        {
            logger.Debug(key, $@"SecretDelete - {key}");
            secretStorage.Delete(key);
        }
    }
}

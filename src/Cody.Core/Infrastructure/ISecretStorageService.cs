using System;

namespace Cody.Core.Infrastructure
{
    public interface ISecretStorageService
    {
        void Set(string key, string value);
        string Get(string key);
        void Delete(string key);
        string AccessToken { get; set; }
    }
}

using System;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Shell.Connected.CredentialStorage;

namespace Cody.VisualStudio.Services
{
    public class SecretStorageService : ISecretStorageService
    {
        private readonly IVsCredentialStorageService _secretStorageService;
        private readonly ILog _logger;
        private readonly string AccessTokenKey = "cody.access-token";
        private readonly string FeatureName = "Cody.VisualStudio";
        private readonly string UserName = "CodyAgent";
        private readonly string Type = "token";

        public event EventHandler AuthorizationDetailsChanged;

        public SecretStorageService(IVsCredentialStorageService secretStorageService, ILog logger)
        {
            _secretStorageService = secretStorageService;
            _logger = logger;
        }

        public string Get(string key)
        {
            try
            {
                var credentialKey = _secretStorageService.CreateCredentialKey(FeatureName, key, UserName, Type);
                var credential = _secretStorageService.Retrieve(credentialKey);

                var value = credential?.TokenValue;
                //_logger.Debug($"Get '{key}':{value}");

                if (IsEndpoint(key))
                    AuthorizationDetailsChanged?.Invoke(this, EventArgs.Empty);

                return value;
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot get key: '{key}'", ex);
            }

            return null;

        }

        private bool IsEndpoint(string key)
        {
            try
            {
                var isUri = IsValidUri(key);
                if (isUri)
                {
                    _logger.Debug($"Detected Uri:'{key}'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed.", ex);
            }

            return false;
        }
        private bool IsValidUri(string uriString)
        {
            return Uri.TryCreate(uriString, UriKind.Absolute, out _);
        }

        public void Set(string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    // cannot be an empty string ("") or start with the null character
                    value = "NULL";
                }

                var credentialKey = _secretStorageService.CreateCredentialKey(FeatureName, key, UserName, Type);
                _secretStorageService.Add(credentialKey, value);

                //_logger.Debug($"Set '{key}':{value}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot set key: '{key}'", ex);
            }
        }

        public void Delete(string key)
        {
            try
            {
                var credentialKey = _secretStorageService.CreateCredentialKey(FeatureName, key, UserName, Type);
                _secretStorageService.Remove(credentialKey);

                _logger.Debug($"Remove '{key}'");
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot delete key: '{key}'", ex);
            }
        }

        public string AccessToken
        {
            get
            {
                return Get(AccessTokenKey);
            }
            set
            {
                var oldAccessToken = AccessToken;
                Set(AccessTokenKey, value);

                if (oldAccessToken != value)
                    AuthorizationDetailsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

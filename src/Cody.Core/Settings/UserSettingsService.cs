using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using System;

namespace Cody.Core.Settings
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserSettingsProvider _settingsProvider;
        private readonly ISecretStorageService _secretStorage;
        private readonly ILog _logger;

        public event EventHandler AuthorizationDetailsChanged;

        public UserSettingsService(IUserSettingsProvider settingsProvider, ISecretStorageService secretStorage, ILog log)
        {
            _settingsProvider = settingsProvider;
            _secretStorage = secretStorage;
            _logger = log;
        }

        private string GetOrDefault(string settingName, string defaultValue = null)
        {
            try
            {
                if (_settingsProvider.SettingExists(settingName))
                    return _settingsProvider.GetSetting(settingName);

                return defaultValue;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed getting '{settingName}'", ex);
            }

            return null;
        }

        private void Set(string settingName, string value)
        {
            try
            {
                _settingsProvider.SetSetting(settingName, value);
                _logger.Info($"Value for the {settingName} setting has been changed to `{value}`");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed setting '{settingName}' with `{value}`", ex);
            }
        }

        public string AnonymousUserID
        {
            get => GetOrDefault(nameof(AnonymousUserID), Guid.NewGuid().ToString());
            set => Set(nameof(AnonymousUserID), value);
        }

        public string ServerEndpoint
        {
            get => GetOrDefault(nameof(ServerEndpoint), "https://sourcegraph.com/");
            set
            {
                var endpoint = GetOrDefault(nameof(ServerEndpoint));
                if (!string.Equals(value, endpoint, StringComparison.InvariantCulture))
                {
                    Set(nameof(ServerEndpoint), value);
                    AuthorizationDetailsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string AccessToken
        {
            get
            {
                var envToken = Environment.GetEnvironmentVariable("SourcegraphCodyToken");
                var userToken = _secretStorage.AccessToken;

                if (envToken != null && userToken == null) // use env token only when a user token is not set
                {
                    _logger.Warn("You are using a access token from environment variables!");
                    return envToken;
                }

                return userToken;
            }
            set
            {
                var userToken = _secretStorage.AccessToken;
                if (!string.Equals(value, userToken, StringComparison.InvariantCulture))
                {
                    _secretStorage.AccessToken = value;
                    AuthorizationDetailsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string CustomConfiguration
        {
            get => GetOrDefault(nameof(CustomConfiguration), string.Empty);
            set => Set(nameof(CustomConfiguration), value);
        }

        public bool AcceptNonTrustedCert
        {
            get
            {
                var value = GetOrDefault(nameof(AcceptNonTrustedCert), false.ToString());
                return bool.Parse(value);
            }
            set => Set(nameof(AcceptNonTrustedCert), value.ToString());
        }

        public bool AutomaticallyTriggerCompletions
        {
            get
            {
                var value = GetOrDefault(nameof(AutomaticallyTriggerCompletions), true.ToString());
                return bool.Parse(value);
            }
            set => Set(nameof(AutomaticallyTriggerCompletions), value.ToString());
        }
    }
}

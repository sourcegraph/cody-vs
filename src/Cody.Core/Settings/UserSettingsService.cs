using Cody.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Settings
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserSettingsProvider settingsProvider;
        private readonly ILog log;

        public UserSettingsService(IUserSettingsProvider settingsProvider, ILog log)
        {
            this.settingsProvider = settingsProvider;
            this.log = log;
        }

        private string GetOrDefault(string settingName, string defaultValue)
        {
            if (settingsProvider.SettingExists(settingName))
                return settingsProvider.GetSetting(settingName);

            return defaultValue;
        }

        private void Set(string settingName, string value)
        {
            settingsProvider.SetSetting(settingName, value);
            log.Info($"Value for the {settingName} setting has been changed.");
        }

        public string AnonymousUserID
        {
            get => GetOrDefault(nameof(AnonymousUserID), Guid.NewGuid().ToString());
            set => Set(nameof(AnonymousUserID), value);
        }

        public string ServerEndpoint
        {
            get => GetOrDefault(nameof(ServerEndpoint), "https://sourcegraph.com/");
            set => Set(nameof(ServerEndpoint), value);
        }

        public string AccessToken
        {
            get
            {
                var envToken = Environment.GetEnvironmentVariable("SourcegraphCodyToken");
                var userToken = GetOrDefault(nameof(AccessToken), null);
                ;
                if (envToken != null && userToken == null) // use env token only when an user token is not set
                {
                    log.Warn("You are using a access token from environment variables!");
                    return envToken;
                }

                return GetOrDefault(nameof(AccessToken), null);
            }
            set
            {
                Set(nameof(AccessToken), value);
            }
        }
    }
}

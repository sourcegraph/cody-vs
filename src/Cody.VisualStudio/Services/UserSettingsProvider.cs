using Cody.Core.Settings;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System;

namespace Cody.VisualStudio.Services
{
    public class UserSettingsProvider : IUserSettingsProvider
    {
        private readonly SettingsManager _settingsManager;
        private readonly WritableSettingsStore _userSettingsStore;

        private const string CollectionName = "Cody";

        public UserSettingsProvider(IServiceProvider serviceProvider)
        {
            _settingsManager = new ShellSettingsManager(serviceProvider);
            _userSettingsStore = _settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!_userSettingsStore.CollectionExists(CollectionName))
                _userSettingsStore.CreateCollection(CollectionName);
        }

        public bool SettingExists(string name) => _userSettingsStore.PropertyExists(CollectionName, name);

        public string GetSetting(string name) => _userSettingsStore.GetString(CollectionName, name);

        public void SetSetting(string name, string value) => _userSettingsStore.SetString(CollectionName, name, value);

    }
}

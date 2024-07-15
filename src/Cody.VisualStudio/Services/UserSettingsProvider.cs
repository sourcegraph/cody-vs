using Cody.Core.Settings;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    internal class UserSettingsProvider : IUserSettingsProvider
    {
        private SettingsManager settingsManager;
        private WritableSettingsStore userSettingsStore;

        private const string CollectionName = "Cody";

        public UserSettingsProvider(IServiceProvider serviceProvider)
        {
            settingsManager = new ShellSettingsManager(serviceProvider);
            userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if(!userSettingsStore.CollectionExists(CollectionName))
                userSettingsStore.CreateCollection(CollectionName);
        }

        public bool SettingExists(string name) => userSettingsStore.PropertyExists(CollectionName, name);

        public string GetSetting(string name) => userSettingsStore.GetString(CollectionName, name);

        public void SetSetting(string name, string value) => userSettingsStore.SetString(CollectionName, name, value);

    }
}

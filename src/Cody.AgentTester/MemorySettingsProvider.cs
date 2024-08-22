using Cody.Core.Settings;
using System;
using System.Collections.Generic;

namespace Cody.AgentTester
{
    public class MemorySettingsProvider : IUserSettingsProvider
    {
        private Dictionary<string, string> dic = new Dictionary<string, string>();

        public MemorySettingsProvider()
        {
            dic["ServerEndpoint"] = "https://sourcegraph.com/";
            dic["AnonymousUserID"] = Guid.NewGuid().ToString();

        }

        public string GetSetting(string name) => dic[name];

        public void SetSetting(string name, string value) => dic[name] = value;

        public bool SettingExists(string name) => dic.ContainsKey(name);
    }
}

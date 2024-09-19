using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Settings
{
    public interface IUserSettingsProvider
    {
        bool SettingExists(string name);

        string GetSetting(string name);

        void SetSetting(string name, string value);
    }
}

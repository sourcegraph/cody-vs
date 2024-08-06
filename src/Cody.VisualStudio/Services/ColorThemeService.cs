using Cody.Core.Infrastructure;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class ColorThemeService : IColorThemeService
    {
        private ThemeResourceKey[] colorsList = new ThemeResourceKey[]
        {
            EnvironmentColors.ToolWindowBackgroundColorKey,
            EnvironmentColors.ToolWindowTextColorKey,
            EnvironmentColors.ToolWindowBorderColorKey,
        };

        private IServiceProvider serviceProvider;

        public ColorThemeService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IReadOnlyDictionary<string, string> GetThemedColors()
        {
            var result = new Dictionary<string, string>();
            foreach (var colorKey in colorsList)
            {
                var color = VSColorTheme.GetThemedColor(colorKey);
                result.Add(ToCssVariableName(colorKey.Name), ToCssColor(color));
            }

            return result;
        }

        public bool IsDarkTheme()
        {
            const string darkTheme = "{1ded0138-47ce-435e-84ef-9ec1f439b749}";
            const string systemTheme = "{619dac1e-8220-4bd9-96fb-75ceb61a6107}";

            var settingsManager = new ShellSettingsManager(serviceProvider);
            var store = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
            var themeId = store.GetString("Theme", "BackupThemeId");

            if(themeId == darkTheme) return true;
            else if(themeId == systemTheme)
            {
                var colorMode = (int)Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "AppsUseLightTheme",
                    -1);

                if(colorMode == 0) return true;
            }

            return false;
        }

        private string ToCssColor(Color color) => $"rgb({color.R}, {color.G}, {color.B}, {color.A / 255f})";

        private string ToCssVariableName(string name) => $"--visualstudio-{name.ToLower()}";
    }
}

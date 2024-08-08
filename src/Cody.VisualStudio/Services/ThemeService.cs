using Cody.Core.Infrastructure;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class ThemeService : IThemeService
    {
        private ThemeResourceKey[] colorsList = new ThemeResourceKey[]
        {
            EnvironmentColors.ToolWindowBackgroundColorKey,
            EnvironmentColors.ToolWindowTextColorKey,
            EnvironmentColors.ToolWindowBorderColorKey,
        };

        private IServiceProvider serviceProvider;

        public ThemeService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IReadOnlyDictionary<string, string> GetColors()
        {
            var result = new Dictionary<string, string>();
            foreach (var colorKey in colorsList)
            {
                var color = VSColorTheme.GetThemedColor(colorKey);
                result.Add(ToCssVariableName(colorKey.Name), ToCssColor(color));
            }

            return result;
        }

        public FontInformation GetFont()
        {
            const string environmentCategory = "{1F987C00-E7C4-4869-8A17-23FD602268B0}";

            var storage = (IVsFontAndColorStorage)serviceProvider.GetService(typeof(SVsFontAndColorStorage));
            storage.OpenCategory(new Guid(environmentCategory), (uint)(__FCSTORAGEFLAGS.FCSF_READONLY));

            FontInformation uIFont;
            var logFont = new LOGFONTW[1];
            var pInfo = new FontInfo[1];
            storage.GetFont(logFont , pInfo);
            if (pInfo[0].bFaceNameValid == 1 && pInfo[0].bPointSizeValid == 1)
                uIFont = new FontInformation(pInfo[0].bstrFaceName, pInfo[0].wPointSize);
            else
            {
                var systemFont = SystemFonts.CaptionFont;
                uIFont = new FontInformation(systemFont.Name, systemFont.SizeInPoints);
            }
                
            storage.CloseCategory();

            return uIFont;
        }

        public bool IsDarkTheme()
        {
            const string darkTheme = "{1ded0138-47ce-435e-84ef-9ec1f439b749}";
            const string systemTheme = "{619dac1e-8220-4bd9-96fb-75ceb61a6107}";

            var settingsManager = new ShellSettingsManager(serviceProvider);
            var store = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
            var themeId = store.GetString("Theme", "BackupThemeId");

            if (themeId == darkTheme) return true;
            else if (themeId == systemTheme)
            {
                var colorMode = (int)Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "AppsUseLightTheme",
                    -1);

                if (colorMode == 0) return true;
            }

            return false;
        }

        private string ToCssColor(Color color) => $"rgb({color.R}, {color.G}, {color.B}, {color.A / 255f})";

        private string ToCssVariableName(string name) => $"--visualstudio-{name.ToLower()}";

        [Conditional("DEBUG")]
        public static void GetAllColors()
        {
            var list = new List<string>();
            var properties = typeof(EnvironmentColors).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach (var property in properties)
            {
                if (property.Name.EndsWith("ColorKey"))
                {
                    var value = (ThemeResourceKey)property.GetValue(null);
                    var color = VSColorTheme.GetThemedColor(value);
                    var line = $"{value.Name}\t{color.R}\t{color.G}\t{color.B}\t{color.A / 255f}";

                    list.Add(line);
                }
            }

            File.WriteAllLines("colors.txt", list);
        }
    }
}

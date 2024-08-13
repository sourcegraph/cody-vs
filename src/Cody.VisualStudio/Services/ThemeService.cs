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
using System.Text;

namespace Cody.VisualStudio.Services
{
    public class ThemeService : IThemeService
    {
        private IServiceProvider serviceProvider;

        public ThemeService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            // TODO: Update the webviews when the theme changes.
            // VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
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

        public FontInformation GetEditorFont()
        {
            const string textEditorCategory = "{A27B4E24-A735-4D1D-B8E7-9716E1E3D8E0}";
            return GetFontInfo(new Guid(textEditorCategory));
        }

        public FontInformation GetUIFont()
        {
            const string environmentCategory = "{1F987C00-E7C4-4869-8A17-23FD602268B0}";
            return GetFontInfo(new Guid(environmentCategory));
        }

        private FontInformation GetFontInfo(Guid categoryGuid)
        {
            var storage = (IVsFontAndColorStorage)serviceProvider.GetService(typeof(SVsFontAndColorStorage));
            storage.OpenCategory(categoryGuid, (uint)(__FCSTORAGEFLAGS.FCSF_LOADDEFAULTS | __FCSTORAGEFLAGS.FCSF_READONLY));

            var logFont = new LOGFONTW[1];
            var pInfo = new FontInfo[1];
            storage.GetFont(logFont, pInfo);

            var result = new FontInformation(pInfo[0].bstrFaceName, pInfo[0].wPointSize);

            storage.CloseCategory();

            return result;
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

        /// <summary>
        /// Generates a CSS stylesheet script that sets the themed colors for the Visual Studio environment.
        /// </summary>
        /// <returns>A string containing the CSS stylesheet script.</returns>
        public string GetThemingScript()
        {
            var colors = GetColors();
            var sb = new StringBuilder();
            sb.AppendLine("const rootStyle = document.documentElement.style;");

            // Generate the CSS variables for each color.
            foreach (var color in colors)
            {
                sb.AppendLine($"rootStyle.setProperty('{color.Key}', '{color.Value}');");
            }

            // Generate the CSS variables for the fonts.
            var editorFont = GetEditorFont();
            sb.AppendLine($"rootStyle.setProperty('--visualstudio-editor-font-family', '{editorFont.FontName}');");
            sb.AppendLine($"rootStyle.setProperty('--visualstudio-editor-font-size', '{editorFont.Size}pt');");

            var uiFont = GetUIFont();
            sb.AppendLine($"rootStyle.setProperty('--visualstudio-ui-font-family', '{uiFont.FontName}');");
            sb.AppendLine($"rootStyle.setProperty('--visualstudio-ui-font-size', '{uiFont.Size}pt');");

            return sb.ToString();
        }

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

        private readonly ThemeResourceKey[] colorsList = new ThemeResourceKey[]
        {
            EnvironmentColors.ToolWindowBackgroundColorKey,
            EnvironmentColors.ToolWindowTextColorKey,
            EnvironmentColors.ToolWindowBorderColorKey,
            EnvironmentColors.DropDownBackgroundColorKey,
            EnvironmentColors.DropDownTextColorKey,
            EnvironmentColors.DropDownBorderColorKey,
            EnvironmentColors.ToolWindowButtonHoverActiveColorKey,
            EnvironmentColors.ToolWindowButtonHoverActiveBorderColorKey,
            EnvironmentColors.PanelSeparatorColorKey,
            EnvironmentColors.EnvironmentBackgroundColorKey,
            EnvironmentColors.DropDownMouseDownTextColorKey,
            EnvironmentColors.DropDownMouseOverBackgroundEndColorKey,
            EnvironmentColors.MainWindowButtonHoverActiveColorKey,
            EnvironmentColors.MainWindowButtonHoverInactiveBorderColorKey,
            EnvironmentColors.MainWindowButtonHoverActiveBorderColorKey,
            EnvironmentColors.EditorExpansionFillColorKey,
            EnvironmentColors.EditorExpansionLinkColorKey,
            EnvironmentColors.EditorExpansionTextColorKey,
            EnvironmentColors.ComboBoxBackgroundColorKey,
            EnvironmentColors.ComboBoxTextColorKey,
            EnvironmentColors.ComboBoxBorderColorKey,
            EnvironmentColors.ComboBoxFocusedButtonBackgroundColorKey,
            EnvironmentColors.ComboBoxItemTextInactiveColorKey,
            EnvironmentColors.ComboBoxSelectionColorKey,
            EnvironmentColors.SystemButtonTextColorKey,
            EnvironmentColors.DiagReportLinkTextColorKey,
            EnvironmentColors.PanelTitleBarTextColorKey,
            EnvironmentColors.PanelTextColorKey,
            EnvironmentColors.PanelHyperlinkColorKey,
            EnvironmentColors.PanelHyperlinkDisabledColorKey,
            EnvironmentColors.ToolTipColorKey,
            EnvironmentColors.ToolWindowValidationErrorBorderColorKey,
            EnvironmentColors.ToolWindowValidationErrorTextColorKey,
            EnvironmentColors.SystemInfoBackgroundColorKey,
            EnvironmentColors.SystemInfoTextColorKey,
            EnvironmentColors.MainWindowSolutionNameActiveBackgroundColorKey,
            EnvironmentColors.MainWindowSolutionNameActiveTextColorKey,
            EnvironmentColors.WizardOrientationPanelBackgroundColorKey,
            EnvironmentColors.WizardOrientationPanelTextColorKey,
            EnvironmentColors.SystemWindowFrameColorKey,
            EnvironmentColors.HelpHowDoIPaneBackgroundColorKey,
            EnvironmentColors.HelpHowDoIPaneTextColorKey,
            EnvironmentColors.HelpHowDoITaskBackgroundColorKey,
            EnvironmentColors.DropDownButtonMouseOverBackgroundColorKey,
            EnvironmentColors.ToolWindowTabSelectedActiveTextColorKey,
            EnvironmentColors.TitleBarActiveBorderColorKey,
            EnvironmentColors.TitleBarInactiveColorKey,
            EnvironmentColors.VizSurfaceBrownDarkColorKey,
            EnvironmentColors.VizSurfaceDarkGoldDarkColorKey,
            EnvironmentColors.VizSurfaceGoldDarkColorKey,
            EnvironmentColors.VizSurfaceGreenDarkColorKey,
            EnvironmentColors.VizSurfacePlumDarkColorKey,
            EnvironmentColors.VizSurfaceRedDarkColorKey,
            EnvironmentColors.VizSurfaceSoftBlueDarkColorKey,
            EnvironmentColors.VizSurfaceSteelBlueDarkColorKey,
            EnvironmentColors.VizSurfaceStrongBlueDarkColorKey,
            EnvironmentColors.VSBrandingTextColorKey,
            EnvironmentColors.FileTabButtonHoverSelectedActiveBorderColorKey,
            EnvironmentColors.FileTabSelectedBackgroundColorKey,
            EnvironmentColors.FileTabParentTextColorKey,
            EnvironmentColors.FileTabPrimarySeparatorColorKey,
            EnvironmentColors.FileTabBackgroundColorKey,
            EnvironmentColors.FileTabActiveGroupTitleBackgroundColorKey,
            EnvironmentColors.FileTabHotTextColorKey,
            EnvironmentColors.FileTabInactiveTextColorKey,
            EnvironmentColors.FileTabButtonHoverInactiveBorderColorKey,
            EnvironmentColors.FileTabProvisionalHoverColorKey,
            EnvironmentColors.FileTabProvisionalHoverBorderColorKey,
            EnvironmentColors.FileTabProvisionalHoverForegroundColorKey,
            EnvironmentColors.FileTabProvisionalInactiveForegroundColorKey,
            EnvironmentColors.HelpSearchBackgroundColorKey,
            EnvironmentColors.HelpSearchBorderColorKey,
            EnvironmentColors.HelpSearchFilterBackgroundColorKey,
            EnvironmentColors.HelpSearchFilterBorderColorKey,
            EnvironmentColors.HelpSearchFilterTextColorKey,
            EnvironmentColors.HelpSearchTextColorKey,
            EnvironmentColors.ScrollBarArrowBackgroundColorKey,
            EnvironmentColors.ScrollBarArrowDisabledBackgroundColorKey,
            EnvironmentColors.ScrollBarArrowGlyphColorKey,
            EnvironmentColors.ScrollBarArrowGlyphDisabledColorKey,
            EnvironmentColors.ScrollBarArrowGlyphMouseOverColorKey,
            EnvironmentColors.ScrollBarArrowGlyphPressedColorKey,
            EnvironmentColors.ScrollBarArrowMouseOverBackgroundColorKey,
            EnvironmentColors.ScrollBarArrowPressedBackgroundColorKey,
            EnvironmentColors.ScrollBarBackgroundColorKey,
            EnvironmentColors.ScrollBarBorderColorKey,
            EnvironmentColors.ScrollBarDisabledBackgroundColorKey,
            EnvironmentColors.ScrollBarThumbBackgroundColorKey,
            EnvironmentColors.ScrollBarThumbBorderColorKey,
            EnvironmentColors.ScrollBarThumbDisabledColorKey,
            EnvironmentColors.ScrollBarThumbGlyphColorKey,
            EnvironmentColors.ScrollBarThumbGlyphMouseOverBorderColorKey,
            EnvironmentColors.ScrollBarThumbGlyphPressedBorderColorKey,
            EnvironmentColors.ScrollBarThumbMouseOverBackgroundColorKey,
            EnvironmentColors.ScrollBarThumbMouseOverBorderColorKey,
            EnvironmentColors.ScrollBarThumbPressedBackgroundColorKey,
            EnvironmentColors.ScrollBarThumbPressedBorderColorKey,
            EnvironmentColors.SmartTagBorderColorKey,
            EnvironmentColors.SmartTagFillColorKey,
            EnvironmentColors.SmartTagHoverBorderColorKey,
            EnvironmentColors.SmartTagHoverFillColorKey,
            EnvironmentColors.SmartTagHoverTextColorKey,
            EnvironmentColors.SmartTagTextColorKey,
            EnvironmentColors.SnaplinesColorKey,
            EnvironmentColors.SnaplinesPaddingColorKey,
            EnvironmentColors.SnaplinesTextBaselineColorKey,
            EnvironmentColors.SortBackgroundColorKey,
            EnvironmentColors.SortTextColorKey,
            EnvironmentColors.SplashScreenBorderColorKey,
            EnvironmentColors.StartPageButtonBorderColorKey,
            EnvironmentColors.StartPageButtonPinDownColorKey,
            EnvironmentColors.StartPageButtonPinHoverColorKey,
            EnvironmentColors.StartPageButtonPinnedColorKey,
            EnvironmentColors.StartPageButtonTextColorKey,
            EnvironmentColors.StartPageButtonTextHoverColorKey,
            EnvironmentColors.StartPageButtonUnpinnedColorKey,
            EnvironmentColors.StartPageCheckboxCheckmarkColorKey,
            EnvironmentColors.StartPageSelectedItemBackgroundColorKey,
            EnvironmentColors.StartPageSelectedItemStrokeColorKey,
            EnvironmentColors.StartPageSeparatorColorKey,
            EnvironmentColors.StartPageTextBodyColorKey,
            EnvironmentColors.StartPageTextBodySelectedColorKey,
            EnvironmentColors.StartPageTextBodyUnselectedColorKey,
            EnvironmentColors.StartPageTextControlLinkSelectedColorKey,
            EnvironmentColors.StartPageTextControlLinkSelectedHoverColorKey,
            EnvironmentColors.StartPageTextDateColorKey,
            EnvironmentColors.StartPageTextHeadingColorKey,
            EnvironmentColors.StartPageTextHeadingMouseOverColorKey,
            EnvironmentColors.StartPageTextHeadingSelectedColorKey,
            EnvironmentColors.StartPageTextSubHeadingColorKey,
            EnvironmentColors.StartPageTextSubHeadingMouseOverColorKey,
            EnvironmentColors.StartPageTextSubHeadingSelectedColorKey,
            EnvironmentColors.StatusBarBuildingColorKey,
            EnvironmentColors.StatusBarDebuggingColorKey,
            EnvironmentColors.StatusBarDefaultColorKey,
            EnvironmentColors.StatusBarHighlightColorKey,
            EnvironmentColors.StatusBarNoSolutionColorKey,
            EnvironmentColors.StatusBarTextColorKey,
        };
    }
}

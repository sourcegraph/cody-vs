using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Infrastructure
{
    public interface IThemeService
    {
        bool IsDarkTheme();

        IReadOnlyDictionary<string, string> GetColors();

        string GetThemingScript();

        FontInformation GetEditorFont();

        FontInformation GetUIFont();
    }

    public class FontInformation
    {
        public FontInformation(string fontName, float size)
        {
            FontName = fontName;
            Size = size;
        }

        public string FontName { get; protected set; }

        public float Size { get; protected set; }
    }
}

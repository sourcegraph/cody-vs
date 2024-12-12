using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cody.Core.Common
{
    public static class StringExtensions
    {
        public static string ToUri(this string path)
        {
            var uri = new Uri(path).AbsoluteUri;
            return Regex.Replace(uri, "(file:///)(\\D+)(:)", m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + "%3A");
        }

        public static string ConvertLineBreaks(this string text, string lineBrakeChars)
        {
            return Regex.Replace(text, @"\r\n?|\n", lineBrakeChars);
        }
    }
}

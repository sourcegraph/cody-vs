using System;
using System.Collections.Generic;
using System.IO;
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
            try
            {
                var uri = new Uri("file:///" + path).AbsoluteUri;
                return Regex.Replace(uri, "(file:///)(\\D+)(:)", m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + "%3A");
            }
            catch (Exception ex)
            {
                ex.Data.Add("path", path);
                throw;
            }
        }

        public static string ToWindowsPath(this string uri)
        {
            try
            {
                var uriObj = new Uri(Uri.UnescapeDataString(uri));
                return uriObj.LocalPath;
            }
            catch (Exception ex)
            {
                ex.Data.Add("uri", uri);
                throw;
            }
        }

        public static string ConvertLineBreaks(this string text, string lineBrakeChars)
        {
            return Regex.Replace(text, @"\r\n?|\n", lineBrakeChars);
        }
    }
}

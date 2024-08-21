using System;

namespace Cody.VisualStudio.Utilities
{
    public static class FilePathHelper
    {
        public static bool IsFilePath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out Uri fileUri) && fileUri.IsFile;
        }

        public static string SanitizeFilePath(string path)
        {
            // Remove the leading slash and convert to a proper file path
            return path.TrimStart('/').Replace('/', '\\');
        }
    }
}

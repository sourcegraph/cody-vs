using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions
{
    public static class SimpleLog
    {
        static StreamWriter writer;

        public static void SetLogFile(string path)
        {
            var stream = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read);     
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
        }

        private static void WriteLog(string type, string @class, string message)
        {
            if (writer == null) return;

            var now = DateTime.Now;
            var log = $"{now.ToString("yyyy-MM-dd HH:mm:ss.fff")} [{Environment.CurrentManagedThreadId}] {type} {@class} - {message}";
            writer.WriteLine(log);        
        }

        public static void Info(string @class, string message) => WriteLog("Info", @class, message);

        public static void Warning(string @class, string message) => WriteLog("Warning", @class,message);

        public static void Error(string @class, string message) => WriteLog("Error", @class, message);
    }
}

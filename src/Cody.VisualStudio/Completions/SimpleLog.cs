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

        private static void WriteLog(string type, string message)
        {
            if (writer == null) return;

            var now = DateTime.Now;
            var log = $"{now.ToString("yyyy-MM-dd HH:mm:ss.fff")} [{Environment.CurrentManagedThreadId}] {type} - {message}";
            writer.WriteLine(log);        
        }

        public static void Info(string message) => WriteLog("Info", message);

        public static void Warning(string message) => WriteLog("Warning", message);

        public static void Error(string message) => WriteLog("Error", message);
    }
}

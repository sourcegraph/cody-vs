using Cody.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cody.AgentTester
{
    public class ConsoleLogger : ILog
    {
        public void Debug(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = null)
        {
            WriteToConsole("debug", message, callerName);
        }

        public void Debug(string message, Exception ex, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = null)
        {
            WriteToConsole("debug", message, callerName, ex);
        }

        public void Error(string message, [CallerMemberName] string callerName = "")
        {
            WriteToConsole("error", message, callerName);
        }

        public void Error(string message, Exception ex, [CallerMemberName] string callerName = "")
        {
            WriteToConsole("error", message, callerName, ex);
        }

        public void Info(string message, [CallerMemberName] string callerName = "")
        {
            WriteToConsole("info", message, callerName);
        }

        public void Warn(string message, [CallerMemberName] string callerName = "")
        {
            WriteToConsole("warn", message, callerName);
        }

        private void WriteToConsole(string prefix, string message, string callerName, Exception ex = null)
        {
            Console.WriteLine($"{prefix.ToUpper()} {callerName}: {message}");
            if(ex != null) Console.WriteLine(ex.ToString());
        }
    }
}

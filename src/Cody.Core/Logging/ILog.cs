using System;
using System.Runtime.CompilerServices;

namespace Cody.Core.Logging
{
    public interface ILog
    {
        void Info(string message, [CallerMemberName] string callerName = "");
        void Warn(string message, [CallerMemberName] string callerName = "");
        void Debug(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = null);
        void Debug(string message, Exception ex, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = null);
        void Error(string message, [CallerMemberName] string callerName = "");
        void Error(string message, Exception ex, [CallerMemberName] string callerName = "");
    }
}

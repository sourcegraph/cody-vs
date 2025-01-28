using System;
using System.Runtime.CompilerServices;

namespace Cody.Core.Logging
{
    public interface ISentryLog
    {
        void Error(string message, Exception ex, [CallerMemberName] string callerName = "");

        void Error(string message, [CallerMemberName] string callerName = "");
    }
}

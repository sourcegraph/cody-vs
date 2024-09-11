using System.Runtime.CompilerServices;

namespace Cody.Core.Logging
{
    public interface ITestLogger
    {
        void WriteLog(string message, string type = "", [CallerMemberName] string callerName = "");
    }
}

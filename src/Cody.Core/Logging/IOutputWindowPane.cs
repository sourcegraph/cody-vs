namespace Cody.Core.Logging
{
    public interface IOutputWindowPane
    {
        void Info(string message, string callerName);
        void Warn(string message, string callerName);
        void Debug(string message, string callerName);
        void Error(string message, string callerName);
    }
}

using System;

namespace Cody.Core.Logging
{
    public interface ISentryLog
    {
        void Error(Exception exception);

        void Error(string message);
    }
}

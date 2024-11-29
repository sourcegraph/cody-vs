using Sentry;
using System;

namespace Cody.Core.Logging
{
    public class SentryLog : ISentryLog
    {
        public void Error(Exception exception)
        {
            SentrySdk.CaptureException(exception);
        }

        public void Error(string message)
        {
            SentrySdk.CaptureMessage(message);
        }

        public static void Initialize()
        {
#if !DEBUG
            if (!Debugger.IsAttached)
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                string env = "dev";
                if (version.Minor != 0) env = version.Minor % 2 != 0 ? "preview" : "production";

                SentrySdk.Init(options =>
                {
                    options.Dsn = "https://d129345ba8e1848a01435eb2596ca899@o19358.ingest.us.sentry.io/4508375896752129";
                    options.IsGlobalModeEnabled = true;
                    options.Environment = env;
                    options.Release = "cody-vs@" + version.ToString();
                });
            }
#endif
        }
    }
}

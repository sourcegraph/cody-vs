using Cody.Core.Common;
using Sentry;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cody.Core.Logging
{
    public class SentryLog : ISentryLog
    {
        public const string CodyAssemblyPrefix = "Cody.";

        public void Error(string message, Exception ex, [CallerMemberName] string callerName = "")
        {
            SentrySdk.CaptureException(ex, scope =>
            {
                scope.SetExtra(nameof(callerName), callerName);
                scope.SetExtra(nameof(message), message);
            });
        }

        public void Error(string message, [CallerMemberName] string callerName = "")
        {
            SentrySdk.CaptureMessage(message, scope =>
            {
                scope.SetExtra(nameof(callerName), callerName);
            });
        }

        public static void Initialize()
        {
            if (!Configuration.IsDebug && !Debugger.IsAttached)
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                string env = "dev";
                if (version.Minor != 0) env = version.Minor % 2 != 0 ? "preview" : "production";

                SentrySdk.Init(options =>
                {
                    options.Dsn = "https://d129345ba8e1848a01435eb2596ca899@o19358.ingest.us.sentry.io/4508375896752129";
                    options.IsGlobalModeEnabled = true;
                    options.MaxBreadcrumbs = 10;
                    options.Environment = env;
                    options.Release = "cody-vs@" + version.ToString();
                    options.SetBeforeSend(se =>
                    {
                        if (se.Exception?.Source?.StartsWith(CodyAssemblyPrefix) ?? false) return se;
                        if (se.Exception?.InnerException?.Source?.StartsWith(CodyAssemblyPrefix) ?? false) return se;
                        if (se.Message != null) return se;
                        if (se.SentryExceptions == null) return se;

                        foreach(var ex in se.SentryExceptions)
                        {
                            if (ex.Module?.StartsWith(CodyAssemblyPrefix) ?? false) return se;
                            if (ex.Stacktrace != null)
                            {
                                foreach (var frame in ex.Stacktrace.Frames)
                                {
                                    if (frame.Package?.StartsWith(CodyAssemblyPrefix) ?? false) return se;
                                }
                            }
                        }

                        return null;
                    });
                });
            }
        }
    }
}

using Cody.Core.Common;
using Sentry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cody.Core.Logging
{
    public class SentryLog : ISentryLog
    {
        public const string CodyAssemblyPrefix = "Cody.";
        public const string ErrorData = "ErrorData";
        public const int DaysToLogToSentry = 180;

        public void Error(string message, Exception ex, [CallerMemberName] string callerName = "")
        {
            SentrySdk.CaptureException(ex, scope =>
            {
                scope.Contexts[ErrorData] = new
                {
                    Message = message,
                    CallerName = callerName,
                };
            });
        }

        public void Error(string message, [CallerMemberName] string callerName = "")
        {
            SentrySdk.AddBreadcrumb(message,
                data: new Dictionary<string, string> { ["callerName"] = callerName },
                level: BreadcrumbLevel.Error);
        }

        private static DateTime GetLinkerBuildTime(Assembly assembly)
        {
            try
            {
                var filePath = assembly.Location;
                const int PeHeaderOffset = 60;
                const int LinkerTimestampOffset = 8;

                var buffer = new byte[2048];

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    stream.Read(buffer, 0, buffer.Length);
                }

                var offset = BitConverter.ToInt32(buffer, PeHeaderOffset);
                var secondsSince1970 = BitConverter.ToInt32(buffer, offset + LinkerTimestampOffset);
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                var linkTimeUtc = epoch.AddSeconds(secondsSince1970);
                return linkTimeUtc;
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }

        public static void Initialize(CancellationToken shutdownToken)
        {
            if (!Configuration.IsDebug && !Debugger.IsAttached)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var buildDate = GetLinkerBuildTime(assembly);
                if (Math.Abs((DateTime.UtcNow - buildDate).Days) > DaysToLogToSentry) return;

                var version = assembly.GetName().Version;
                string env = "dev";
                if (version.Minor != 0) env = version.Minor % 2 != 0 ? "preview" : "production";

                SentrySdk.Init(options =>
                {
                    options.Dsn = "https://d129345ba8e1848a01435eb2596ca899@o19358.ingest.us.sentry.io/4508375896752129";
                    options.IsGlobalModeEnabled = true;
                    options.MaxBreadcrumbs = 50;
                    options.Environment = env;
                    options.Release = "cody-vs@" + version.ToString();
                    options.SetBeforeSend(se =>
                    {
                        if (se.Exception?.Source?.StartsWith(CodyAssemblyPrefix) ?? false) return se;
                        if (se.Exception?.InnerException?.Source?.StartsWith(CodyAssemblyPrefix) ?? false) return se;
                        if (se.Message != null) return se;
                        if (se.Contexts.ContainsKey(ErrorData)) return se;
                        if (se.SentryExceptions == null) return se;
                        if (se.Exception is ObjectDisposedException && shutdownToken.IsCancellationRequested) return null;

                        foreach (var ex in se.SentryExceptions)
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

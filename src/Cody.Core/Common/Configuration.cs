using System;
using System.Collections.Generic;

namespace Cody.Core.Common
{
    public static partial class Configuration
    {
        public static bool DebugLogs => Get(false);

        public static bool AgentDebug => Get(false);

        public static bool AgentVerboseDebug => Get(false);

        public static bool AllowNodeDebug => Get(false);

        public static string AgentDirectory => Get((string)null);

        public static int? RemoteAgentPort => (int?)Get((long?)null);

        public static bool Trace => Get(false);

        public static string TraceFile => Get((string)null);

        public static bool ShowCodyAgentOutput => Get(false);

        public static bool ShowCodyNotificationsOutput => Get(false);
    }
}

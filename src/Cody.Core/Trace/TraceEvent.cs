using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Trace
{
    public class TraceEvent
    {
        public TraceEvent(string loggerName)
        {
            Timestamp = DateTime.Now;
            ThreadId = Environment.CurrentManagedThreadId;
            LoggerName = loggerName;
        }

        public string LoggerName { get; }

        public DateTime Timestamp { get; }

        public int ThreadId { get; }

        public string EventName { get; set; }

        public string Message { get; set; }

        public object[] MessageArgs { get; set; }

        public object Data { get; set; }

        public Exception Exception { get; set; }
    }
}

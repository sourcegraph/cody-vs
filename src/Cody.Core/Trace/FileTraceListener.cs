using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Trace
{
    public class FileTraceListener : TraceListener
    {
        private StreamWriter writer;

        public FileTraceListener(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }

        protected override void Initialize()
        {
            var stream = File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
        }

        protected string FormatTraceEvent(TraceEvent traceEvent)
        {
            var sb = new StringBuilder();
            var eventName = string.IsNullOrEmpty(traceEvent.EventName) ? "<none>" : traceEvent.EventName;
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss.fff} [{1,2}] {2}.{3}: ", traceEvent.Timestamp, traceEvent.ThreadId, traceEvent.LoggerName, eventName);

            if (!string.IsNullOrEmpty(traceEvent.Message))
            {
                sb.AppendFormat(traceEvent.Message, traceEvent.MessageArgs);
            }

            if (traceEvent.Data != null)
            {
                var output = JsonConvert.SerializeObject(traceEvent.Data);
                sb.Append(output);
            }

            if (traceEvent.Exception != null)
            {
                sb.Append(traceEvent.Exception);
            }

            return sb.ToString();
        }

        protected override void Write(TraceEvent traceEvent)
        {
            var formatedTraceEvent = FormatTraceEvent(traceEvent);
            writer.WriteLine(formatedTraceEvent);
        }
    }
}

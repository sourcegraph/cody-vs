using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Trace
{
    public class TraceLogger
    {
        public TraceLogger(string name)
        {
            if(string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        public string Name { get; private set; }

        public bool Enabled { get; set; } = true;

        private bool ShouldTrace() => TraceManager.Enabled && Enabled;

        protected void WriteTraceEvent(TraceEvent traceEvent)
        {
            if (TraceManager.Filter != null && !TraceManager.Filter(traceEvent)) return;

            foreach (var listener in TraceManager.Listeners)
            {
                if (listener.Enabled) listener.WriteTraceEvent(traceEvent);
            }
        }

        public void TraceEvent(string eventName) => TraceEvent(eventName, null);

        public void TraceEvent(string eventName, string message, params object[] args)
        {
            if (ShouldTrace())
            {
                var traceEvent = new TraceEvent(Name)
                {
                    EventName = eventName,
                    Message = message,
                    MessageArgs = args
                };

                WriteTraceEvent(traceEvent);
            }
        }

        public void TraceEvent(string eventName, object data)
        {
            if (ShouldTrace())
            {
                var traceEvent = new TraceEvent(Name)
                {
                    EventName = eventName,
                    Data = data
                };

                WriteTraceEvent(traceEvent);
            }
        }

        public void TraceData(object data) => TraceData(data, null);

        public void TraceData(object data, string message, params object[] args)
        {
            if (ShouldTrace())
            {
                var traceEvent = new TraceEvent(Name)
                {
                    Data = data,
                    Message = message,
                    MessageArgs = args
                };

                WriteTraceEvent(traceEvent);
            }
        }

        public void TraceMessage(string message) => TraceMessage(message, null);

        public void TraceMessage(string message, params object[] args)
        {
            if (ShouldTrace())
            {
                var traceEvent = new TraceEvent(Name)
                {
                    Message = message,
                    MessageArgs = args
                };

                WriteTraceEvent(traceEvent);
            }
        }

        public void TraceException(Exception exception)
        {
            if (ShouldTrace())
            {
                var traceEvent = new TraceEvent(Name)
                {
                    Exception = exception
                };

                WriteTraceEvent(traceEvent);
            }
        }
    }
}

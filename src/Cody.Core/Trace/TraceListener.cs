using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Trace
{
    public abstract class TraceListener
    {
        public bool Enabled { get; set; } = true;

        public Func<TraceEvent, bool> Filter { get; set; }

        private bool? successfullyInitialized;

        protected abstract void Initialize();

        public void WriteTraceEvent(TraceEvent traceEvent)
        {
            if (Enabled && traceEvent != null)
            {
                if (Filter != null && !Filter(traceEvent)) return;

                if (successfullyInitialized == true)
                {
                    try
                    {
                        Write(traceEvent);
                        return;
                    }
                    catch { }
                }

                if (successfullyInitialized == null)
                {
                    try
                    {
                        Initialize();
                        successfullyInitialized = true;
                        Write(traceEvent);
                    }
                    catch
                    {
                        successfullyInitialized = false;
                    }
                }

            }
        }

        protected abstract void Write(TraceEvent traceEvent);
    }
}

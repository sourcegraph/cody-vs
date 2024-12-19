using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Trace
{
    public class LogioTraceListener : TraceListener
    {
        private TcpClient client;
        private NetworkStream stream;
        private HashSet<string> inputs = new HashSet<string>();

        public LogioTraceListener(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        public string Hostname { get; }
        public int Port { get; }

        protected override void Initialize()
        {
            client = new TcpClient();
            client.Connect(Hostname, Port);
            stream = client.GetStream();
        }

        protected override void Write(TraceEvent traceEvent)
        {
            var eventName = string.IsNullOrEmpty(traceEvent.EventName) ? "<none>" : traceEvent.EventName;
            var input = $"{traceEvent.LoggerName}|{eventName}";
            if (!inputs.Contains(input))
            {
                var newInputMsg = $"+input|{input}\0";
                var newInputBytes = Encoding.UTF8.GetBytes(newInputMsg);

                stream.Write(newInputBytes, 0, newInputBytes.Length);

                inputs.Add(input);
            }

            var sb = new StringBuilder();
            sb.AppendFormat("[{1}]", traceEvent.ThreadId);
            if (!string.IsNullOrEmpty(traceEvent.Message))
            {
                sb.Append(" ");
                if (traceEvent.MessageArgs != null && traceEvent.MessageArgs.Any()) sb.AppendFormat(traceEvent.Message, traceEvent.MessageArgs);
                else sb.Append(traceEvent.Message);
            }

            if (traceEvent.Data != null)
            {
                var output = JsonConvert.SerializeObject(traceEvent.Data);
                sb.Append(" ");
                sb.Append(output);
            }

            if (traceEvent.Exception != null)
            {
                sb.Append(" ");
                sb.Append(traceEvent.Exception);
            }

            var msg = $"+msg|{input}|{sb}\0";
            var msgBytes = Encoding.UTF8.GetBytes(msg);

            stream.Write(msgBytes, 0, msgBytes.Length);
        }
    }
}

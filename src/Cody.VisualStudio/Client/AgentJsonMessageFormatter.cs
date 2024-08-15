using Cody.Core.Logging;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;
using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Client
{
    internal class AgentJsonMessageFormatter : JsonMessageFormatter, IJsonRpcFormatterTracingCallbacks
    {
        private ILog log;

        public AgentJsonMessageFormatter(ILog log)
        {
            this.log = log;
        }

        public bool TraceSentMessages { get; set; }

        public void OnSerializationComplete(JsonRpcMessage message, ReadOnlySequence<byte> encodedMessage)
        {
            if (TraceSentMessages)
            {
                string result = Encoding.UTF8.GetString(encodedMessage.ToArray());
                log.Debug($"Sending to agent: {result}");
            }
        }
    }
}

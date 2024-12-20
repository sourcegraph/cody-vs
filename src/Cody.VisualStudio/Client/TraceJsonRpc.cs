using Cody.Core.Trace;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;

namespace Cody.VisualStudio.Client
{
    public class TraceJsonRpc : JsonRpc, IJsonRpcTracingCallbacks
    {
        private static readonly TraceLogger trace = new TraceLogger(nameof(TraceJsonRpc));

        public TraceJsonRpc(IJsonRpcMessageHandler messageHandler) : base(messageHandler) { }

        void IJsonRpcTracingCallbacks.OnMessageSerialized(JsonRpcMessage message, object encodedMessage)
        {
            trace.TraceEvent("ToAgent", encodedMessage);
        }

        void IJsonRpcTracingCallbacks.OnMessageDeserialized(JsonRpcMessage message, object encodedMessage)
        {
            trace.TraceEvent("FromAgent", encodedMessage);
        }
    }
}

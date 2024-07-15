using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Connector
{
    public class NotificationHandlers
    {
        [JsonRpcMethod("debug/message")]
        public void Debug(string channel, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, "Agent");
        }
    }
}

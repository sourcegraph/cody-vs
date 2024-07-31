using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Connector
{
    internal interface IAgentProcess : IDisposable
    {
        bool IsConnected { get; }
        Stream SendingStream { get; }
        Stream ReceivingStream { get; }
    }
}

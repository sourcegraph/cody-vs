using System;
using Cody.Core.Agent.Protocol;

namespace Cody.Core.Agent
{
    public interface INotificationHandler
    {
        event EventHandler OnOptionsPageShowRequest;
        event EventHandler<string> OnRegisterWebViewRequest;
        event EventHandler<ProtocolAuthStatus> AuthorizationDetailsChanged;
    }
}

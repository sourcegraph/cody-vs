using System;
using Cody.Core.Agent.Protocol;
using Cody.Core.Ide;

namespace Cody.AgentTester
{
    public class FakeInfobarNotifications : IInfobarNotifications
    {
        public void ShowNotification(ShowWindowMessageParams messageParams, Func<string> callback)
        {
        }
    }
}

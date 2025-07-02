using System;
using System.Threading.Tasks;
using Cody.Core.Agent.Protocol;
using Cody.Core.Ide;

namespace Cody.AgentTester
{
    public class FakeInfobarNotifications : IInfobarNotifications
    {
        public Task<string> Show(ShowWindowMessageParams messageParams)
        {
            return null;
        }
    }
}

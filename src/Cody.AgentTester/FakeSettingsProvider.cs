using System;

namespace Cody.AgentTester
{
    public class FakeSettingsProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return null;
        }
    }
}

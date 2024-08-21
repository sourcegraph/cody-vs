using System;

namespace Cody.AgentTester
{
    public class FakeServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            // For now, return null for all service requests
            // You can extend this method to return mock objects for specific service types if needed
            return null;
        }
    }
}

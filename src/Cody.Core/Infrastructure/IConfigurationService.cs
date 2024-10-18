using Cody.Core.Agent.Protocol;

namespace Cody.Core.Infrastructure
{
    public interface IConfigurationService
    {
        ClientInfo GetClientInfo();
        ExtensionConfiguration GetConfiguration();
    }
}

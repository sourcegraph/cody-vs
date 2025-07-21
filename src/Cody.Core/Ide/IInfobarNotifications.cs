using System.Threading.Tasks;
using Cody.Core.Agent.Protocol;

namespace Cody.Core.Ide
{
    public interface IInfobarNotifications
    {
        Task<string> Show(ShowWindowMessageParams messageParams);
    }
}

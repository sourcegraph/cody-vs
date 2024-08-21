using Cody.Core.Agent.Protocol;

namespace Cody.Core.Workspace
{
    public interface IFileService
    {
        bool OpenFileInEditor(string path, Range range = null);
    }
}

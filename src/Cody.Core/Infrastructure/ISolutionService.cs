namespace Cody.Core.Infrastructure
{
    public interface ISolutionService
    {
        bool IsSolutionOpen();

        string GetSolutionDirectory();
    }
}

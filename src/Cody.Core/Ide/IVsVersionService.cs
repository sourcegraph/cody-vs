namespace Cody.Core.Ide
{
    public interface IVsVersionService
    {
        string SemanticVersion { get; }
        string DisplayVersion { get; }
        string EditionName { get; }
        bool HasCompletionSupport { get; }
    }
}


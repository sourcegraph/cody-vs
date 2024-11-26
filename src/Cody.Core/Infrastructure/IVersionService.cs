namespace Cody.Core.Inf
{
    public interface IVersionService
    {
        string CodyVersion { get; }
        string AgentVersion { get; }
        string NodeVersion { get; }
    }
}

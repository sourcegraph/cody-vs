namespace Cody.Core.Agent
{
    public interface IAgentProxy
    {
        bool IsConnected { get; }
        void Start();
        T CreateAgentService<T>() where T : class;
    }
}

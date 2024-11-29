using System;

namespace Cody.Core.Inf
{
    public interface IVersionService
    {
        string Full { get; }
        string Agent { get; }
        string Node { get; }
        DateTime GetDebugBuildDate();
    }
}

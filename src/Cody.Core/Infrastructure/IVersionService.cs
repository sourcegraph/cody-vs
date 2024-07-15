using System;

namespace Cody.Core.Inf
{
    public interface IVersionService
    {
        string Full { get; }
        string Major { get; }
        string Build { get; }
        bool IsDebug { get; }
        DateTime BuildDate { get; }
        void AddBuildMetadata(string build, bool isDebug);
    }
}
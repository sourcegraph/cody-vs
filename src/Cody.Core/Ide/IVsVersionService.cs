using System;

namespace Cody.Core.Ide
{
    public interface IVsVersionService
    {
        Version Version { get; }
    }
}

using System;

namespace Cody.Core.Infrastructure
{
    public interface IProgressService
    {
        void End(string id);
        void ReportProgress(string id, string message, int? increment);
        void Start(string id, string title, Action actionOnUserCancel);
    }
}

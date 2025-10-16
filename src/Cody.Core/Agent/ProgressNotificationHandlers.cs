using Cody.Core.Agent.Protocol;
using Cody.Core.Infrastructure;
using System;

namespace Cody.Core.Agent
{
    public class ProgressNotificationHandlers
    {
        private readonly IProgressService _progressService;
        private IAgentService _agentService;

        public ProgressNotificationHandlers(IProgressService progressService)
        {
            this._progressService = progressService;
        }

        public void SetAgentService(IAgentService agentService) => this._agentService = agentService;

        [AgentCallback("progress/start", deserializeToSingleObject: true)]
        public void Start(ProgressStartParams progressStart)
        {
            Action cancelAction = null;
            if (progressStart.Options.Cancellable == true)
            {
                cancelAction = () => _agentService.Get().CancelProgress(progressStart.Id);
            };

            _progressService.Start(progressStart.Id, progressStart.Options.Title, cancelAction);
        }

        [AgentCallback("progress/report", deserializeToSingleObject: true)]
        public void Report(ProgressReportParams progressReport)
        {
            _progressService.ReportProgress(progressReport.Id, progressReport.Message, progressReport.Increment);
        }

        [AgentCallback("progress/end")]
        public void End(string id)
        {
            _progressService.End(id);
        }
    }
}

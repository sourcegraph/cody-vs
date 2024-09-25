using Cody.Core.Agent.Protocol;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cody.Core.Agent
{
    public class ProgressNotificationHandlers : IInjectAgentClient
    {
        private IProgressService progressService;

        public IAgentClient AgentClient { set; private get; }

        public ProgressNotificationHandlers(IProgressService progressService)
        {
            this.progressService = progressService;
        }

        [AgentCallback("progress/start", deserializeToSingleObject: true)]
        public void Start(ProgressStartParams progressStart)
        {
            Action cancelAction = null;
            if (progressStart.Options.Cancellable == true)
            {
                cancelAction = () => AgentClient.CancelProgress(progressStart.Id);
            };

            progressService.Start(progressStart.Id, progressStart.Options.Title, cancelAction);
        }

        [AgentCallback("progress/report", deserializeToSingleObject: true)]
        public void Report(ProgressReportParams progressReport)
        {
            progressService.ReportProgress(progressReport.Id, progressReport.Message, progressReport.Increment);
        }

        [AgentCallback("progress/end")]
        public void End(string id)
        {
            progressService.End(id);
        }
    }
}

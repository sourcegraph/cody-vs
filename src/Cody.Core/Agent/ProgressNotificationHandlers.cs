using Cody.Core.Agent.Protocol;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class ProgressNotificationHandlers
    {
        private IProgressService progressService;
        private IAgentService agentService;

        public ProgressNotificationHandlers(IProgressService progressService)
        {
            this.progressService = progressService;
        }

        public void SetAgentService(IAgentService agentService) => this.agentService = agentService;

        [AgentCallback("progress/start", deserializeToSingleObject: true)]
        public void Start(ProgressStartParams progressStart)
        {
            Action cancelAction = null;
            if(progressStart.Options.Cancellable == true)
            {
                cancelAction = () => agentService.CancelProgress(progressStart.Id);
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

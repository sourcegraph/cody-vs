using Cody.Core.Logging;
using Cody.Core.Trace;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text;
using System;

namespace Cody.VisualStudio.Completions
{
    public class CodyProposalManager : ProposalManagerBase
    {
        private readonly ILog _logger;
        private static TraceLogger trace = new TraceLogger(nameof(CodyProposalManager));

        public CodyProposalManager(ILog logger)
        {
            _logger = logger;
        }

        public override bool TryGetIsProposalPosition(VirtualSnapshotPoint caret, ProposalScenario scenario, char triggerCharacter, ref bool value)
        {
            try
            {
                switch (scenario)
                {
                    case ProposalScenario.TypeChar:
                        trace.TraceEvent("TypeCharScenario");
                        if (char.IsWhiteSpace(triggerCharacter) && caret.Position.Position >= 2)
                        {
                            char c = caret.Position.Snapshot[caret.Position.Position - 2];
                            if (!char.IsWhiteSpace(c)) value = true;
                        }
                        else value = true;

                        break;
                    case ProposalScenario.Return:
                        trace.TraceEvent("ReturnScenario");
                        if (caret.Position.GetContainingLine().End == caret.Position) value = true;
                        break;
                    default:
                        trace.TraceEvent("OtherScenario");
                        value = true;
                        break;
                }

                trace.TraceEvent("ProposalScenario", scenario.ToString());
                trace.TraceEvent("ShowProposal", value);
                return value;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed.", ex);
            }

            value = false;
            return value;
        }
    }
}

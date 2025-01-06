using Cody.Core.Trace;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text;

namespace Cody.VisualStudio.Completions
{
    public class CodyProposalManager : ProposalManagerBase
    {
        private static TraceLogger trace = new TraceLogger(nameof(CodyProposalManager));

        public override bool TryGetIsProposalPosition(VirtualSnapshotPoint caret, ProposalScenario scenario, char triggerCharacter, ref bool value)
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

            trace.TraceEvent("ShowProposal", value);
            return value;
        }
    }
}

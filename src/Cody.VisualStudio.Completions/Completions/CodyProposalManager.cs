using Cody.Core.Logging;
using Cody.Core.Trace;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;

namespace Cody.VisualStudio.Completions
{
    public class CodyProposalManager : ProposalManagerBase
    {
        private readonly ILog _logger;
        private static TraceLogger trace = new TraceLogger(nameof(CodyProposalManager));

        private readonly ProposalScenario[] acceptedScenarios = new ProposalScenario[]
        {
            ProposalScenario.TypeChar,
            ProposalScenario.Return,
            ProposalScenario.ExplicitInvocation,
            ProposalScenario.CompletionAccepted,
            ProposalScenario.CompletedProposal,
            ProposalScenario.DivergedProposal

        };

        private const string LastCaretMoveLineKey = "cody_lastCaretMoveLine";

        public CodyProposalManager(ILog logger)
        {
            _logger = logger;
        }

        public override bool TryGetIsProposalPosition(VirtualSnapshotPoint caret, ProposalScenario scenario, char triggerCharacter, ref bool value)
        {
            if (acceptedScenarios.Contains(scenario)) value = true;
            else if (scenario == ProposalScenario.CaretMove)
            {
                var currentLine = caret.Position.GetContainingLine();
                var properties = caret.Position.Snapshot.TextBuffer.Properties;
                if (properties.TryGetProperty(LastCaretMoveLineKey, out int lastLine))
                {
                    if (currentLine.LineNumber != lastLine)
                    {
                        properties.Exchange(LastCaretMoveLineKey, currentLine.LineNumber);
                        if (caret.Position != currentLine.End) value = true;
                    }
                }
                else properties.AddProperty(LastCaretMoveLineKey, currentLine.LineNumber);
            }

            trace.TraceEvent("ProposalScenario", scenario.ToString());
            trace.TraceEvent("ShowProposal", value);

            return value;
        }
    }
}

using Cody.Core.Logging;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions
{
    public class CodyProposalManager : ProposalManagerBase
    {
        public override bool TryGetIsProposalPosition(VirtualSnapshotPoint caret, ProposalScenario scenario, char triggerCharacter, ref bool value)
        {
            switch(scenario)
            {
                case ProposalScenario.TypeChar:
                    SimpleLog.Info("CodyProposalManager", "TypeChar");
                    if (char.IsWhiteSpace(triggerCharacter) && caret.Position.Position >= 2)
                    {
                        char c = caret.Position.Snapshot[caret.Position.Position - 2];
                        if (!char.IsWhiteSpace(c)) value = true;
                    }
                    else value = true;
                    break;
                case ProposalScenario.Return:
                    SimpleLog.Info("CodyProposalManager", "Return");
                    if (caret.Position.GetContainingLine().End == caret.Position) value = true;
                    break;
                default:
                    SimpleLog.Info("CodyProposalManager", "defalut");
                    value = true;
                    break;
            }

            SimpleLog.Info("CodyProposalManager", $"show proposal: {value}");
            return value;
        }
    }
}

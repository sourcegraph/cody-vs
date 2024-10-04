using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Cody.VisualStudio.Proposals
{

    [Export(typeof(ProposalManagerProviderBase))]
    [Name(nameof(CodyProposalManagerProvider))]
    [Order(Before = "InlineCSharpProposalManagerProvider")]
    [Order(Before = "Highest Priority")]
    [ContentType("any")]
    public class CodyProposalManagerProvider: ProposalManagerProviderBase
    {
        public override Task<ProposalManagerBase> GetProposalManagerAsync(ITextView view, CancellationToken cancel)
        {
            return null;
        }
    }

    public class CodyProposalManager : ProposalManagerBase
    {
        public override bool TryGetIsProposalPosition(VirtualSnapshotPoint caret, ProposalScenario scenario, char triggerCharacter, ref bool value)
        {
            return value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Language.Suggestions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete

namespace Cody.VisualStudio.Proposals
{
    [Export(typeof(CodyProposalSourceProvider))]
    [Export(typeof(ProposalSourceProviderBase))]
    [Name(nameof(CodyProposalSourceProvider))]
    [Order(Before = "InlineCSharpProposalSourceProvider")]
    [Order(Before = "Highest Priority")]
    [ContentType("any")]
    public class CodyProposalSourceProvider : ProposalSourceProviderBase
    {
        [ImportingConstructor]
        public CodyProposalSourceProvider(SuggestionServiceBase suggestionServiceBase, JoinableTaskContext joinableTaskContext)
        {
            //suggestionServiceBase.ProposalDisplayed += 
        }

        public override Task<ProposalSourceBase> GetProposalSourceAsync(ITextView view, CancellationToken cancel)
        {
            var wpfView = view as IWpfTextView;
            if (wpfView == null) return Task.FromResult<ProposalSourceBase>(null);

            return Task.FromResult<ProposalSourceBase>( new CodyProposalSource(wpfView));
        }
    }

    public class CodyProposalSource : ProposalSourceBase
    {
        private readonly IWpfTextView _view;

        public CodyProposalSource(IWpfTextView view)
        {
            _view = view;
        }

        public override Task<ProposalCollectionBase> RequestProposalsAsync(VirtualSnapshotPoint caret, CompletionState completionState, ProposalScenario scenario,
            char triggeringCharacter, CancellationToken cancel)
        {
            return Task.FromResult<ProposalCollectionBase>(new CodyProposalCollection());
        }
    }

    public class CodyProposalCollection : ProposalCollectionBase
    {
        public CodyProposalCollection()
        {
            var proposedEdit = new ProposedEdit();
            var caret = new VirtualSnapshotPoint();

            var proposal = new Proposal("Test Proposal", new[] { proposedEdit }, caret);

            Proposals = new[] { proposal };
        }

        public override string SourceName { get; } = nameof(CodyProposalSource);
        public override IReadOnlyList<ProposalBase> Proposals { get; }
    }
}

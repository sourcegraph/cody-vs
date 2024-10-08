using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Cody.VisualStudio.Completions
{
    public class CodyProposalSource : ProposalSourceBase
    {
        IAgentService agentService;

        private ITextDocument textDocument;
        private IVsTextView vsTextView;

        public CodyProposalSource(ITextDocument textDocument, IVsTextView vsTextView)
        {
            this.textDocument = textDocument;
            this.vsTextView = vsTextView;
        }

        public override async Task<ProposalCollectionBase> RequestProposalsAsync(
            VirtualSnapshotPoint caret,
            CompletionState completionState,
            ProposalScenario scenario,
            char triggeringCharacter,
            CancellationToken cancel)
        {
            agentService = CodyPackage.AgentServiceInstance;
            if (agentService == null) return null;

            try
            {
                vsTextView.GetLineAndColumn(caret.Position.Position, out int line, out int col);
                
                var autocomplete = new AutocompleteParams
                {
                    Uri = textDocument.FilePath.ToUri(),
                    Position = new Position { Line = line, Character = col },
                    TriggerKind = scenario == ProposalScenario.ExplicitInvocation ? TriggerKind.Invoke : TriggerKind.Automatic
                };
                
                var lineText = caret.Position.Snapshot.GetLineFromLineNumber(line).GetText();
                System.Diagnostics.Debug.WriteLine($"XX: Before call vs:{caret.VirtualSpaces} poslc:{line}:{col} pos:{caret.Position.Position} si:'{completionState?.SelectedItem}' ats:{completionState?.ApplicableToSpan} text:'{lineText}'");
                var autocompleteResult = await agentService.Autocomplete(autocomplete, cancel);

                if(autocompleteResult.Items.Length == 0) System.Diagnostics.Debug.WriteLine($"XX: No autocoplite");
                else System.Diagnostics.Debug.WriteLine($"XX: {autocompleteResult.Items.First().Range.Start.Line}:{autocompleteResult.Items.First().Range.Start.Character} autocomplite:'{autocompleteResult.Items.First().InsertText}'");
                System.Diagnostics.Debug.WriteLine($"XX: si:'{completionState?.SelectedItem}' ats:{completionState?.ApplicableToSpan}");
                System.Diagnostics.Debug.WriteLine($"XX: cs'{textDocument.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(line).GetText()}'");

                var proposalList = new List<ProposalBase>();
                if (autocompleteResult != null && autocompleteResult.Items.Any())
                {
                    foreach (var autocompleteItem in autocompleteResult.Items)
                    {
                        
                        var range = autocompleteItem.Range;
                        vsTextView.GetNearestPosition(range.Start.Line, range.Start.Character, out int startPos, out _);
                        var start = new SnapshotPoint(caret.Position.Snapshot, startPos);
                        vsTextView.GetNearestPosition(range.End.Line, range.End.Character, out int endPos, out _);
                        var end = new SnapshotPoint(caret.Position.Snapshot, endPos);

                        var edits = new List<ProposedEdit>(1)
                        {
                            new ProposedEdit(new SnapshotSpan(start, end), autocompleteItem.InsertText)
                            //new ProposedEdit(new SnapshotSpan(newPoint.Position, 0), autocompleteItem.InsertText)
                        };

                        var proposal = Proposal.TryCreateProposal(null, edits, caret, proposalId: autocompleteItem.Id, flags: ProposalFlags.FormatAfterCommit);

                        if (proposal != null) proposalList.Add(proposal);
                    }

                    var collection = new CodyProposalCollection(proposalList);
                    return collection;
                }
            }
            catch (OperationCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"XX: Canceled");
            }

            return null;
        }
    }
}

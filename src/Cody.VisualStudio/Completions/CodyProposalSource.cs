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
            SimpleLog.Info("CodyProposalSource", "begin");
            agentService = CodyPackage.AgentServiceInstance;
            if (agentService == null)
            {
                SimpleLog.Warning("CodyProposalSource", "Agent service not jet ready");
                return null;
            }

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
                SimpleLog.Info("CodyProposalSource", $"Before autocomplete call vs:{caret.VirtualSpaces} poslc:{line}:{col} si:'{completionState?.SelectedItem}' ats:{completionState?.ApplicableToSpan} text:'{lineText}'");
                var autocompleteResult = await agentService.Autocomplete(autocomplete, cancel);

                if(autocompleteResult.Items.Length == 0) SimpleLog.Info("CodyProposalSource", $"no autocoplite to show");
                else SimpleLog.Info("CodyProposalSource", $"{autocompleteResult.Items.First().Range.Start.Line}:{autocompleteResult.Items.First().Range.Start.Character} autocomplite:'{autocompleteResult.Items.First().InsertText}'");

                var proposalList = new List<ProposalBase>();
                if (autocompleteResult != null && autocompleteResult.Items.Any())
                {
                    foreach (var (item, index) in autocompleteResult.Items.Select((item, index) => (item, index)))
                    {

                        //var range = autocompleteItem.Range;
                        //vsTextView.GetNearestPosition(range.Start.Line, range.Start.Character, out int startPos, out _);
                        //var start = new SnapshotPoint(caret.Position.Snapshot, startPos);
                        //vsTextView.GetNearestPosition(range.End.Line, range.End.Character, out int endPos, out _);
                        //var end = new SnapshotPoint(caret.Position.Snapshot, endPos);

                        //var completionText = autocompleteItem.InsertText;
                        //int insertionStart = caret.IsInVirtualSpace ? completionText.TakeWhile(char.IsWhiteSpace).Count() : 0;
                        //completionText = completionText.Substring(insertionStart);

                        var completionText = autocompleteResult.CompletionEvent.Items[index].InsertText;

                        var edits = new List<ProposedEdit>(1)
                        {
                            new ProposedEdit(new SnapshotSpan(caret.Position, 0), completionText)
                        };

                        var proposal = Proposal.TryCreateProposal(null, edits, caret, proposalId: item.Id, flags: ProposalFlags.FormatAfterCommit);

                        if (proposal != null) proposalList.Add(proposal);
                    }

                    var collection = new CodyProposalCollection(proposalList);
                    return collection;
                }
            }
            catch (OperationCanceledException ex)
            {
                SimpleLog.Error("CodyProposalSource", "canceled");
            }

            return null;
        }

        private Position ToLineColPosition(SnapshotPoint point)
        {
            var containgLine = point.GetContainingLine();
            int col = point.Position - containgLine.Start.Position;
            return new Position { Line = containgLine.LineNumber, Character = col };
        }

        private int ToPosition(ITextSnapshot textSnapshot, int line, int col)
        {
            var containgLine = textSnapshot.GetLineFromLineNumber(line);
            return containgLine.Start.Position + col;
        }
    }
}

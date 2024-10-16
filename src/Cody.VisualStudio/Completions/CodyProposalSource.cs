using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Logging;
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
        private static CancellationTokenSource prevCancellationTokenSource;

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
                prevCancellationTokenSource?.Cancel();

                vsTextView.GetLineAndColumn(caret.Position.Position, out int caretline, out int caretCol);

                var autocomplete = new AutocompleteParams
                {
                    Uri = textDocument.FilePath.ToUri(),
                    Position = new Position { Line = caretline, Character = caretCol + caret.VirtualSpaces },
                    TriggerKind = scenario == ProposalScenario.ExplicitInvocation ? TriggerKind.Invoke : TriggerKind.Automatic
                };

                var lineText = caret.Position.Snapshot.GetLineFromLineNumber(caretline).GetText();
                SimpleLog.Info("CodyProposalSource", $"Before autocomplete call vs:{caret.VirtualSpaces} poslc:{caretline}:{caretCol} si:'{completionState?.SelectedItem}' ats:{completionState?.ApplicableToSpan} text:'{lineText}'");

                var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancel);
                cancellationTokenSource.CancelAfter(4000); //timeout for Autocomplete
                prevCancellationTokenSource = cancellationTokenSource;

                var autocompleteResult = await agentService.Autocomplete(autocomplete, cancellationTokenSource.Token);

                if (autocompleteResult.Items.Length == 0) SimpleLog.Info("CodyProposalSource", $"no autocoplite to show");
                else
                {
                    foreach (var item in autocompleteResult.Items)
                    {
                        SimpleLog.Info("CodyProposalSource", $"autocomplite: {item.Range.Start.Line}:{item.Range.Start.Character}-{item.Range.End.Line}:{item.Range.End.Character} '{item.InsertText}'");
                    }
                }

                var proposalList = new List<ProposalBase>();
                if (autocompleteResult != null && autocompleteResult.Items.Any())
                {
                    foreach (var item in autocompleteResult.Items)
                    {

                        //var range = item.Range;
                        ////vsTextView.GetNearestPosition(range.Start.Line, range.Start.Character, out int startPos, out _);
                        //var startPos = ToPosition(caret.Position.Snapshot, range.Start.Line, range.Start.Character);
                        //var start = new SnapshotPoint(caret.Position.Snapshot, startPos);
                        ////vsTextView.GetNearestPosition(range.End.Line, range.End.Character, out int endPos, out _);
                        //var endPos = ToPosition(caret.Position.Snapshot, range.End.Line, range.End.Character);
                        //var end = new SnapshotPoint(caret.Position.Snapshot, endPos);

                        var completionText = item.InsertText;
                        int insertionStart = caret.IsInVirtualSpace ? completionText.TakeWhile(char.IsWhiteSpace).Count() : 0;
                        completionText = completionText.Substring(insertionStart);

                        var edits = new List<ProposedEdit>(1)
                        {
                            new ProposedEdit(new SnapshotSpan(caret.Position, 0), completionText)
                        };

                        var proposal = Proposal.TryCreateProposal(null, edits, caret, proposalId: item.Id, flags: ProposalFlags.SingleTabToAccept);

                        if (proposal != null) proposalList.Add(proposal);
                    }

                    var collection = new CodyProposalCollection(proposalList);
                    return collection;
                }
            }
            catch (OperationCanceledException)
            {
                SimpleLog.Warning("CodyProposalSource", "canceled");
            }
            catch (Exception ex)
            {
                SimpleLog.Error("CodyProposalSource", ex.ToString());
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

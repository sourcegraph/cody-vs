using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Trace;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions
{
    public class CodyProposalSource : ProposalSourceBase
    {
        private static TraceLogger trace = new TraceLogger(nameof(CodyProposalSource));
        IAgentService agentService;

        private ITextDocument textDocument;
        private IVsTextView vsTextView;

        private static uint session = 0;

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
            session++;
            var stopwatch = new Stopwatch();

            try
            {
                trace.TraceEvent("Begin", "session: {0}", session);

                agentService = CodyPackage.AgentServiceInstance;
                if (agentService == null)
                {
                    trace.TraceMessage("Agent service not jet ready");
                    return null;
                }

                vsTextView.GetLineAndColumn(caret.Position.Position, out int caretline, out int caretCol);

                var autocompleteRequest = new AutocompleteParams
                {
                    Uri = textDocument.FilePath.ToUri(),
                    Position = new Position { Line = caretline, Character = caretCol + caret.VirtualSpaces },
                    TriggerKind = scenario == ProposalScenario.ExplicitInvocation ? TriggerKind.Invoke : TriggerKind.Automatic
                };

                var lineText = caret.Position.Snapshot.GetLineFromLineNumber(caretline).GetText();

                trace.TraceEvent("BeforeRequest", new { session, caret = $"{caretline}:{caretCol}", lineText, virtualSpaces = caret.VirtualSpaces, selectedItem = completionState?.SelectedItem });
                trace.TraceEvent("AutocompliteRequest", autocompleteRequest);

                var autocompleteTask = agentService.Autocomplete(autocompleteRequest, CancellationToken.None);
                var cancelationTask = Task.Delay(5000, cancel);
                stopwatch.Start();
                var resultTask = await Task.WhenAny(autocompleteTask, cancelationTask);
                stopwatch.Stop();
                if (resultTask == cancelationTask)
                {
                    if (cancel.IsCancellationRequested)
                        trace.TraceEvent("AutocompliteCanceled", "session: {0}", session);
                    else
                        trace.TraceEvent("AutocompliteTimeout", "session: {0}", session);

                    return null;
                }

                var autocomplete = await autocompleteTask;

                trace.TraceEvent("CallDuration", "session: {0}, duration: {1}ms", session, stopwatch.ElapsedMilliseconds);

                if (autocomplete.Items.Length == 0)
                {
                    trace.TraceEvent("NoAutocompliteResults", "session: {0}", session);
                }
                else
                {
                    foreach (var item in autocomplete.Items)
                    {
                        trace.TraceEvent("AutocompliteResult", item);
                    }
                }

                var newPosition = caret.Position.TranslateTo(textDocument.TextBuffer.CurrentSnapshot, PointTrackingMode.Positive);
                vsTextView.GetLineAndColumn(newPosition, out int newCaretLine, out int newCaretCol);
                var newText = newPosition.GetContainingLine().GetText();
                trace.TraceEvent("AfterResponse", "session: {0}, newCaret: {1}:{2} lineText:'{3}'", session, newCaretLine, newCaretCol, newText);

                var proposalList = new List<ProposalBase>();
                if (autocomplete.Items.Any())
                {
                    foreach (var item in autocomplete.Items)
                    {

                        //var range = item.Range;
                        ////vsTextView.GetNearestPosition(range.Start.Line, range.Start.Character, out int startPos, out _);
                        //var startPos = ToPosition(caret.Position.Snapshot, range.Start.Line, range.Start.Character);
                        //var start = new SnapshotPoint(caret.Position.Snapshot, startPos);
                        ////vsTextView.GetNearestPosition(range.End.Line, range.End.Character, out int endPos, out _);
                        //var endPos = ToPosition(caret.Position.Snapshot, range.End.Line, range.End.Character);
                        //var end = new SnapshotPoint(caret.Position.Snapshot, endPos);

                        var completionText = item.InsertText;

                        var edits = new List<ProposedEdit>(1)
                        {
                            new ProposedEdit(new SnapshotSpan(caret.Position, 0), completionText)
                        };

                        var proposal = Proposal.TryCreateProposal("Cody", edits, caret,
                            proposalId: CodyProposalSourceProvider.ProposalIdPrefix + item.Id, flags: ProposalFlags.SingleTabToAccept);

                        if (proposal != null) proposalList.Add(proposal);
                        else trace.TraceEvent("ProposalSkipped", "session: {0}", session);
                    }

                    var collection = new CodyProposalCollection(proposalList);
                    return collection;
                }
            }
            catch (Exception ex)
            {
                trace.TraceException(ex);
            }
            finally
            {
                trace.TraceEvent("End", "session: {0}", session);
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

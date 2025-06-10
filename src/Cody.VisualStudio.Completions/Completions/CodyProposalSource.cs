using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Settings;
using Cody.Core.Trace;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions
{
    public class CodyProposalSource : ProposalSourceBase
    {
        private static TraceLogger trace = new TraceLogger(nameof(CodyProposalSource));
        private static readonly StringDifferenceOptions diffOptions = new StringDifferenceOptions(StringDifferenceTypes.Word, 0, true);

        private IAgentService agentService;
        private IUserSettingsService userSettingsService;
        private ITextDocument textDocument;
        private readonly ITextView view;
        private readonly ITextDifferencingService textDifferencingService;
        private static uint sessionCounter = 0;

        private ITextSnapshot trackedSnapshot;

        public CodyProposalSource(ITextDocument textDocument, ITextView view, ITextDifferencingService textDifferencingService)
        {
            this.textDocument = textDocument;
            this.view = view;
            this.textDifferencingService = textDifferencingService;

            trackedSnapshot = textDocument.TextBuffer.CurrentSnapshot;
            textDocument.TextBuffer.ChangedHighPriority += OnTextBufferChanged;
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            trackedSnapshot = e.After;
        }

        public override Task DisposeAsync()
        {
            textDocument.TextBuffer.ChangedHighPriority -= OnTextBufferChanged;
            return base.DisposeAsync();
        }

        public override async Task<ProposalCollectionBase> RequestProposalsAsync(
            VirtualSnapshotPoint caret,
            CompletionState completionState,
            ProposalScenario scenario,
            char triggeringCharacter,
            CancellationToken cancel)
        {
            var session = sessionCounter++;

            try
            {
                trace.TraceEvent("Begin", "session: {0}", session);
                trace.TraceEvent("Scenario", "session: {0}, scenario {1}", session, scenario);

                agentService = CodyPackage.AgentService;
                userSettingsService = CodyPackage.UserSettingsService;

                if (!ShouldDoProposals(completionState, scenario, agentService, userSettingsService))
                {
                    return null;
                }

                UpdateCaretAndCompletion(ref caret, ref completionState);

                var caretPosition = ToLineColPosition(caret.Position);
                var autocompleteRequest = new AutocompleteParams
                {
                    Uri = textDocument.FilePath.ToUri(),
                    Position = caretPosition,
                    TriggerKind = scenario == ProposalScenario.ExplicitInvocation ? TriggerKind.Invoke : TriggerKind.Automatic
                };

                if (completionState != null)
                {
                    var completionStart = ToLineColPosition(completionState.ApplicableToSpan.Start);
                    var completionEnd = ToLineColPosition(completionState.ApplicableToSpan.End);
                    autocompleteRequest.SelectedCompletionInfo = new SelectedCompletionInfo()
                    {
                        Text = completionState.SelectedItem,
                        Range = new Range
                        {
                            Start = completionStart,
                            End = completionEnd
                        }
                    };
                }

                trace.TraceEvent("BeforeRequest", new
                {
                    session,
                    caret.Position.Position,
                    caret = $"{caretPosition.Line}:{caretPosition.Character}",
                    lineText = caret.Position.Snapshot.GetLineFromLineNumber(caretPosition.Line).GetText(),
                    virtualSpaces = caret.VirtualSpaces,
                    selectedItem = completionState?.SelectedItem,
                    completionState?.IsSoftSelection,
                    applicableToSpan = completionState?.ApplicableToSpan.ToString(),
                    completionState?.IsSuggestion,
                    completionState?.IsSnippet
                });

                var autocomplete = await GetAutocompleteItems(autocompleteRequest, session, cancel);

                if (autocomplete == null)
                {
                    trace.TraceEvent("Result", "session: {0}, No results (null)", session);
                    return null;
                }
                else if (autocomplete.InlineCompletionItems.Length == 0 && autocomplete.DecoratedEditItems.Length == 0)
                {
                    trace.TraceEvent("Result", "session: {0}, No results (empty)", session);
                    return null;
                }
                else
                {
                    if (autocomplete.InlineCompletionItems.Any())
                    {
                        foreach (var item in autocomplete.InlineCompletionItems)
                            trace.TraceEvent("AutocompliteResult", item);
                    }

                    if (autocomplete.DecoratedEditItems.Any())
                    {
                        foreach (var item in autocomplete.DecoratedEditItems)
                            trace.TraceEvent("AutoEditResult", new { item.InsertText, item.OriginalText });
                    }

                }

                CodyProposalCollection collection = null;
                if (autocomplete.DecoratedEditItems.Any())
                    collection = CreateAutoeditProposals(autocomplete, caret, completionState, session);
                else if (autocomplete.InlineCompletionItems.Any())
                    collection = CreateAutocompleteProposals(autocomplete, caret, completionState, session);

                if (collection == null || collection.Proposals.Count == 0)
                {
                    trace.TraceEvent("NoProposalsToDisplay", "session: {0}", session);
                    return null;
                }

                if (cancel.IsCancellationRequested)
                {
                    trace.TraceEvent("AutocompliteCanceled", "session: {0}", session);
                    return null;
                }

                return collection;
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

        private bool ShouldDoProposals(CompletionState completionState, ProposalScenario scenario, IAgentService agentService, IUserSettingsService userSettings)
        {
            if (completionState != null && (completionState.IsSnippet || completionState.IsSuggestion || completionState.IsPreprocessorDirective))
            {
                trace.TraceMessage("Scenario without autocomplete");
                return false;
            }

            if (agentService == null)
            {
                trace.TraceMessage("Agent service not jet ready");
                return false;
            }

            if (userSettings == null)
            {
                trace.TraceMessage("User settings service not jet ready");
                return false;
            }

            if (!userSettings.AutomaticallyTriggerCompletions && scenario != ProposalScenario.ExplicitInvocation)
            {
                trace.TraceMessage("Automatic triggering autocomplete disabled");
                return false;
            }

            return true;
        }

        private void UpdateCaretAndCompletion(ref VirtualSnapshotPoint caret, ref CompletionState completionState)
        {
            if (trackedSnapshot == null || trackedSnapshot.Version.VersionNumber < caret.Position.Snapshot.Version.VersionNumber)
            {
                trace.TraceEvent("TrackinSnapshotFailed");
                return;
            }

            caret = caret.TranslateTo(trackedSnapshot);
            if (completionState != null)
            {
                completionState = completionState.TranslateTo(trackedSnapshot);
            }
        }

        private async Task<AutocompleteResult> GetAutocompleteItems(AutocompleteParams autocompleteRequest, uint session, CancellationToken cancel)
        {
            trace.TraceEvent("AutocompliteRequest", autocompleteRequest);
            var stopwatch = new Stopwatch();

            var autocompleteCancel = new CancellationTokenSource();
            var autocompleteTask = agentService.Autocomplete(autocompleteRequest, autocompleteCancel.Token);
            var cancelationTask = Task.Delay(8000, cancel);
            stopwatch.Start();
            var resultTask = await Task.WhenAny(autocompleteTask, cancelationTask);
            stopwatch.Stop();

            if (resultTask == cancelationTask)
            {
                if (cancel.IsCancellationRequested)
                    trace.TraceEvent("AutocompliteCanceled", "session: {0}", session);
                else
                    trace.TraceEvent("AutocompliteTimeout", "session: {0}", session);

                autocompleteCancel.Cancel();
                return null;
            }

            var autocomplete = await autocompleteTask;
            trace.TraceEvent("CallDuration", "session: {0}, duration: {1}ms", session, stopwatch.ElapsedMilliseconds);

            return autocomplete;
        }

        private CodyProposalCollection CreateAutocompleteProposals(AutocompleteResult autocomplete, VirtualSnapshotPoint caret, CompletionState completionState, uint session)
        {
            var proposalList = new List<ProposalBase>();

            foreach (var item in autocomplete.InlineCompletionItems)
            {
                var snapshot = caret.Position.Snapshot;
                var startPos = ToPosition(snapshot, item.Range.Start.Line, item.Range.Start.Character);
                var endPos = ToPosition(snapshot, item.Range.End.Line, item.Range.End.Character);

                var (actualText, offset) = GetLinesOfOriginalText(snapshot, startPos, endPos);

                var modText = actualText;
                if (completionState != null)
                {
                    var span = completionState.ApplicableToSpan;
                    modText = ReplaceRange(actualText, offset, span.Start.Position, span.End.Position, completionState.SelectedItem);
                }

                var completionText = item.InsertText;
                if (caret.IsInVirtualSpace) completionText = AdjustVirtualSpaces(completionText, caret.VirtualSpaces, session);

                var newText = ReplaceRange(actualText, offset, startPos, endPos, completionText);

                var diffs = FindDifferences(modText, newText);

                if (diffs.Any())
                {
                    var edits = diffs.Select(x => new ProposedEdit(new SnapshotSpan(snapshot, x.Position + offset, x.RemovedText.Length), x.AddedText)).ToList();

                    var proposal = Proposal.TryCreateProposal("Cody", edits, caret,
                        completionState: completionState,
                        proposalId: CodyProposalSourceProvider.ProposalIdPrefix + item.Id,
                        flags: ProposalFlags.SingleTabToAccept | ProposalFlags.FormatAfterCommit);

                    if (proposal != null) proposalList.Add(proposal);
                    else trace.TraceEvent("ProposalInvalid", "session: {0}", session);
                }
                else
                {
                    trace.TraceEvent("NothingNewToPropose", "session: {0}", session);
                }
            }

            var collection = new CodyProposalCollection(proposalList);
            return collection;
        }

        private (string text, int offset) GetLinesOfOriginalText(ITextSnapshot snapshot, int startPos, int endPos)
        {
            int? offset = null;
            var text = new StringBuilder();
            var startLine = snapshot.GetLineNumberFromPosition(startPos);
            var endLine = snapshot.GetLineNumberFromPosition(endPos);

            for (int i = startLine; i <= endLine; i++)
            {
                var line = snapshot.GetLineFromLineNumber(i);
                if (!offset.HasValue) offset = line.Start.Position;
                text.Append(line.GetTextIncludingLineBreak());
            }

            return (text.ToString(), offset.Value);
        }

        private CodyProposalCollection CreateAutoeditProposals(AutocompleteResult autocomplete, VirtualSnapshotPoint caret, CompletionState completionState, uint session)
        {
            var proposalList = new List<ProposalBase>();

            foreach (var item in autocomplete.DecoratedEditItems)
            {
                var edits = new List<ProposedEdit>();
                var snapshot = caret.Position.Snapshot;

                var range = FindRange(snapshot, item.Range.Start.Line, item.OriginalText);
                if (range == null)
                {
                    trace.TraceEvent("TextMismatch");
                    return null;
                }

                var actualText = snapshot.GetText(range.Start, range.End - range.Start);

                if (completionState != null)
                {
                    var before = actualText;
                    var span = completionState.ApplicableToSpan;
                    actualText = ReplaceRange(actualText, range.Start, span.Start.Position, span.End.Position, completionState.SelectedItem);
                    trace.TraceEvent("IncludeCompletionState", new { before, after = actualText });
                }

                var diffs = FindDifferences(actualText, item.InsertText);


                foreach (var diff in diffs)
                {
                    trace.TraceEvent("DiffItem", diff);
                    if (diff.AddedText.All(x => char.IsWhiteSpace(x)) && diff.RemovedText.All(x => char.IsWhiteSpace(x))) continue;

                    var addedText = diff.AddedText;
                    if (caret.IsInVirtualSpace && diff == diffs.First()) addedText = AdjustVirtualSpaces(addedText, caret.VirtualSpaces, session);
                    if (diff == diffs.Last()) addedText = addedText.TrimEnd();

                    var proposedChange = new ProposedEdit(new SnapshotSpan(snapshot, range.Start + diff.Position, diff.RemovedText.Length), addedText);
                    edits.Add(proposedChange);
                }

                var proposal = Proposal.TryCreateProposal("Cody", edits, caret,
                    completionState: completionState,
                    proposalId: CodyProposalSourceProvider.ProposalIdPrefix + item.Id, flags: ProposalFlags.SingleTabToAccept | ProposalFlags.FormatAfterCommit);

                if (proposal != null) proposalList.Add(proposal);
                else trace.TraceEvent("ProposalInvalid", "session: {0}", session);
            }

            return new CodyProposalCollection(proposalList);
        }

        private TextRange FindRange(ITextSnapshot snapshot, int startLine, string originalText)
        {
            var lineCount = CountLines(originalText);
            int? offset = null;
            var textBlock = new StringBuilder();
            for (int lineNum = startLine; lineNum < startLine + lineCount; lineNum++)
            {
                var line = snapshot.GetLineFromLineNumber(lineNum);
                if (line == null) break;
                if (!offset.HasValue) offset = line.Start.Position;
                textBlock.Append(line.GetTextIncludingLineBreak());
            }

            var block = textBlock.ToString();
            var index = block.IndexOf(originalText);
            if (index >= 0) return new TextRange
            {
                Start = offset.Value + index,
                End = offset.Value + index + originalText.Length
            };

            return null;
        }

        private IReadOnlyList<Difference> FindDifferences(string oldText, string newText)
        {
            var results = new List<Difference>();
            var diffs = textDifferencingService.DiffStrings(oldText, newText, diffOptions);
            foreach (var diff in diffs.Differences)
            {
                var spanOld = diffs.LeftDecomposition.GetSpanInOriginal(diff.Left);
                var spanNew = diffs.RightDecomposition.GetSpanInOriginal(diff.Right);
                var removedText = oldText.Substring(spanOld.Start, spanOld.Length);
                var addedText = newText.Substring(spanNew.Start, spanNew.Length);
                var result = new Difference(removedText, addedText, spanOld.Start);
                results.Add(result);
            }

            return results;
        }

        private string AdjustVirtualSpaces(string text, int virtualSpaces, uint session)
        {
            var toSkip = Math.Min(text.TakeWhile(x => x == ' ').Count(), virtualSpaces);
            var result = text.Substring(toSkip);
            trace.TraceEvent("VirtualSpaceAdjustion", "session: {0}", session);

            return result;
        }

        private static string ReplaceRange(string input, int offset, int startRange, int endRange, string replacementText)
        {
            return input.Substring(0, startRange - offset) + replacementText + input.Substring(endRange - offset);
        }

        private static int CountLines(string text)
        {
            int count = 0;
            using (var reader = new StringReader(text))
            {
                while (reader.ReadLine() != null) count++;
            }

            return count;
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

        public class TextRange
        {
            public int Start { get; set; }
            public int End { get; set; }
        }

    }
}

using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Settings;
using Cody.Core.Trace;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
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

        private IAgentService agentService;
        private ITextDocument textDocument;
        private readonly ITextView view;
        private static uint sessionCounter = 0;

        private ITextSnapshot trackedSnapshot;

        public CodyProposalSource(ITextDocument textDocument, ITextView view)
        {
            this.textDocument = textDocument;
            this.view = view;
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

                if (!ShouldDoProposals(completionState, scenario, agentService, CodyPackage.UserSettingsService))
                {
                    trace.TraceEvent("SkipScenario", "Skip scenario {0}", scenario);
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

                if (autocomplete == null || autocomplete.Items.Length == 0) trace.TraceEvent("AutocompliteResult", "session: {0}, No results", session);
                else foreach (var item in autocomplete.Items) trace.TraceEvent("AutocompliteResult", item);

                var newPosition = caret.Position.TranslateTo(textDocument.TextBuffer.CurrentSnapshot, PointTrackingMode.Positive);
                var newText = newPosition.GetContainingLine().GetText();
                trace.TraceEvent("AfterResponse", "session: {0}, newCaret: {1} lineText:'{2}'", session, newPosition.Position, newText);

                var collection = CreateProposals(autocomplete, caret, completionState, session);
                if (cancel.IsCancellationRequested) trace.TraceEvent("AutocompliteCanceled2", "session: {0}", session);

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
            if (scenario == ProposalScenario.CaretMove ||
                (completionState != null && (completionState.IsSnippet || completionState.IsSuggestion || completionState.IsPreprocessorDirective)))
            {
                trace.TraceMessage("Scenario without autocomplete");
                return false;
            }

            if (agentService == null)
            {
                trace.TraceMessage("Agent service not jet ready");
                return false;
            }

            if (userSettings != null && !userSettings.AutomaticallyTriggerCompletions && scenario != ProposalScenario.ExplicitInvocation)
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

        private CodyProposalCollection CreateProposals(AutocompleteResult autocomplete, VirtualSnapshotPoint caret, CompletionState completionState, uint session)
        {
            var proposalList = new List<ProposalBase>();
            if (autocomplete != null && autocomplete.Items.Any())
            {
                foreach (var item in autocomplete.Items)
                {
                    var completionText = AdjustCompletionText(caret, completionState, item.InsertText, session);

                    int lenght = 0;
                    if (item.Range.Start.Character != item.Range.End.Character || item.Range.Start.Line != item.Range.End.Line)
                    {
                        var startPos = ToPosition(caret.Position.Snapshot, item.Range.Start.Line, item.Range.Start.Character);
                        var endPos = ToPosition(caret.Position.Snapshot, item.Range.End.Line, item.Range.End.Character);

                        lenght = endPos - startPos;
                    }

                    var edits = new List<ProposedEdit>(1)
                        {
                            new ProposedEdit(new SnapshotSpan(caret.Position, lenght), completionText)
                        };

                    var proposal = Proposal.TryCreateProposal("Cody", edits, caret,
                        completionState: completionState,
                        proposalId: CodyProposalSourceProvider.ProposalIdPrefix + item.Id, flags: ProposalFlags.SingleTabToAccept | ProposalFlags.FormatAfterCommit);

                    if (proposal != null) proposalList.Add(proposal);
                    else trace.TraceEvent("ProposalSkipped", "session: {0}", session);
                }

            }

            var collection = new CodyProposalCollection(proposalList);
            return collection;
        }

        private string AdjustCompletionText(VirtualSnapshotPoint caret, CompletionState completionState, string completionText, uint session)
        {
            if (caret.IsInVirtualSpace)
            {
                var toSkip = Math.Min(caret.VirtualSpaces, completionText.TakeWhile(char.IsWhiteSpace).Count());
                completionText = completionText.Substring(toSkip);
                trace.TraceEvent("VirtualSpaceAdjustion", "session: {0}", session);
            }

            var newLineChars = view.Options.GetOptionValue(DefaultOptions.NewLineCharacterOptionId);
            completionText = completionText.ConvertLineBreaks(newLineChars);

            if (completionState == null || completionText == null) return completionText;
            var enteredText = completionState.ApplicableToSpan.GetText();
            var common = completionState.SelectedItem.TrimPrefix(enteredText, StringComparison.Ordinal);

            if (completionText.StartsWith(common)) return completionText.Substring(common.Length);
            else
            {
                trace.TraceEvent("ProposalSkipped", "session: {0}, IntellisenceMistmatch", session);
                return string.Empty;
            }
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

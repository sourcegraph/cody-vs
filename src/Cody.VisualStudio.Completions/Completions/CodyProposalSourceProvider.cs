using Cody.Core.Agent.Protocol;
using Cody.Core.Trace;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Language.Suggestions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions
{
    [Export(typeof(ProposalSourceProviderBase))]
    [Name(nameof(CodyProposalSourceProvider))]
    [Order(Before = "InlineCSharpProposalSourceProvider")]
    //[Order(Before = "IntelliCodeCSharpProposalSource")]
    [Order(Before = "Highest Priority")]
    [ContentType("any")]
    public class CodyProposalSourceProvider : ProposalSourceProviderBase, IDisposable
    {
        private static TraceLogger trace = new TraceLogger(nameof(CodyProposalSourceProvider));

        private readonly ITextDocumentFactoryService textDocumentFactoryService;
        private readonly SuggestionServiceBase suggestionServiceBase;
        private readonly ITextDifferencingService textDifferencingService;

        public const string ProposalIdPrefix = "cody";

        //[ImportMany] public IEnumerable<ProposalSourceProviderBase> Input { get; set; }

        [ImportingConstructor]
        public CodyProposalSourceProvider(
            ITextDocumentFactoryService textDocumentFactoryService,
            SuggestionServiceBase suggestionServiceBase,
            ITextDifferencingSelectorService textDifferencingSelectorService)
        {
            this.textDocumentFactoryService = textDocumentFactoryService;
            this.suggestionServiceBase = suggestionServiceBase;
            this.textDifferencingService = textDifferencingSelectorService.DefaultTextDifferencingService;

            //suggestionServiceBase.ProposalRejected += OnProposalRejected;
            this.suggestionServiceBase.ProposalDisplayed += OnProposalDisplayed;
            this.suggestionServiceBase.SuggestionDismissed += OnSuggestionDismissed;
            this.suggestionServiceBase.SuggestionAccepted += OnSuggestionAccepted;
        }

        private bool IsReadyAndIsCodyProposal(string providerName, string proposalId)
        {
            // e.ProviderName always return 'IntelliCodeLineCompletions' which is a VS bug
            return CodyPackage.AgentService != null &&
                (providerName == "IntelliCodeLineCompletions" || providerName == nameof(CodyProposalSourceProvider)) &&
                proposalId.StartsWith(ProposalIdPrefix);
        }

        private void OnSuggestionDismissed(object sender, SuggestionDismissedEventArgs e)
        {
            trace.TraceEvent("SuggestionDismissed", "reason: {0}", e.Reason);
        }

        private void OnProposalDisplayed(object sender, ProposalDisplayedEventArgs e)
        {
            if (IsReadyAndIsCodyProposal(e.ProviderName, e.OriginalProposal.ProposalId))
            {
                var completionId = e.OriginalProposal.ProposalId.Substring(ProposalIdPrefix.Length);
                var completionItem = new CompletionItemParams() { CompletionID = completionId };
                trace.TraceEvent("ProposalDisplayed", completionId);
                CodyPackage.AgentService.CompletionSuggested(completionItem);
            }

            if (CodyPackage.TestingSupportService != null)
            {
                CodyPackage.TestingSupportService.SetAutocompleteSuggestion(
                    e.OriginalProposal.ProposalId,
                    e.OriginalProposal.ProposalId.StartsWith(ProposalIdPrefix),
                    e.OriginalProposal.Edits.First().ReplacementText);
            }
        }

        private void OnSuggestionAccepted(object sender, SuggestionAcceptedEventArgs e)
        {
            if (IsReadyAndIsCodyProposal(e.ProviderName, e.OriginalProposal.ProposalId))
            {
                var completionId = e.OriginalProposal.ProposalId.Substring(ProposalIdPrefix.Length);
                var completionItem = new CompletionItemParams() { CompletionID = completionId };
                trace.TraceEvent("SuggestionAccepted", completionId);
                CodyPackage.AgentService.CompletionAccepted(completionItem);
            }
        }

        public async override Task<ProposalSourceBase> GetProposalSourceAsync(ITextView view, CancellationToken cancel)
        {
            trace.TraceEvent("Enter");
            IWpfTextView wpfTextView = view as IWpfTextView;
            if (wpfTextView != null && view.Roles.Contains("DOCUMENT") && view.Roles.Contains("EDITABLE"))
            {
                textDocumentFactoryService.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out var document);
                if (document != null)
                {
                    trace.TraceEvent("CreateProposalSource", "Created for '{0}'", document.FilePath);
                    return view.Properties.GetOrCreateSingletonProperty(() => new CodyProposalSource(document, view, textDifferencingService));
                }
            }

            return null;
        }

        public void Dispose()
        {
            suggestionServiceBase.ProposalDisplayed -= OnProposalDisplayed;
            suggestionServiceBase.SuggestionDismissed -= OnSuggestionDismissed;
            suggestionServiceBase.SuggestionAccepted -= OnSuggestionAccepted;
        }
    }
}

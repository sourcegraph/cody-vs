using Cody.Core.Agent.Protocol;
using Cody.Core.Trace;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Language.Suggestions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions
{
    [Export(typeof(ProposalSourceProviderBase))]
    [Name(nameof(CodyProposalSourceProvider))]
    [Order(Before = "InlineCSharpProposalSourceProvider")]
    [Order(Before = "Highest Priority")]
    [ContentType("any")]
    public class CodyProposalSourceProvider : ProposalSourceProviderBase
    {
        private static TraceLogger trace = new TraceLogger(nameof(CodyProposalSourceProvider));

        private readonly ITextDocumentFactoryService textDocumentFactoryService;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService;

        [ImportingConstructor]
        public CodyProposalSourceProvider(
            ITextDocumentFactoryService textDocumentFactoryService,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService,
            SuggestionServiceBase suggestionServiceBase)
        {
            this.textDocumentFactoryService = textDocumentFactoryService;
            this.editorAdaptersFactoryService = editorAdaptersFactoryService;

            suggestionServiceBase.ProposalDisplayed += OnProposalDisplayed;
            suggestionServiceBase.SuggestionAccepted += OnSuggestionAccepted;
        }

        private void OnProposalDisplayed(object sender, ProposalDisplayedEventArgs e)
        {
            if (CodyPackage.AgentServiceInstance != null && e.ProviderName == nameof(CodyProposalSourceProvider))
            {
                var completionItem = new CompletionItemParams() { CompletionID = e.OriginalProposal.ProposalId };
                CodyPackage.AgentServiceInstance.CompletionSuggested(completionItem);
            }
        }

        private void OnSuggestionAccepted(object sender, SuggestionAcceptedEventArgs e)
        {
            if (CodyPackage.AgentServiceInstance != null && e.ProviderName == nameof(CodyProposalSourceProvider))
            {
                var completionItem = new CompletionItemParams() { CompletionID = e.OriginalProposal.ProposalId };
                CodyPackage.AgentServiceInstance.CompletionAccepted(completionItem);
            }
        }

        public async override Task<ProposalSourceBase> GetProposalSourceAsync(ITextView view, CancellationToken cancel)
        {
            trace.TraceEvent("begin");
            IWpfTextView wpfTextView = view as IWpfTextView;
            if (wpfTextView != null && view.Roles.Contains("DOCUMENT") && view.Roles.Contains("EDITABLE"))
            {
                textDocumentFactoryService.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out var document);
                var vsTextView = editorAdaptersFactoryService.GetViewAdapter(view);
                if (document != null && vsTextView != null)
                {
                    trace.TraceEvent("CreateProposalSource", "Created for '{0}'", document.FilePath);
                    return view.Properties.GetOrCreateSingletonProperty(() => new CodyProposalSource(document, vsTextView));
                }
            }

            return null;
        }
    }
}

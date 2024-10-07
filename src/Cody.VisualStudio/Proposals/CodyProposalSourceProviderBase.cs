using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cody.Core.Logging;
using Cody.VisualStudio.Inf;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Language.Suggestions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
        private static ILog _logger;

        [ImportingConstructor]
        public CodyProposalSourceProvider(SuggestionServiceBase suggestionServiceBase, JoinableTaskContext joinableTaskContext)
        {
            InitializeLogger();
            //suggestionServiceBase.ProposalDisplayed += 
        }

        public override Task<ProposalSourceBase> GetProposalSourceAsync(ITextView view, CancellationToken cancel)
        {
            var wpfView = view as IWpfTextView;
            if (wpfView == null) return Task.FromResult<ProposalSourceBase>(null);

            _logger?.Debug("Init.");

            return Task.FromResult<ProposalSourceBase>( new CodyProposalSource(wpfView, _logger));
        }

        private CodyPackage GetPackage()
        {
            // copy&paste from CodyToolWindow 

            var vsShell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));
            IVsPackage package;
            var guidPackage = new Guid(CodyPackage.PackageGuidString);
            if (vsShell.IsPackageLoaded(ref guidPackage, out package) == Microsoft.VisualStudio.VSConstants.S_OK)
            {
                var currentPackage = (CodyPackage)package;
                return currentPackage;
            }

            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.Create();
            logger.Error("Couldn't get logger instance from the CodyPackage.");

            return null;
        }

        private void InitializeLogger()
        {
            try
            {
                if (_logger == null)
                {
                    var package = GetPackage();
                    if (package != null)
                        _logger = package.Logger;

                    _logger?.Debug("Init.");
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
    }

    public class CodyProposalSource : ProposalSourceBase
    {
        private readonly IWpfTextView _view;
        private static ILog _logger;

        public CodyProposalSource(IWpfTextView view, ILog logger)
        {
            _view = view;

            if (_logger == null && logger != null)
            {
                _logger = logger;

                _logger.Debug("Init.");
            }
        }

        public override Task<ProposalCollectionBase> RequestProposalsAsync(VirtualSnapshotPoint caret, CompletionState completionState, ProposalScenario scenario,
            char triggeringCharacter, CancellationToken cancel)
        {
            _logger?.Debug("Init.");

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

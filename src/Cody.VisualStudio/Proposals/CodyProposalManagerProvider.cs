using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Cody.Core.Logging;
using Cody.VisualStudio.Inf;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Cody.VisualStudio.Proposals
{

    [Export(typeof(ProposalManagerProviderBase))]
    [Name(nameof(CodyProposalManagerProvider))]
    [Order(Before = "InlineCSharpProposalManagerProvider")]
    [Order(Before = "Highest Priority")]
    [ContentType("any")]
    public class CodyProposalManagerProvider: ProposalManagerProviderBase
    {
        private static ILog _logger;

        public override Task<ProposalManagerBase> GetProposalManagerAsync(ITextView view, CancellationToken cancel)
        {
            InitializeLogger();

            return Task.FromResult<ProposalManagerBase>(new CodyProposalManager(_logger));
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
    }

    public class CodyProposalManager : ProposalManagerBase
    {
        private readonly ILog _logger;

        public CodyProposalManager(ILog logger)
        {
            _logger = logger;

            _logger?.Debug("Created.");
        }

        public override bool TryGetIsProposalPosition(VirtualSnapshotPoint caret, ProposalScenario scenario, char triggerCharacter, ref bool value)
        {
            _logger?.Debug("Init.");

            return true; // return value;
        }
    }
}

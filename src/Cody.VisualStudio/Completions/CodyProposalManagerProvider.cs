using Cody.Core.Logging;
using Cody.Core.Trace;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions
{
    [Export(typeof(ProposalManagerProviderBase))]
    [Name(nameof(CodyProposalManagerProvider))]
    [Order(Before = "InlineCSharpProposalManagerProvider")]
    [Order(Before = "Highest Priority")]
    [ContentType("any")]
    public class CodyProposalManagerProvider : ProposalManagerProviderBase
    {
        private static TraceLogger trace = new TraceLogger(nameof(CodyProposalManagerProvider));

        public async override Task<ProposalManagerBase> GetProposalManagerAsync(ITextView view, CancellationToken cancel)
        {
            trace.TraceEvent("begin");
            return new CodyProposalManager();
        }
    }
}

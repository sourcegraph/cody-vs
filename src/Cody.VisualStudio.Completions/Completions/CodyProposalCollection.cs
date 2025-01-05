using Microsoft.VisualStudio.Language.Proposals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Completions
{
    public class CodyProposalCollection : ProposalCollection
    {
        public CodyProposalCollection(IReadOnlyList<ProposalBase> proposals) : base(nameof(CodyProposalSource), proposals)
        {
        }
    }
}

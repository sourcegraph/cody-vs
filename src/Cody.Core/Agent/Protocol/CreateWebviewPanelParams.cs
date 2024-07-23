using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class CreateWebviewPanelParams
    {
        public string Handle { get; set; }

        public string ViewType { get; set; }

        public string Title { get; set; }

        public ShowOptions ShowOptions { get; set; }

        public DefiniteWebviewOptions Options { get; set; }
    }

    public class ShowOptions
    {
        public bool PreserveFocus { get; set; }

        public int ViewColumn { get; set; }
    }
}

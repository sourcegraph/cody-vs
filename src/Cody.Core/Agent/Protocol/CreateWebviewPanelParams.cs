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

        public override string ToString()
        {
            return $"Handle:'{Handle} ViewType:'{ViewType}' 'Title:{Title}' ShowOptions:[ {ShowOptions} ] Options:[ {Options} ]";
        }
    }

    public class ShowOptions
    {
        public bool PreserveFocus { get; set; }

        public int ViewColumn { get; set; }

        public override string ToString()
        {
            return $"PreserveFocus:{PreserveFocus} ViewColumn:{ViewColumn}";
        }
    }
}

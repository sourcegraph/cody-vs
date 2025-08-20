using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class UserEditPromptResult
    {
        public string Instruction { get; set; }

        public string SelectedModelId { get; set; }
    }
}

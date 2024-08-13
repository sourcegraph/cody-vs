using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class AgentMethodAttribute : Attribute
    {
        public AgentMethodAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class AgentNotificationAttribute : Attribute
    {
        public AgentNotificationAttribute(string name, bool deserializeToSingleObject = false)
        {
            Name = name;
            DeserializeToSingleObject = deserializeToSingleObject;
        }

        public string Name { get; private set; }

        public bool DeserializeToSingleObject { get; private set; }
    }
}

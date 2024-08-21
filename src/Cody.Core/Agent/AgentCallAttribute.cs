using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    /// <summary>
    /// Use this attribute to mark methods that invoke notifications or requests to agent (client -> server).
    /// Method invoking notifications must return void. Requests must return Task.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AgentCallAttribute : Attribute
    {
        public AgentCallAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

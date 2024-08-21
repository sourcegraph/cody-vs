using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    /// <summary>
    /// Use this attribute to mark methods that handle notifications or requests from the agent (server -> client).
    /// Method handling notifications must return void. Requests must return Task.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AgentCallbackAttribute : Attribute
    {
        public AgentCallbackAttribute(string name, bool deserializeToSingleObject = false)
        {
            Name = name;
            DeserializeToSingleObject = deserializeToSingleObject;
        }

        public string Name { get; private set; }

        public bool DeserializeToSingleObject { get; private set; }
    }
}

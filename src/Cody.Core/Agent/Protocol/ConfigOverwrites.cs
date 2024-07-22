using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ConfigOverwrites
    {
        public string ChatModel { get; set; }

        public int ChatModelMaxTokens { get; set; }

        public string FastChatModel { get; set; }

        public int FastChatModelMaxTokens { get; set; }

        public string CompletionModel { get; set; }

        public int CompletionModelMaxTokens { get; set; }

        public string Provider { get; set; }

        public bool SmartContextWindow { get; set; }
    }
}

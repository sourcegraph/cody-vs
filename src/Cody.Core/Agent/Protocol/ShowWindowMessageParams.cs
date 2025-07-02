using System.Collections.Generic;

namespace Cody.Core.Agent.Protocol
{

    public class ShowWindowMessageParams
    {
        public SeverityEnum Severity { get; set; }

        public string Message { get; set; }

        public MessageOptions Options { get; set; }

        public List<string> Items { get; set; }

    }

    public enum SeverityEnum
    {
        Error,

        Warning,

        Information
    }

    public class MessageOptions
    {
        public bool? Modal { get; set; }

        public string Detail { get; set; }
    }


}

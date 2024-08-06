using System;

namespace Cody.Core.Agent
{
    public class SetHtmlEvent : EventArgs
    {
        public string Handle { get; set; }
        public string Html { get; set; }
    }

    public class SetWebviewRequestEvent : EventArgs
    {
        public string Handle { get; set; }
        public string Messsage { get; set; }
    }

    public class AgentResponseEvent : EventArgs
    {
        //string id, string stringEncodedMessage
        public string Id { get; set; }
        public string StringEncodedMessage { get; set; }
    }
}
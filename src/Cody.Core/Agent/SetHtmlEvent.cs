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
}
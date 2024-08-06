﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    public class ClientCapabilities
    {
        public string Completions { get; set; }
        public ChatCapability? Chat { get; set; }
        public Capability? Git { get; set; }
        public Capability? ProgressBars { get; set; }
        public Capability? Edit { get; set; }
        public Capability? EditWorkspace { get; set; }
        public Capability? UntitledDocuments { get; set; }
        public Capability? ShowDocument { get; set; }
        public Capability? CodeLenses { get; set; }
        public ShowWindowMessageCapability? ShowWindowMessage { get; set; }
        public Capability? Ignore { get; set; }
        public Capability? CodeActions { get; set; }
        public string WebviewMessages { get; set; }
        public WebviewCapabilities Webview { get; set; }
    }

    public enum Capability
    {
        None,
        Enabled
    }

    public enum ChatCapability
    {
        None,
        Streaming
    }

    public enum ShowWindowMessageCapability
    {
        Notification,
        Request
    }
}

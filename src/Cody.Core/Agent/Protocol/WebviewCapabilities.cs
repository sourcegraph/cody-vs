namespace Cody.Core.Agent.Protocol
{
    public class WebviewCapabilities
    {
        public WebviewView View { get; set; }
        public string CspSource { get; set; }
        public string WebviewBundleServingPrefix { get; set; }
        public bool? SkipResourceRelativization { get; set; }
        public string InjectScript { get; set; }
        public string InjectStyle { get; set; }
    }

    public enum WebviewView
    {
        Single,
        Multiple
    }
}

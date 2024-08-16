namespace Cody.Core.Agent.Protocol
{
    public class WebviewCapabilities
    {
        public string CspSource { get; set; }
        public string WebviewBundleServingPrefix { get; set; }
        public WebviewView View { get; set; }
    }

    public enum WebviewView
    {
        Single,
        Multiple
    }
}

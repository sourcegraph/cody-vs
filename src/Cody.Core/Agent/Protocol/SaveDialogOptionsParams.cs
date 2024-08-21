namespace Cody.Core.Agent.Protocol
{
    public class SaveDialogOptionsParams
    {
        public string DefaultUri { get; set; }
        public string SaveLabel { get; set; }
        public object Filters { get; set; } // Typescript: Record<string, string[]> | undefined | null
        public string Title { get; set; }
    }
}

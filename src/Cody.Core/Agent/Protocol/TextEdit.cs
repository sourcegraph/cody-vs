using Cody.Core.Agent.Protocol;
using JsonSubTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent.Protocol
{
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(ReplaceTextEdit), "replace")]
    [JsonSubtypes.KnownSubType(typeof(InsertTextEdit), "insert")]
    [JsonSubtypes.KnownSubType(typeof(DeleteTextEdit), "delete")]
    public abstract class TextEdit
    {
        public string Type { get; set; }
        public WorkspaceEditEntryMetadata Metadata { get; set; }
    }

    public class ReplaceTextEdit : TextEdit
    {
        public Range Range { get; set; }
        public string Value { get; set; }
    }

    public class InsertTextEdit : TextEdit
    {
        public Position Position { get; set; }
        public string Value { get; set; }
    }

    public class DeleteTextEdit : TextEdit
    {
        public Range Range { get; set; }
    }
}

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
    [JsonSubtypes.KnownSubType(typeof(CreateFileOperation), "create-file")]
    [JsonSubtypes.KnownSubType(typeof(RenameFileOperation), "rename-file")]
    [JsonSubtypes.KnownSubType(typeof(DeleteFileOperation), "delete-file")]
    [JsonSubtypes.KnownSubType(typeof(EditFileOperation), "edit-file")]
    public abstract class WorkspaceEditOperation
    {
        public string Type { get; set; }
    }

    public class CreateFileOperation : WorkspaceEditOperation
    {
        public string Uri { get; set; }

        public WriteFileOptions Options { get; set; }

        public string TextContents { get; set; }

        public WorkspaceEditEntryMetadata Metadata { get; set; }
    }

    public class RenameFileOperation : WorkspaceEditOperation
    {
        public string OldUri { get; set; }

        public string NewUri { get; set; }

        public WriteFileOptions Options { get; set; }

        public WorkspaceEditEntryMetadata Metadata { get; set; }
    }

    public class DeleteFileOperation : WorkspaceEditOperation
    {
        public string Uri { get; set; }

        public DeleteFileOptions DeleteOptions { get; set; }

        public WorkspaceEditEntryMetadata Metadata { get; set; }
    }

    public class EditFileOperation : WorkspaceEditOperation
    {
        public string Uri { get; set; }

        public TextEdit[] Edits { get; set; }
    }
}

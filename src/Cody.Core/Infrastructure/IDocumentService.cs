using Cody.Core.Agent.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Infrastructure
{
    public interface IDocumentService
    {
        bool ShowDocument(string path, Range selection);

        bool SelectInDocument(string path, Range selection);

        bool InsertTextInDocument(string path, Position position, string text);

        bool ReplaceTextInDocument(string path, Range range, string text);

        bool DeleteTextInDocument(string path, Range range);

        bool EditTextInDocument(string path, IEnumerable<TextEdit> edits);

        Task<bool> EditCompletion { get; }

        bool CreateDocument(string path, string content, bool overwrite);
        bool RenameDocument(string oldName, string newName);
        bool DeleteDocument(string path);
    }
}

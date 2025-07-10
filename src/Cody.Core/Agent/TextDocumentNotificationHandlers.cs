using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class TextDocumentNotificationHandlers
    {
        private readonly IDocumentService documentService;

        public TextDocumentNotificationHandlers(IDocumentService documentService)
        {
            this.documentService = documentService;
        }

        [AgentCallback("textDocument/edit", deserializeToSingleObject: true)]
        public bool Edit(TextDocumentEditParams textDocumentEdit)
        {
            var path = textDocumentEdit.Uri.ToWindowsPath();
            foreach (var edit in textDocumentEdit.Edits)
            {
                bool result = false;
                if (edit is InsertTextEdit insert)
                    result = documentService.InsertTextInDocument(path, insert.Position, insert.Value);
                else if (edit is ReplaceTextEdit replace)
                    result = documentService.ReplaceTextInDocument(path, replace.Range, replace.Value);
                else if (edit is DeleteTextEdit delete)
                    result = documentService.DeleteTextInDocument(path, delete.Range);

                if (!result) return false;
            }

            return true;
        }

        [AgentCallback("textDocument/show", deserializeToSingleObject: true)]
        public Task<bool> ShowTextDocument(TextDocumentShowParams textDocumentShow)
        {
            var result = documentService.ShowDocument(textDocumentShow.Uri.ToWindowsPath(), textDocumentShow.Options?.Selection);
            return Task.FromResult(result);
        }
    }
}

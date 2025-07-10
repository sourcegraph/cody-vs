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
            return false;
        }

        [AgentCallback("textDocument/show", deserializeToSingleObject: true)]
        public Task<bool> ShowTextDocument(TextDocumentShowParams textDocumentShow)
        {
            var result = documentService.ShowDocument(textDocumentShow.Uri.ToWindowsPath(), textDocumentShow.Options?.Selection);
            return Task.FromResult(result);
        }
    }
}

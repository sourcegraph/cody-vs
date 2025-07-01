using Cody.Core.Agent.Protocol;
using Cody.Core.Infrastructure;
using Cody.Core.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class TextDocumentNotificationHandlers
    {
        private IFileService fileService;

        public TextDocumentNotificationHandlers(IFileService fileService)
        {
            this.fileService = fileService;
        }

        [AgentCallback("textDocument/edit", deserializeToSingleObject: true)]
        public bool Edit(TextDocumentEditParams textDocumentEdit)
        {
            return false;
        }

        [AgentCallback("textDocument/show", deserializeToSingleObject: true)]
        public Task<bool> ShowTextDocument(TextDocumentShowParams textDocumentShow)
        {
            var path = new Uri(textDocumentShow.Uri).ToString();
            return Task.FromResult(fileService.OpenFileInEditor(path));
        }
    }
}

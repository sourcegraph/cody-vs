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
        private readonly IFileDialogService fileDialogService;
        private readonly IStatusbarService statusbarService;

        public TextDocumentNotificationHandlers(IDocumentService documentService,
            IFileDialogService fileDialogService, IStatusbarService statusbarService)
        {
            this.documentService = documentService;
            this.fileDialogService = fileDialogService;
            this.statusbarService = statusbarService;
        }

        [AgentCallback("window/showSaveDialog", deserializeToSingleObject: true)]
        public string ShowSaveDialog(SaveDialogOptionsParams paramValues)
        {
            var filePath = fileDialogService.ShowSaveFileDialog(
                paramValues.DefaultUri?.ToWindowsPath(), paramValues.Title, paramValues.Filters);

            var uri = filePath?.ToUri();

            return uri;
        }

        [AgentCallback("textDocument/edit", deserializeToSingleObject: true)]
        public bool Edit(TextDocumentEditParams textDocumentEdit)
        {
            try
            {
                var path = textDocumentEdit.Uri.ToWindowsPath();
                return documentService.EditTextInDocument(path, textDocumentEdit.Edits);
            }
            finally
            {
                statusbarService.StopProgressAnimation();
            }
        }

        [AgentCallback("textDocument/show", deserializeToSingleObject: true)]
        public bool ShowTextDocument(TextDocumentShowParams textDocumentShow)
        {
            var result = documentService.ShowDocument(textDocumentShow.Uri.ToWindowsPath(), textDocumentShow.Options?.Selection);
            return result;
        }

        [AgentCallback("workspace/edit", deserializeToSingleObject: true)]
        public bool WorkspaceEdit(WorkspaceEditParams workspaceEdit)
        {
            try
            {
                foreach (var operation in workspaceEdit.Operations)
                {
                    bool result = false;
                    switch (operation)
                    {
                        case CreateFileOperation createFile:
                            result = documentService.CreateDocument(createFile.Uri.ToWindowsPath(), createFile.TextContents, createFile.Options?.Overwrite ?? false);
                            break;
                        case RenameFileOperation renameFile:
                            result = documentService.RenameDocument(renameFile.OldUri.ToWindowsPath(), renameFile.NewUri.ToWindowsPath());
                            break;
                        case DeleteFileOperation deleteFile:
                            result = documentService.DeleteDocument(deleteFile.Uri.ToWindowsPath());
                            break;
                        case EditFileOperation editFile:
                            result = documentService.EditTextInDocument(editFile.Uri.ToWindowsPath(), editFile.Edits);
                            break;
                    }

                    if (!result) return false;
                }

                return true;
            }
            finally
            {
                statusbarService.StopProgressAnimation();
            }
        }
    }
}

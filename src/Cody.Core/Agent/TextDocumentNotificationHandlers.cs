using System;
using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;

namespace Cody.Core.Agent
{
    public class TextDocumentNotificationHandlers
    {
        private readonly IDocumentService documentService;
        private readonly IFileDialogService fileDialogService;
        private readonly ILog logger;

        public TextDocumentNotificationHandlers(IDocumentService documentService, IFileDialogService fileDialogService, ILog logger)
        {
            this.documentService = documentService;
            this.fileDialogService = fileDialogService;
            this.logger = logger;
        }

        [AgentCallback("window/showSaveDialog", deserializeToSingleObject: true)]
        public string ShowSaveDialog(SaveDialogOptionsParams paramValues)
        {
            try
            {
                var filePath = fileDialogService.ShowSaveFileDialog(
                    paramValues.DefaultUri?.ToWindowsPath(), paramValues.Title, paramValues.Filters);

                var uri = filePath?.ToUri();

                return uri;
            }
            catch (Exception ex)
            {
                logger.Error("Save file dialog failed.", ex);
            }

            return null;
        }

        [AgentCallback("textDocument/edit", deserializeToSingleObject: true)]
        public bool Edit(TextDocumentEditParams textDocumentEdit)
        {
            try
            {
                var path = textDocumentEdit.Uri.ToWindowsPath();
                return documentService.EditTextInDocument(path, textDocumentEdit.Edits);
            }
            catch (Exception ex)
            {
                logger.Error($"Document edit failed for '{textDocumentEdit.Uri}'", ex);
            }

            return false;
        }

        [AgentCallback("textDocument/show", deserializeToSingleObject: true)]
        public bool ShowTextDocument(TextDocumentShowParams textDocumentShow)
        {
            try
            {
                var result = documentService.ShowDocument(textDocumentShow.Uri.ToWindowsPath(), textDocumentShow.Options?.Selection);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Document show failed for '{textDocumentShow.Uri}'", ex);
            }

            return false;
        }

        [AgentCallback("workspace/edit", deserializeToSingleObject: true)]
        public bool WorkspaceEdit(WorkspaceEditParams workspaceEdit)
        {
            if (workspaceEdit?.Operations == null)
            {
                logger.Error("Workspace edit failed - operations are NULL.");
                return false;
            }

            foreach (var operation in workspaceEdit.Operations)
            {
                bool result;
                try
                {
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
                        default:
                            throw new NotSupportedException($"Not supported operation: {operation}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Workspace operation ('{operation.Type}') failed for '{GetOperationUri(operation)}'", ex);
                    return false;
                }

                if (!result) return false;
            }

            return true;
        }

        private string GetOperationUri(WorkspaceEditOperation operation)
        {
            switch (operation)
            {
                case CreateFileOperation createFile:
                    return createFile.Uri;
                case RenameFileOperation renameFile:
                    return renameFile.OldUri;
                case DeleteFileOperation deleteFile:
                    return deleteFile.Uri;
                case EditFileOperation editFile:
                    return editFile.Uri;
                default:
                    return "unknown";
            }
        }
    }
}

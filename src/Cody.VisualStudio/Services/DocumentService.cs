using System;
using Cody.Core.Ide;
using Cody.Core.Logging;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Cody.VisualStudio.Services
{
    public class DocumentService: IDocumentService
    {
        private readonly ILog _logger;

        public DocumentService(ILog logger)
        {
            _logger = logger;
        }

        public bool InsertAtCursor(string text)
        {
            Document activeDocument = null;
            try
            {

                var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                activeDocument = dte?.ActiveDocument;
                if (activeDocument?.Selection is TextSelection selection)
                {
                    _logger.Debug($"Active document: '{activeDocument.FullName}'");
                    selection.Insert(text);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Explicit text insert failed in file '{activeDocument?.FullName}'", ex);
            }


            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.DocumentSync
{
    public class DocumentSyncCallback : IDocumentSyncActions
    {
        public void OnChanged(string fullPath, DocumentRange selection, IEnumerable<DocumentChange> changes)
        {
            throw new NotImplementedException();
        }

        public void OnClosed(string fullPath)
        {
            throw new NotImplementedException();
        }

        public void OnFocus(string fullPath)
        {
            throw new NotImplementedException();
        }

        public void OnOpened(string fullPath, string content, DocumentRange selection)
        {
            throw new NotImplementedException();
        }

        public void OnSaved(string fullPath)
        {
            throw new NotImplementedException();
        }
    }
}

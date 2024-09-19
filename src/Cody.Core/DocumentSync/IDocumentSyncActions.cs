using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.DocumentSync
{
    public interface IDocumentSyncActions
    {
        void OnOpened(string fullPath, string content, DocumentRange visibleRange, DocumentRange selection);

        void OnFocus(string fullPath);

        void OnChanged(string fullPath, DocumentRange visibleRange, DocumentRange selection, IEnumerable<DocumentChange> changes);

        void OnSaved(string fullPath);

        void OnClosed(string fullPath);

    }

    public class DocumentPosition
    {
        public int Line { get; set; }

        public int Column { get; set; }

        public override string ToString() => $"({Line},{Column})";
    }

    public class DocumentRange
    {
        public DocumentPosition Start { get; set; }

        public DocumentPosition End { get; set; }

        public override string ToString() => $"{Start}-{End}";
    }

    public class DocumentChange
    {
        public string Text { get; set; }
        public DocumentRange Range { get; set; }

        public override string ToString() => $"['{Text}':{Range}]";
    }
}

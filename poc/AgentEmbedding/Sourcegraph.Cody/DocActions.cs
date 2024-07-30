using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sourcegraph.Cody
{
    public class DocActions : IDocumentActions
    {
        public void OnChanged(string fullPath, DocumentRange selection, IEnumerable<DocumentChange> changes)
        {
            System.Diagnostics.Debug.WriteLine("Change " + fullPath, "[[Cody]]");
        }

        public void OnClosed(string fullPath)
        {
            System.Diagnostics.Debug.WriteLine("Closed " + fullPath, "[[Cody]]");
        }

        public void OnFocus(string fullPath)
        {
            System.Diagnostics.Debug.WriteLine("Focus " + fullPath, "[[Cody]]");
        }

        public void OnOpened(string fullPath, string content, DocumentRange selection)
        {
            System.Diagnostics.Debug.WriteLine("Opened " + fullPath, "[[Cody]]");
        }

        public void OnSaved(string fullPath)
        {
            System.Diagnostics.Debug.WriteLine("Saved " + fullPath, "[[Cody]]");
        }
    }
}

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Cody.Core.Logging;

namespace Cody.VisualStudio.Services
{
    public class DocumentsSyncManager
    {
        private readonly IVsRunningDocumentTable runningDocumentTable;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService;
        private readonly ILog log;

        private uint lastDocumentShow = 0;
        private uint runningDocumentTableCookie;
        private HashSet<uint> openDocuments = new HashSet<uint>();
        private IVsTextView activeTextView;
        private ITextBuffer activeTextBuffer;
        private uint activeDocId;

        public DocumentsSyncManager(IVsRunningDocumentTable runningDocumentTable, IVsEditorAdaptersFactoryService editorAdaptersFactoryService, ILog log)
        {
            this.runningDocumentTable = runningDocumentTable;
            this.editorAdaptersFactoryService = editorAdaptersFactoryService;
            this.log = log;
        }

        public void Initialize()
        {
            var handlers = new RunningDocTableHandlers(OnAfterSave, OnShow, OnHide);
            runningDocumentTable.AdviseRunningDocTableEvents(handlers, out runningDocumentTableCookie);

            runningDocumentTable.GetRunningDocumentsEnum(out IEnumRunningDocuments enumDocuments);
            var docArray = new uint[100];
            enumDocuments.Reset();
            var docList = new List<uint>();
            bool more = false;
            do
            {
                more = enumDocuments.Next((uint)docArray.Length, docArray, out uint fetched) == 0;
                for (int i = 0; i < fetched; i++)
                {
                    if (!IsSolutionOrProjectDocument(docArray[i]))
                    {
                        openDocuments.Add(docArray[i]);
                        //sent didopen

                        Debug.WriteLine($"[[Cody]] didOpen() {GetDocumentFullPath(docArray[i])}");
                    }
                }
            }
            while (more);
        }

        private bool IsSolutionOrProjectDocument(uint docId)
        {
            runningDocumentTable.GetDocumentInfo(docId, out _, out _, out _, out _, out _, out uint itemId, out _);
            return itemId == (uint)VSConstants.VSITEMID.Root;
        }

        private string GetDocumentFullPath(uint docId)
        {
            runningDocumentTable.GetDocumentInfo(docId, out _, out _, out _, out string documentFullPath, out _, out _, out _);
            return documentFullPath;
        }

        private ITextBuffer GetTextBuffer(IVsTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var wpfTextView = editorAdaptersFactoryService.GetWpfTextView(textView);
            return wpfTextView.TextBuffer;
        }

        private void OnAfterSave(uint docId)
        {
            Debug.WriteLine($"[[Cody]] didSave() {GetDocumentFullPath(docId)}");
        }

        private void TrySetActiveDocument(uint docId, IVsWindowFrame pFrame)
        {
            var textView = VsShellUtilities.GetTextView(pFrame);
            if (textView != null)
            {
                if (activeTextBuffer != null)
                {
                    activeTextBuffer.ChangedLowPriority -= OnTextBufferChanged;
                    activeTextView = null;
                }

                var textBuffer = GetTextBuffer(textView);

                activeTextBuffer = textBuffer;
                activeTextView = textView;
                activeDocId = docId;

                textBuffer.ChangedLowPriority += OnTextBufferChanged;
            }
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) => OnTextChange(activeTextView, (ITextBuffer)sender, e.Changes);

        private void OnShow(uint docId, IVsWindowFrame pFrame)
        {
            if (docId == lastDocumentShow) return;

            var docFullPath = GetDocumentFullPath(docId);
            if (openDocuments.Contains(docId))
            {
                Debug.WriteLine($"[[Cody]] didFocus() {docFullPath}");
                //sent didFocus
            }
            else
            {
                openDocuments.Add(docId);
                Debug.WriteLine($"[[Cody]] didOpen() {docFullPath}");
                //sent didOpen
            }

            TrySetActiveDocument(docId, pFrame);

            lastDocumentShow = docId;
        }

        private void OnHide(uint docId, IVsWindowFrame pFrame)
        {
            if (openDocuments.Contains(docId))
            {
                openDocuments.Remove(docId);
                Debug.WriteLine($"[[Cody]] didClose() {GetDocumentFullPath(docId)}");
                //sent didclose
            }
        }

        private void OnTextChange(IVsTextView textView, ITextBuffer textBuffer, INormalizedTextChangeCollection textChanges)
        {
            Debug.WriteLine($"[[Cody]] didChange() {GetDocumentFullPath(activeDocId)}");
            //send didChange
        }
    }
}

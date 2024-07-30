using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.VisualStudio.Threading.SingleThreadedSynchronizationContext;

namespace Sourcegraph.Cody
{
    public class DocumentsManager : IVsRunningDocTableEvents
    {
        private RunningDocumentTable rdt;

        private readonly IVsUIShell vsUIShell;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService;
        private readonly IDocumentActions documentActions;


        private uint lastShowdoc = 0;

        public DocumentsManager(IVsUIShell vsUIShell, IDocumentActions documentActions, IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            this.rdt = new RunningDocumentTable();
            this.vsUIShell = vsUIShell;
            this.documentActions = documentActions;
            
            this.editorAdaptersFactoryService = editorAdaptersFactoryService;
        }

        public void Initialize()
        {
            foreach (var frame in GetOpenDocuments())
            {
                frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out object cookie);
                var docCookie = (uint)(int)cookie;
                var path = rdt.GetDocumentInfo(docCookie).Moniker;
                var content = rdt.GetRunningDocumentContents(docCookie);
                var textView = VsShellUtilities.GetTextView(frame);
                var docRange = GetDocumentSelection(textView);

                documentActions.OnOpened(path, content, docRange);
            }

            rdt.Advise(this);
        }

        private IEnumerable<IVsWindowFrame> GetOpenDocuments()
        {
            var results = new List<IVsWindowFrame>();

            vsUIShell.GetDocumentWindowEnum(out IEnumWindowFrames docEnum);
            var winFrameArray = new IVsWindowFrame[50];

            while (true)
            {
                docEnum.Next((uint)winFrameArray.Length, winFrameArray, out uint fetched);
                if (fetched == 0) break;

                results.AddRange(winFrameArray.Take((int)fetched));
            }

            return results;
        }

        private IVsWindowFrame GetWindowFrame(uint docCookie)
        {
            foreach (var winFrame in GetOpenDocuments())
            {
                winFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out object cookie);
                if (docCookie == (uint)(int)cookie) return winFrame;
            }

            return null;
        }

        private ITextBuffer GetTextBuffer(IVsTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var wpfTextView = editorAdaptersFactoryService.GetWpfTextView(textView);
            return wpfTextView.TextBuffer;
        }

        private DocumentRange GetDocumentSelection(IVsTextView textView)
        {
            int startLine = 0, startCol = 0, endLine = 0, endCol = 0;
            if(textView != null) textView.GetSelection(out startLine, out startCol, out endLine, out endCol);
            return new DocumentRange
            {
                Start = new DocumentPosition
                {
                    Line = startLine,
                    Column = startCol,
                },
                End = new DocumentPosition
                {
                    Line = endLine,
                    Column = endCol,
                }
            };
        }

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            if (dwReadLocksRemaining == 0 && dwEditLocksRemaining == 0)
            {
                var path = rdt.GetDocumentInfo(docCookie).Moniker;
                documentActions.OnClosed(path);
            }
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            var path = rdt.GetDocumentInfo(docCookie).Moniker;
            documentActions.OnSaved(path);
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        private IVsTextView activeTextView;
        private ITextBuffer activeTextBuffer;
        private uint activeDocCookie = 0;

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if (lastShowdoc != docCookie)
            {
                var path = rdt.GetDocumentInfo(docCookie).Moniker;

                if (fFirstShow == 1)
                {
                    var content = rdt.GetRunningDocumentContents(docCookie);
                    var textView = VsShellUtilities.GetTextView(pFrame);
                    var docRange = GetDocumentSelection(textView);

                    documentActions.OnOpened(path, content, docRange);
                }

                
                documentActions.OnFocus(path);

                activeTextView = VsShellUtilities.GetTextView(pFrame);
                if (activeTextView != null)
                {
                    activeTextBuffer = GetTextBuffer(activeTextView);
                    activeTextBuffer.ChangedLowPriority += OnTextBufferChanged;
                }
                else activeTextBuffer = null;

                activeDocCookie = docCookie;
                lastShowdoc = docCookie;
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            if(activeTextBuffer != null) activeTextBuffer.ChangedLowPriority -= OnTextBufferChanged;
            activeTextView = null;
            activeTextBuffer = null;
            activeDocCookie = 0;

            return VSConstants.S_OK;
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            var path = rdt.GetDocumentInfo(activeDocCookie).Moniker;
            var selection = GetDocumentSelection(activeTextView);
            var changes = GetContentChanges(e.Changes, activeTextView);

            documentActions.OnChanged(path, selection, changes);
        }

        private IEnumerable<DocumentChange> GetContentChanges(INormalizedTextChangeCollection textChanges, IVsTextView textView)
        {
            var results = new List<DocumentChange>();

            foreach (var change in textChanges)
            {
                textView.GetLineAndColumn(change.NewPosition, out int startLine, out int startCol);
                textView.GetLineAndColumn(change.NewEnd, out int endLine, out int endCol);

                var contentChange = new DocumentChange
                {
                    Text = change.NewText,
                    Range = new DocumentRange
                    {
                        Start = new DocumentPosition
                        {
                            Line = startLine,
                            Column = startCol
                        },
                        End = new DocumentPosition
                        {
                            Line = endLine,
                            Column = endCol
                        }
                    }
                };

                results.Add(contentChange);
            }

            return results;
        }
    }
}

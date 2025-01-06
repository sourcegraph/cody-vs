using Cody.Core.DocumentSync;
using Cody.Core.Logging;
using Cody.Core.Trace;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cody.VisualStudio.Services
{
    public class DocumentsSyncService : IVsRunningDocTableEvents
    {
        private static readonly TraceLogger trace = new TraceLogger(nameof(DocumentsSyncService));

        private RunningDocumentTable rdt;
        private uint rdtCookie = 0;

        private readonly IVsUIShell vsUIShell;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService;
        private readonly IDocumentSyncActions documentActions;
        private readonly ILog log;

        private ActiveDocument activeDocument;
        private uint lastShowDocCookie = 0;

        public DocumentsSyncService(IVsUIShell vsUIShell, IDocumentSyncActions documentActions, IVsEditorAdaptersFactoryService editorAdaptersFactoryService, ILog log)
        {
            this.rdt = new RunningDocumentTable();
            this.vsUIShell = vsUIShell;
            this.documentActions = documentActions;
            this.editorAdaptersFactoryService = editorAdaptersFactoryService;
            this.log = log;
        }

        public void Initialize()
        {
            try
            {
                uint activeCookie = 0;
                IVsWindowFrame activeFrame = null;
                foreach (var frame in GetOpenDocuments())
                {
                    try
                    {
                        frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out object cookie);
                        var docCookie = (uint)(int)cookie;
                        var path = rdt.GetDocumentInfo(docCookie).Moniker;
                        frame.IsOnScreen(out int onScreen);
                        if (onScreen == 1)
                        {
                            activeCookie = docCookie;
                            activeFrame = frame;
                        }
                        var content = rdt.GetRunningDocumentContents(docCookie);
                        var textView = GetTextView(frame);
                        var visibleRange = GetVisibleRange(textView);
                        var docRange = GetDocumentSelection(textView);

                        documentActions.OnOpened(path, content, visibleRange, docRange);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Can't initialize document sync", ex);
                    }
                }

                if (activeCookie != 0)
                    ((IVsRunningDocTableEvents)this).OnBeforeDocumentWindowShow(activeCookie, 0, activeFrame);
            }
            catch (Exception ex)
            {
                log.Error("Document sync initialization error", ex);
            }
            finally
            {
                rdtCookie = rdt.Advise(this);
            }
        }

        private IVsTextView GetTextView(IVsWindowFrame windowFrame)
        {
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar);
            IVsTextView ppView = pvar as IVsTextView;
            if (ppView == null && pvar is IVsCodeWindow vsCodeWindow)
            {
                try
                {
                    if (vsCodeWindow.GetPrimaryView(out ppView) != 0)
                        vsCodeWindow.GetSecondaryView(out ppView);
                }
                catch
                {
                    return null;
                }
            }

            return ppView;
        }

        public void Deinitialize()
        {
            if (rdtCookie != 0)
            {
                rdt.Unadvise(rdtCookie);
                rdtCookie = 0;
            }
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

        private DocumentRange GetVisibleRange(IVsTextView textView, ITextSnapshot snapshot = null)
        {
            DocumentPosition firstVisiblePosition = null, lastVisiblePosition = null;

            if (textView != null && ThreadHelper.CheckAccess())
            {
                var wpfTextView = editorAdaptersFactoryService.GetWpfTextView(textView);
                snapshot = snapshot ?? wpfTextView.TextSnapshot;
                var lines = wpfTextView.TextViewLines;

                firstVisiblePosition = ToDocumentPosition(snapshot, lines.FirstVisibleLine.Start.Position);
                lastVisiblePosition = ToDocumentPosition(snapshot, lines.LastVisibleLine.End.Position);
            }
            else return null;

            var range = new DocumentRange
            {
                Start = firstVisiblePosition,
                End = lastVisiblePosition
            };

            return range;
        }

        private DocumentRange GetDocumentSelection(IVsTextView textView, ITextSnapshot snapshot = null)
        {
            bool swap = false;
            DocumentPosition start = null, end = null;
            if (textView != null && ThreadHelper.CheckAccess())
            {
                var wpfTextView = editorAdaptersFactoryService.GetWpfTextView(textView);
                if (wpfTextView != null)
                {
                    snapshot = snapshot ?? wpfTextView.TextSnapshot;
                    var selection = wpfTextView.Selection;

                    start = ToDocumentPosition(snapshot, selection.StreamSelectionSpan.Start.Position);
                    end = ToDocumentPosition(snapshot, selection.StreamSelectionSpan.End.Position);
                }
                else return null;
            }
            else return null;

            if(start.Line > end.Line || (start.Line == end.Line && start.Column > end.Column)) swap = true;

            return new DocumentRange
            {
                Start = swap ? end : start,
                End = swap ? start : end
            };
        }

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            trace.TraceEvent("OnBeforeLastDocumentUnlock");
            if (dwReadLocksRemaining == 0 && dwEditLocksRemaining == 0)
            {
                var path = rdt.GetDocumentInfo(docCookie).Moniker;
                trace.TraceEvent("OnClosed", path);
                documentActions.OnClosed(path);
            }
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            var path = rdt.GetDocumentInfo(docCookie).Moniker;
            trace.TraceEvent("OnAfterSave", path);
            documentActions.OnSaved(path);
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            trace.TraceEvent("OnBeforeDocumentWindowShow");
            if (lastShowDocCookie != docCookie)
            {
                var path = rdt.GetDocumentInfo(docCookie).Moniker;
                trace.TraceEvent("OnDocumentShow", path);
                var textView = GetTextView(pFrame);

                if (fFirstShow == 1)
                {
                    trace.TraceEvent("OnFirstShowDocument", path);
                    var content = rdt.GetRunningDocumentContents(docCookie);

                    var docRange = GetDocumentSelection(textView);
                    var visibleRange = GetVisibleRange(textView);

                    documentActions.OnOpened(path, content, visibleRange, docRange);
                }

                documentActions.OnFocus(path);

                if (textView != null)
                {
                    if (activeDocument != null)
                    {
                        activeDocument.TextBuffer.ChangedLowPriority -= OnTextBufferChanged;
                        activeDocument.Selection.SelectionChanged -= OnSelectionChanged;
                    }

                    activeDocument = new ActiveDocument
                    {
                        DocCookie = docCookie,
                        TextView = textView,
                        WpfTextView = editorAdaptersFactoryService.GetWpfTextView(textView)
                    };

                    activeDocument.TextBuffer.ChangedLowPriority += OnTextBufferChanged;
                    activeDocument.Selection.SelectionChanged += OnSelectionChanged;
                }
                else activeDocument = null;

                lastShowDocCookie = docCookie;
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            trace.TraceEvent("OnAfterDocumentWindowHide");
            if (activeDocument != null)
            {
                activeDocument.TextBuffer.ChangedLowPriority -= OnTextBufferChanged;
                activeDocument.Selection.SelectionChanged -= OnSelectionChanged;
                activeDocument = null;
            }

            return VSConstants.S_OK;
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            trace.TraceEvent("OnTextChanged");

            try
            {
                var path = rdt.GetDocumentInfo(activeDocument.DocCookie).Moniker;
                var selection = GetDocumentSelection(activeDocument.TextView, e.Before);
                var changes = GetContentChanges(e.Changes, e.Before);
                var visibleRange = GetVisibleRange(activeDocument.TextView, e.Before);

                trace.TraceEvent("OnChanged", "OnTextBufferChanged");
                documentActions.OnChanged(path, visibleRange, selection, changes);
            }
            catch (Exception ex)
            {
                log.Error("Failed.", ex);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            trace.TraceEvent("OnSelectionChanged");

            try
            {
                var path = rdt.GetDocumentInfo(activeDocument.DocCookie).Moniker;
                var selection = GetDocumentSelection(activeDocument.TextView);
                var visibleRange = GetVisibleRange(activeDocument.TextView);

                trace.TraceEvent("OnChanged", "OnSelectionChanged");
                documentActions.OnChanged(path, visibleRange, selection, Enumerable.Empty<DocumentChange>());
            }
            catch (Exception ex)
            {
                log.Error("Failed.", ex);
            }
        }

        private IEnumerable<DocumentChange> GetContentChanges(INormalizedTextChangeCollection textChanges, ITextSnapshot beforeSnapshot)
        {
            var results = new List<DocumentChange>();
            
            foreach (var change in textChanges)
            {
                var start = ToDocumentPosition(beforeSnapshot, change.OldPosition);
                var end = ToDocumentPosition(beforeSnapshot,change.OldEnd);

                var contentChange = new DocumentChange
                {
                    Text = change.NewText,
                    Range = new DocumentRange
                    {
                        Start = new DocumentPosition
                        {
                            Line = start.Line,
                            Column = start.Column
                        },
                        End = new DocumentPosition
                        {
                            Line = end.Line,
                            Column = end.Column
                        }
                    }
                };

                results.Add(contentChange);
            }

            return results;
        }

        private DocumentPosition ToDocumentPosition(ITextSnapshot snapshot, int position)
        {
            var line = snapshot.GetLineFromPosition(position);
            int col = position - line.Start.Position;
            return new DocumentPosition { Line = line.LineNumber, Column = col };
        }

        public class ActiveDocument
        {

            public IVsTextView TextView { get; set; }

            public uint DocCookie { get; set; }

            public IWpfTextView WpfTextView { get; set; }

            public ITextBuffer TextBuffer => WpfTextView.TextBuffer;

            public ITextSelection Selection => WpfTextView.Selection;

        }
    }


}

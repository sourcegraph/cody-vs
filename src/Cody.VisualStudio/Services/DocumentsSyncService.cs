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
using System.Diagnostics;
using System.Linq;

namespace Cody.VisualStudio.Services
{
    public class DocumentsSyncService : IVsRunningDocTableEvents2
    {
        private static readonly TraceLogger trace = new TraceLogger(nameof(DocumentsSyncService));

        private RunningDocumentTable rdt;
        private uint rdtCookie = 0;

        private readonly IVsUIShell vsUIShell;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService;
        private readonly IDocumentSyncActions documentActions;
        private readonly ILog log;

        private uint lastShowDocCookie = 0;
        private HashSet<uint> openNotificationSend = new HashSet<uint>();
        private HashSet<uint> isSubscribed = new HashSet<uint>();

        public DocumentsSyncService(IVsUIShell vsUIShell,
            IDocumentSyncActions documentActions,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService,
            ILog log)
        {
            this.rdt = new RunningDocumentTable();
            this.vsUIShell = vsUIShell;
            this.documentActions = documentActions;
            this.editorAdaptersFactoryService = editorAdaptersFactoryService;
            this.log = log;
        }

        public void Initialize()
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    uint activeCookie = 0;
                    AssertThatNoOpenedDocuments();
                    foreach (var frame in GetOpenDocuments())
                    {
                        if (frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out object cookie) != VSConstants.S_OK) continue;
                        var docCookie = (uint)(int)cookie;
                        var path = rdt.GetDocumentInfo(docCookie).Moniker;
                        if (path == null) continue;
                        var content = rdt.GetRunningDocumentContents(docCookie);
                        if (content == null) continue; //document that does not contain any text ex. jpeg

                        documentActions.OnOpened(path, content, null, null);
                        openNotificationSend.Add(docCookie);

                        if (frame.IsOnScreen(out int onScreen) == VSConstants.S_OK && onScreen == 1) activeCookie = docCookie;
                    }

                    if (activeCookie != 0)
                    {
                        var path = rdt.GetDocumentInfo(activeCookie).Moniker;
                        if (path != null)
                        {
                            trace.TraceEvent("OnInitFocus");
                            documentActions.OnFocus(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Initialization failed.", ex);
                }
                finally
                {
                    rdtCookie = rdt.Advise(this);
                }
            });
        }

        private void AssertThatNoOpenedDocuments()
        {
            Debug.Assert(openNotificationSend.Count == 0, $"{nameof(openNotificationSend)} is {openNotificationSend}, but it should be ZERO!");
            Debug.Assert(isSubscribed.Count == 0, $"{nameof(isSubscribed)} is {isSubscribed}, but it should be ZERO!");
        }

        private IVsTextView GetVsTextView(IVsWindowFrame windowFrame)
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
                isSubscribed.Clear();
                openNotificationSend.Clear();
            }
        }

        private IEnumerable<IVsWindowFrame> GetOpenDocuments()
        {
            var results = new List<IVsWindowFrame>();
            uint fetched = 0;
            var winFrameArray = new IVsWindowFrame[1];

            if (vsUIShell.GetDocumentWindowEnum(out IEnumWindowFrames docEnum) != VSConstants.S_OK) return results;

            while (docEnum.Next(1, winFrameArray, out fetched) == VSConstants.S_OK && fetched == 1)
            {
                results.Add(winFrameArray[0]);
            }

            return results;
        }

        private DocumentRange GetVisibleRange(ITextView textView)
        {
            DocumentPosition firstVisiblePosition = null, lastVisiblePosition = null;

            if (ThreadHelper.CheckAccess())
            {
                var lines = textView.TextViewLines;
                if (lines != null && lines.IsValid)
                {
                    try
                    {
                        firstVisiblePosition = ToDocumentPosition(lines.FirstVisibleLine.Start);
                        lastVisiblePosition = ToDocumentPosition(lines.LastVisibleLine.End);
                    }
                    catch (ObjectDisposedException)
                    {
                        return null;
                    }
                }
                else return null;
            }
            else return null;

            var range = new DocumentRange
            {
                Start = firstVisiblePosition,
                End = lastVisiblePosition
            };

            return range;
        }

        private DocumentRange GetDocumentSelection(ITextView textView)
        {
            bool swap = false;
            DocumentPosition start = null, end = null;
            if (ThreadHelper.CheckAccess())
            {
                var selection = textView.Selection.StreamSelectionSpan;

                start = ToDocumentPosition(selection.Start.Position);
                end = ToDocumentPosition(selection.End.Position);
            }
            else return null;

            if (start.Line > end.Line || (start.Line == end.Line && start.Column > end.Column)) swap = true;

            return new DocumentRange
            {
                Start = swap ? end : start,
                End = swap ? start : end
            };
        }

        public string GetFilePath(ITextView textView)
        {
            textView.TextBuffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out IVsTextBuffer bufferAdapter);
            var persistFileFormat = bufferAdapter as IPersistFileFormat;

            if (persistFileFormat == null) return null;

            persistFileFormat.GetCurFile(out string filePath, out _);
            return filePath;
        }

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;
        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            trace.TraceEvent("OnBeforeLastDocumentUnlock");
            if (dwReadLocksRemaining == 0 && dwEditLocksRemaining == 0 && openNotificationSend.Contains(docCookie))
            {
                var path = rdt.GetDocumentInfo(docCookie).Moniker;
                if (path == null) return VSConstants.S_OK;
                trace.TraceEvent("OnClosed", path);
                documentActions.OnClosed(path);

                openNotificationSend.Remove(docCookie);
                isSubscribed.Remove(docCookie);
            }
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            if (openNotificationSend.Contains(docCookie))
            {
                var path = rdt.GetDocumentInfo(docCookie).Moniker;
                if (path == null) return VSConstants.S_OK;
                trace.TraceEvent("OnAfterSave", path);
                documentActions.OnSaved(path);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            trace.TraceEvent("OnBeforeDocumentWindowShow");
            if (lastShowDocCookie != docCookie)
            {
                var path = rdt.GetDocumentInfo(docCookie).Moniker;
                if (path == null) return VSConstants.S_OK;

                if (!isSubscribed.Contains(docCookie))
                {
                    trace.TraceEvent("OnSubscribeDocument", path);

                    var content = rdt.GetRunningDocumentContents(docCookie);
                    if (content == null) return VSConstants.S_OK;

                    var textView = GetVsTextView(pFrame);
                    if (textView != null)
                    {
                        var wpfTextView = editorAdaptersFactoryService.GetWpfTextView(textView);
                        if (wpfTextView != null)
                        {
                            Subscribe(wpfTextView);

                            if (!openNotificationSend.Contains(docCookie))
                            {

                                var docRange = GetDocumentSelection(wpfTextView);
                                var visibleRange = GetVisibleRange(wpfTextView);

                                documentActions.OnOpened(path, content, visibleRange, docRange);
                                openNotificationSend.Add(docCookie);
                            }

                            isSubscribed.Add(docCookie);
                        }
                    }
                }

                trace.TraceEvent("OnDocumentFocus", path);
                documentActions.OnFocus(path);

                lastShowDocCookie = docCookie;
            }

            return VSConstants.S_OK;
        }

        private void Subscribe(IWpfTextView textView)
        {
            textView.TextBuffer.Properties.AddProperty(typeof(IWpfTextView), textView);
            textView.Selection.SelectionChanged += OnSelectionChanged;
            textView.TextBuffer.ChangedLowPriority += OnTextBufferChanged;
            textView.Closed += UnsubscribeOnClosed;
        }

        private void UnsubscribeOnClosed(object sender, EventArgs e)
        {
            var textView = (IWpfTextView)sender;

            textView.TextBuffer.Properties.RemoveProperty(typeof(IWpfTextView));
            textView.Selection.SelectionChanged -= OnSelectionChanged;
            textView.TextBuffer.ChangedLowPriority -= OnTextBufferChanged;
            textView.Closed -= UnsubscribeOnClosed;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => VSConstants.S_OK;

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            trace.TraceEvent("OnTextChanged");

            try
            {
                var textBuffer = (ITextBuffer)sender;
                var wpfTextView = textBuffer.Properties.GetProperty<IWpfTextView>(typeof(IWpfTextView));

                var path = GetFilePath(wpfTextView);
                if (path == null) return;
                var selection = GetDocumentSelection(wpfTextView);
                var changes = GetContentChanges(e.Changes, e.Before);
                var visibleRange = GetVisibleRange(wpfTextView);

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
                var textView = ((ITextSelection)sender).TextView;

                var path = GetFilePath(textView);
                if (path == null) return;
                var selection = GetDocumentSelection(textView);
                var visibleRange = GetVisibleRange(textView);

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
                var end = ToDocumentPosition(beforeSnapshot, change.OldEnd);

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

        private DocumentPosition ToDocumentPosition(SnapshotPoint position) => ToDocumentPosition(position.Snapshot, position.Position);

        private DocumentPosition ToDocumentPosition(ITextSnapshot snapshot, int position)
        {
            var line = snapshot.GetLineFromPosition(position);
            int col = position - line.Start.Position;
            return new DocumentPosition { Line = line.LineNumber, Column = col };
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnAfterSave(uint docCookie) => VSConstants.S_OK;

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) => VSConstants.S_OK;

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => VSConstants.S_OK;

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            const int DocumentMoved = 6;
            trace.TraceEvent("OnAfterAttributeChangeEx");

            if (grfAttribs == (uint)__VSRDTATTRIB.RDTA_MkDocument || grfAttribs == DocumentMoved)
            {
                trace.TraceEvent("OnRename");
                documentActions.OnRename(pszMkDocumentOld, pszMkDocumentNew);
            }
            return VSConstants.S_OK;
        }
    }


}

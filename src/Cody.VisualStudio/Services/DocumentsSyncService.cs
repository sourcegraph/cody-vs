﻿using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using Cody.Core.DocumentSync;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;

namespace Cody.VisualStudio.Services
{
    public class DocumentsSyncService : IVsRunningDocTableEvents
    {
        private RunningDocumentTable rdt;

        private readonly IVsUIShell vsUIShell;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService;
        private readonly IDocumentSyncActions documentActions;

        private ActiveDocument activeDocument;
        private uint lastShowDocCookie = 0;

        public DocumentsSyncService(IVsUIShell vsUIShell, IDocumentSyncActions documentActions, IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
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
                var visibleRange = GetVisibleRange(textView);

                documentActions.OnOpened(path, content, visibleRange, docRange);
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

        private DocumentRange GetVisibleRange(IVsTextView textView)
        {
            const int SB_VERT = 1;
            int visibleRows = 0, firstVisibleRow = 0;

            if (textView != null) textView.GetScrollInfo(SB_VERT, out _, out _, out visibleRows, out firstVisibleRow);
            else return null;

            var range = new DocumentRange
            {
                Start = new DocumentPosition
                {
                    Line = Math.Max(firstVisibleRow, 0),
                    Column = 0
                },
                End = new DocumentPosition
                {
                    Line = Math.Max(firstVisibleRow + visibleRows, 0),
                    Column = 0
                }
            };

            return range;
        }

        private DocumentRange GetDocumentSelection(IVsTextView textView)
        {
            bool swap = false;
            int startLine = 0, startCol = 0, endLine = 0, endCol = 0;
            if (textView != null) textView.GetSelection(out startLine, out startCol, out endLine, out endCol);
            
            if(startLine > endLine || (startLine == endLine && startCol > endCol)) swap = true;

            return new DocumentRange
            {
                Start = new DocumentPosition
                {
                    Line = swap ? endLine : startLine,
                    Column = swap ? endCol : startCol,
                },
                End = new DocumentPosition
                {
                    Line = swap ? startLine : endLine,
                    Column = swap ? startCol : endCol,
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

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if (lastShowDocCookie != docCookie)
            {
                var path = rdt.GetDocumentInfo(docCookie).Moniker;
                var textView = VsShellUtilities.GetTextView(pFrame);

                if (fFirstShow == 1)
                {
                    var content = rdt.GetRunningDocumentContents(docCookie);
                    
                    var docRange = GetDocumentSelection(textView);
                    var visibleRange = GetVisibleRange(textView);

                    documentActions.OnOpened(path, content, visibleRange, docRange);
                }

                documentActions.OnFocus(path);

                if (textView != null)
                {
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
            var path = rdt.GetDocumentInfo(activeDocument.DocCookie).Moniker;
            var selection = GetDocumentSelection(activeDocument.TextView);
            var changes = GetContentChanges(e.Changes, activeDocument.TextView);
            var visibleRange = GetVisibleRange(activeDocument.TextView);

            documentActions.OnChanged(path, visibleRange, selection, changes);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            var path = rdt.GetDocumentInfo(activeDocument.DocCookie).Moniker;
            var selection = GetDocumentSelection(activeDocument.TextView);
            var visibleRange = GetVisibleRange(activeDocument.TextView);

            documentActions.OnChanged(path, visibleRange, selection, Enumerable.Empty<DocumentChange>());
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

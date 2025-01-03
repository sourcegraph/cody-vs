using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Logging;
using Cody.Core.Trace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cody.Core.DocumentSync
{
    public class DocumentSyncCallback : IDocumentSyncActions
    {
        private static readonly TraceLogger trace = new TraceLogger(nameof(DocumentSyncCallback));

        private ILog logger;
        private IAgentService agentService;

        public DocumentSyncCallback(IAgentService agentService, ILog logger)
        {
            this.agentService = agentService;
            this.logger = logger;
        }

        public void OnChanged(string fullPath, DocumentRange visibleRange, DocumentRange selection, IEnumerable<DocumentChange> changes)
        {
            trace.TraceEvent("DidChange", "ch: '{0}', sel:{1}, vr:{2}, path:{3}", string.Join("", changes), selection, visibleRange, fullPath);

            Range vRange = null;
            if (visibleRange != null)
            {
                vRange = new Range
                {
                    Start = new Position
                    {
                        Line = visibleRange.Start.Line,
                        Character = visibleRange.Start.Column
                    },
                    End = new Position
                    {
                        Line = visibleRange.End.Line,
                        Character = visibleRange.End.Column
                    }
                };
            }

            var docState = new ProtocolTextDocument
            {
                Uri = fullPath.ToUri(),
                VisibleRange = vRange,
                Selection = new Range
                {
                    Start = new Position
                    {
                        Line = selection.Start.Line,
                        Character = selection.Start.Column
                    },
                    End = new Position
                    {
                        Line = selection.End.Line,
                        Character = selection.End.Column
                    }
                },
                ContentChanges = changes.Select(x => new ProtocolTextDocumentContentChangeEvent
                {
                    Text = x.Text.ConvertLineBreaks("\n"),
                    Range = new Range
                    {
                        Start = new Position
                        {
                            Line = x.Range.Start.Line,
                            Character = x.Range.Start.Column
                        },
                        End = new Position
                        {
                            Line = x.Range.End.Line,
                            Character = x.Range.End.Column
                        }
                    }
                }).ToArray()
            };

            agentService.DidChange(docState);
            //var result = agentService.GetWorkspaceDocuments(new GetDocumentsParams { Uris = new[] { fullPath.ToUri() } }).Result;
            //trace.TraceEvent("AfterDidChange", result.Documents.First().Content);
        }

        public void OnClosed(string fullPath)
        {
            trace.TraceEvent("DidClose", "{0}", fullPath);

            var docState = new ProtocolTextDocument
            {
                Uri = fullPath.ToUri(),
            };

            // Only the 'uri' property is required, other properties are ignored.
            agentService.DidClose(docState);
        }

        public void OnFocus(string fullPath)
        {
            trace.TraceEvent("DidFocus", "{0}", fullPath);
            agentService.DidFocus(new CodyFilePath { Uri = fullPath.ToUri() });
        }

        public void OnOpened(string fullPath, string content, DocumentRange visibleRange, DocumentRange selection)
        {
            trace.TraceEvent("DidOpen", "sel:{0}, vr:{1}, path:{2}", selection, visibleRange, fullPath);

            Range vRange = null;
            if (visibleRange != null)
            {
                vRange = new Range
                {
                    Start = new Position
                    {
                        Line = visibleRange.Start.Line,
                        Character = visibleRange.Start.Column
                    },
                    End = new Position
                    {
                        Line = visibleRange.End.Line,
                        Character = visibleRange.End.Column
                    }
                };
            }

            var docState = new ProtocolTextDocument
            {
                Uri = fullPath.ToUri(),
                Content = content.ConvertLineBreaks("\n"),
                VisibleRange = vRange,
                Selection = new Range
                {
                    Start = new Position
                    {
                        Line = selection.Start.Line,
                        Character = selection.Start.Column
                    },
                    End = new Position
                    {
                        Line = selection.End.Line,
                        Character = selection.End.Column
                    }
                }
            };

            agentService.DidOpen(docState);
        }

        public void OnSaved(string fullPath)
        {
            trace.TraceEvent("DidSave", "{0}", fullPath);
            agentService.DidSave(new CodyFilePath { Uri = fullPath.ToUri() });
        }
    }
}

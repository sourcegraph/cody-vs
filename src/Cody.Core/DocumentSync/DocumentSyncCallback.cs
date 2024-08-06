using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cody.Core.DocumentSync
{
    public class DocumentSyncCallback : IDocumentSyncActions
    {
        private ILog logger;
        private IAgentClientFactory agentClientFactory;

        public DocumentSyncCallback(IAgentClientFactory agentClientFactory, ILog logger)
        {
            this.agentClientFactory = agentClientFactory;
            this.logger = logger;
        }

        private string ToUri(string path)
        {
            var uri = new Uri(path).AbsoluteUri;
            return Regex.Replace(uri, "(file:///)(\\D+)(:)", m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + "%3A");
        }

        public void OnChanged(string fullPath, DocumentRange visibleRange, DocumentRange selection, IEnumerable<DocumentChange> changes)
        {
            logger.Debug($"Sending didChange() for '{fullPath}' changes: {string.Join("|", changes.Select(x => x.Text))}");

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
                Uri = ToUri(fullPath),
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
                    Text = x.Text,
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

            agentClientFactory.CreateAgentClient().DidChange(docState);
        }

        public void OnClosed(string fullPath)
        {
            logger.Debug($"Sending DidClose() for '{fullPath}'");

            var docState = new ProtocolTextDocument
            {
                Uri = ToUri(fullPath),
            };

            // Only the 'uri' property is required, other properties are ignored.
            agentClientFactory.CreateAgentClient().DidClose(docState);
        }

        public void OnFocus(string fullPath)
        {
            logger.Debug($"Sending DidFocus() for '{fullPath}'");
            agentClientFactory.CreateAgentClient().DidFocus(ToUri(fullPath));

        }

        public void OnOpened(string fullPath, string content, DocumentRange visibleRange, DocumentRange selection)
        {
            logger.Debug($"Sending DidOpen() for '{fullPath}'");

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
                Uri = ToUri(fullPath),
                Content = content,
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

            agentClientFactory.CreateAgentClient().DidOpen(docState);
        }

        public void OnSaved(string fullPath)
        {
            logger.Debug($"Sending DidSave() for '{fullPath}'");

            agentClientFactory.CreateAgentClient().DidSave(ToUri(fullPath));
        }
    }
}

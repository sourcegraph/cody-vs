using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ILog log;
        private readonly IServiceProvider serviceProvider;
        private readonly IVsSolution vsSolution;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService;

        public DocumentService(ILog log, IServiceProvider serviceProvider, IVsSolution vsSolution, IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            this.log = log;
            this.serviceProvider = serviceProvider;
            this.vsSolution = vsSolution;
            this.editorAdaptersFactoryService = editorAdaptersFactoryService;
        }

        public bool ShowDocument(string path, Range selection)
        {
            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var tryOpenResult = VsShellUtilities.TryOpenDocument(serviceProvider, path, Guid.Empty, out _, out _, out IVsWindowFrame windowFrame);
                if (tryOpenResult == VSConstants.S_OK)
                {
                    windowFrame?.Show();

                    if (selection != null && windowFrame != null)
                    {
                        var textView = GetVsTextView(windowFrame);
                        if (textView != null)
                        {
                            textView.CenterLines(selection.Start.Line, 0);
                            textView.SetSelection(selection.Start.Line, selection.Start.Character, selection.End.Line, selection.End.Character);
                        }
                    }

                    return true;
                }
                else log.Error($"Cannot show document '{path}' (error code: {tryOpenResult})");

                return false;
            });

            return result;
        }

        public bool InsertTextInDocument(string path, Position position, string text)
        {
            var edits = new TextEdit[1] { new InsertTextEdit { Position = position, Value = text } };
            return EditTextInDocument(path, edits);
        }

        public bool ReplaceTextInDocument(string path, Range range, string text)
        {
            var edits = new TextEdit[1] { new ReplaceTextEdit { Range = range, Value = text } };
            return EditTextInDocument(path, edits);
        }

        public bool DeleteTextInDocument(string path, Range range)
        {
            var edits = new TextEdit[1] { new DeleteTextEdit { Range = range } };
            return EditTextInDocument(path, edits);
        }

        public bool EditTextInDocument(string path, IEnumerable<TextEdit> edits)
        {
            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                try
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    var tryOpenResult = VsShellUtilities.TryOpenDocument(serviceProvider, path, Guid.Empty, out _,
                        out _, out IVsWindowFrame windowFrame);
                    if (tryOpenResult == VSConstants.S_OK)
                    {
                        var textView = GetVsTextView(windowFrame);
                        if (textView != null)
                        {
                            var wpfView = editorAdaptersFactoryService.GetWpfTextView(textView);
                            if (wpfView == null)
                            {
                                log.Error($"Cannot get wpf text view for '{path}'");
                                return false;
                            }

                            return ApplyTextEdit(path, edits, textView, wpfView);
                        }

                        log.Error($"Cannot get VsTextView '{path}'");

                    }
                    else
                        log.Error($"Cannot open document '{path}' (error code: {tryOpenResult})");
                }
                catch (Exception ex)
                {
                    log.Error($"Attempt to edit text document failed: '{path}'", ex);
                }


                return false;
            });

            return result;
        }

        private bool ApplyTextEdit(string path, IEnumerable<TextEdit> edits, IVsTextView textView, IWpfTextView wpfView)
        {
            ITextEdit editContext = null;
            try
            {
                var newLineChars = GetNewLineCharsForTextView(textView);
                editContext = wpfView.TextBuffer.CreateEdit();
                {
                    foreach (var edit in edits)
                    {
                        if (edit is InsertTextEdit insert)
                        {
                            var text = insert.Value.ConvertLineBreaks(newLineChars);
                            var position = ToPosition(editContext.Snapshot, insert.Position.Line,
                                insert.Position.Character);

                            editContext.Insert(position, text);
                        }
                        else if (edit is DeleteTextEdit delete)
                        {
                            var range = delete.Range;
                            var startPos = ToPosition(editContext.Snapshot, range.Start.Line, range.Start.Character);
                            var endPos = ToPosition(editContext.Snapshot, range.End.Line, range.End.Character);

                            editContext.Delete(startPos, endPos - startPos);
                        }
                        else if (edit is ReplaceTextEdit replace)
                        {
                            var text = replace.Value.ConvertLineBreaks(newLineChars);
                            var range = replace.Range;
                            if (editContext.Snapshot.Length == 0 && range.Start.IsPosition(0, 0) &&
                                range.End.IsPosition(9999, 0))
                            {
                                // For new files, agent does not specify the exact range, but only one replacement with end line eq. 9999
                                editContext.Insert(0, text);
                            }
                            else
                            {
                                var startPos = ToPosition(editContext.Snapshot, range.Start.Line,
                                    range.Start.Character);
                                var endPos = ToPosition(editContext.Snapshot, range.End.Line, range.End.Character);

                                editContext.Replace(startPos, endPos - startPos, text);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Context edit failed.", ex);
                if (editContext != null)
                    editContext.Dispose();

                return false;
            }

            try
            {
                editContext.Apply();
            }
            catch (InvalidOperationException ex)
            {
                log.Error($"Cannot commit edits in document '{path}'", ex);
                return false;
            }
            finally
            {
                if (editContext != null)
                    editContext.Dispose();
            }

            return true;
        }

        private int ToPosition(ITextSnapshot textSnapshot, int line, int col)
        {
            var containgLine = textSnapshot.GetLineFromLineNumber(line);
            return containgLine.Start.Position + col;
        }

        private string GetNewLineCharsForTextView(IVsTextView view)
        {
            string newLine = null;
            var wpfView = editorAdaptersFactoryService.GetWpfTextView(view);

            if (wpfView != null)
                newLine = wpfView.Options.GetOptionValue(DefaultOptions.NewLineCharacterOptionId);

            return newLine ?? Environment.NewLine;
        }

        private IVsTextView GetVsTextView(IVsWindowFrame windowFrame)
        {
            if (windowFrame == null) return null;

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

        public bool CreateDocument(string path, string content, bool overwrite)
        {
            try
            {
                if (CreateFileWithDirectories(path, content, overwrite)) log.Info($"File '{path}' created.");
                else
                {
                    log.Warn($"File '{path}' already exists and overwriting is not allowed");
                    return false;
                }

                return ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    var project = GetAllProjects()
                        .FirstOrDefault(x => x.IsFileUnderProjectDirectory(path));

                    if (project != null)
                    {
                        return project.AddFile(path);
                    }

                    log.Warn($"Cannot assign file '{path}' to any project");
                    return true;
                });
            }
            catch (Exception ex)
            {
                log.Error($"Cannot create a new file '{path}'", ex);
                return false;
            }
        }

        public bool DeleteDocument(string path)
        {
            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var project = GetAllProjects()
                    .First(x => x.IsFileInProject(path));

                if (project != null)
                {
                    return project.RemoveFile(path);
                }

                log.Warn($"Cannot assign file '{path}' to any project");
                return false;
            });

            try
            {
                if (!result && File.Exists(path)) File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Cannot delete file '{path}'", ex);
                return false;
            }
        }

        private bool CreateFileWithDirectories(string path, string content, bool overwrite)
        {
            if (File.Exists(path) && !overwrite) return false;

            string directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            File.WriteAllText(path, content);

            return true;
        }

        private IEnumerable<ProjectItem> GetAllProjects()
        {
            vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION,
                                   Guid.Empty, out IEnumHierarchies enumHierarchies);

            if (enumHierarchies == null) yield break;

            IVsHierarchy[] hierarchies = new IVsHierarchy[1];
            uint fetched;

            while (enumHierarchies.Next(1, hierarchies, out fetched) == 0 && fetched == 1)
            {
                var hierarchy = hierarchies.First();
                if (hierarchy is IVsProject2 project)
                    yield return new ProjectItem(project);
            }
        }

        public bool RenameDocument(string oldName, string newName)
        {
            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    string directoryPath = Path.GetDirectoryName(newName);
                    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                    File.Move(oldName, newName);

                    VsShellUtilities.RenameDocument(serviceProvider, oldName, newName);
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error($"Failed to rename file from '{oldName}' to '{newName}'", ex);
                    return false;
                }
            });

            return result;
        }

    }
}

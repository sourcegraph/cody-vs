using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            return ChangeTextInDocument(path, new Range { Start = position, End = position }, text);
        }

        public bool ReplaceTextInDocument(string path, Range range, string text)
        {
            return ChangeTextInDocument(path, range, text);
        }

        public bool DeleteTextInDocument(string path, Range range)
        {
            return ChangeTextInDocument(path, range, string.Empty);
        }

        private bool ChangeTextInDocument(string path, Range range, string text)
        {
            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var tryOpenResult = VsShellUtilities.TryOpenDocument(serviceProvider, path, Guid.Empty, out _, out _, out IVsWindowFrame windowFrame);
                if (tryOpenResult == VSConstants.S_OK)
                {
                    var textView = GetVsTextView(windowFrame);
                    if (textView != null)
                    {
                        var bufferResult = textView.GetBuffer(out IVsTextLines textLines);
                        if (bufferResult != VSConstants.S_OK)
                        {
                            log.Error($"Cannot get text buffer (error code: {bufferResult})");
                            return false;
                        }

                        if (!string.IsNullOrEmpty(text))
                        {
                            var newLineChars = GetNewLineCharsForTextView(textView);
                            text = text.ConvertLineBreaks(newLineChars);
                        }

                        textLines.GetSize(out int bufferLength);
                        if (bufferLength == 0 && range.Start.IsPosition(0, 0) && range.End.IsPosition(9999, 0))
                        {
                            // For new files, agent does not specify the exact range, but only one ending in line 9999
                            log.Info($"Initializing content for '{path}'");
                            var initResult = textLines.InitializeContent(text, text.Length);
                            return initResult == VSConstants.S_OK;
                        }

                        var textPtr = IntPtr.Zero;
                        try
                        {
                            var length = (text == null) ? 0 : text.Length;

                            textPtr = Marshal.StringToCoTaskMemAuto(text);
                            var replaceResult = textLines.ReplaceLines(
                                range.Start.Line, range.Start.Character,
                                range.End.Line, range.End.Character,
                                textPtr, length,
                                null);

                            if (replaceResult != VSConstants.S_OK)
                            {
                                log.Error($"Cannot change text in '{path}' (error code: {replaceResult})");
                                return false;
                            }
                        }
                        finally
                        {
                            if (textPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(textPtr);
                        }

                        return true;
                    }
                    else log.Error($"Cannot get VsTextView '{path}'");

                }
                else log.Error($"Cannot open document '{path}' (error code: {tryOpenResult})");

                return false;
            });

            return result;
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
                CreateFileWithDirectories(path, content, overwrite);

                return ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    var project = GetAllProjects()
                        .FirstOrDefault(x => x.IsFileUnderProjectDirectory(path));

                    if (project != null)
                    {
                        return project.AddFile(path);
                    }

                    return false;
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

                return false;
            });

            try
            {
                File.Delete(path);
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

using Cody.Core.Agent.Protocol;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
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

        public DocumentService(ILog log, IServiceProvider serviceProvider)
        {
            this.log = log;
            this.serviceProvider = serviceProvider;
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

                        var textPtr = IntPtr.Zero;
                        var length = (text == null) ? 0 : text.Length;
                        try
                        {
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

    }
}

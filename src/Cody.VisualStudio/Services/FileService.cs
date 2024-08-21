using Cody.Core.Agent.Protocol;
using Cody.Core.Workspace;
using Cody.Core.Logging;
using Cody.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace Cody.VisualStudio.Services
{
    public class FileService : IFileService
    {
        private readonly ILog _logger;
        private readonly IServiceProvider _serviceProvider;

        public FileService(IServiceProvider serviceProvider, ILog logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public bool OpenFileInEditor(string path, Range range = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                string filePath = FilePathHelper.SanitizeFilePath(path);
                if (Uri.TryCreate(filePath, UriKind.Absolute, out Uri fileUri))
                {
                    filePath = fileUri.LocalPath;
                }

                VsShellUtilities.OpenDocument(_serviceProvider, filePath, Guid.Empty, out _, out _, out IVsWindowFrame windowFrame, out IVsTextView textView);
                windowFrame?.Show();

                if (range != null && textView != null)
                {
                    textView.SetCaretPos(range.Start.Line, range.Start.Character);
                    textView.SetSelection(range.Start.Line, range.Start.Character, range.End.Line, range.End.Character);
                    textView.CenterLines(range.Start.Line, 1);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to open file: {path}", ex);
                return false;
            }
        }

    }
}

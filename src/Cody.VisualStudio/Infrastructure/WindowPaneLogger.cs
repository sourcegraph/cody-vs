using System;
using System.Diagnostics;
using System.Threading;
using Cody.Core.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

#pragma warning disable VSTHRD010

namespace Cody.VisualStudio.Inf
{
    public class WindowPaneLogger : IOutputWindowPane
    {
        private readonly IVsOutputWindow _outputWindow;
        private readonly IVsOutputWindowPane _pane;

        private Guid _guid = Guid.NewGuid();

        public WindowPaneLogger(IVsOutputWindow outputWindow, string name)
        {
            _outputWindow = outputWindow;

            if (_outputWindow.GetPane(_guid, out _pane) != VSConstants.S_OK)
            {
                _outputWindow.CreatePane(ref _guid, name, 1, 0);
                _outputWindow.GetPane(ref _guid, out _pane);
            }
            else if (_pane == null)
            {
                throw new Exception("Cannot get IVsOutputWindowPane!");
            }
        }

        private void Log(string message, string logType, string callerName = "")
        {
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var outputString = $"{time}: [ProcessId:{ProcessId}] [ThreadId:{threadId}] {logType} [{callerName}] {message}{Environment.NewLine}";

            _pane.OutputStringThreadSafe(outputString);
        }

        public void Info(string message, string callerName)
        {
            Log(message, "Info", callerName);
        }

        public void Warn(string message, string callerName)
        {
            Log(message, "Warn", callerName);
        }

        public void Debug(string message, string callerName)
        {
            Log(message, "Debug", callerName);
        }

        public void Error(string message, string callerName)
        {
            Log(message, "Error", callerName);

#if DEBUG
            if (_pane != null)
                _pane.Activate();
#endif
        }

        private static int? _processId;
        public static int ProcessId
        {
            get
            {
                if (_processId == null)
                {
                    using (var thisProcess = Process.GetCurrentProcess())
                    {
                        _processId = thisProcess.Id;
                    }
                }
                return _processId.Value;
            }
        }
    }
}

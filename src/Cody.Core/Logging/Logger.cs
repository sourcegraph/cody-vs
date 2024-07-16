﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cody.Core.Logging
{
    public class Logger: ILog
    {
        private IOutputWindowPane _outputWindowPane;

        public Logger()
        {
        }

        public void Info(string message, [CallerMemberName] string callerName = "")
        {
            var customMessage = FormatCallerName(message, callerName);

            // TODO: _fileLogger.Info(customMessage);
            DebugWrite(customMessage);
            _outputWindowPane?.Info(message, callerName);
        }

        public void Debug(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = null)
        {
#if DEBUG
            var callerTypeName = Path.GetFileNameWithoutExtension(callerFilePath);
            callerName = $"{callerTypeName}.{callerName}";
            var customMessage = FormatCallerName(message, callerName);

            // TODO: _fileLogger.Debug(customMessage);
            DebugWrite(customMessage);
            _outputWindowPane?.Debug(message, callerName);
#endif
        }

        public void Debug(string message, Exception ex, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = null)
        {
#if DEBUG
            Debug(message, callerName, callerFilePath);
            Error(message, ex, callerName);
#endif
        }

        public void Warn(string message, [CallerMemberName] string callerName = "")
        {
            // TODO: _fileLogger.Warn(customMessage);
            DebugWrite(message);
            _outputWindowPane?.Warn(message, callerName);
        }

        public void Error(string message, [CallerMemberName] string callerName = "")
        {
            var customMessage = FormatCallerName(message, callerName);

            // TODO: _fileLogger.Error(customMessage);
            DebugWrite(customMessage);
            _outputWindowPane?.Error(message, callerName);
        }

        public void Error(string message, Exception ex, [CallerMemberName] string callerName = "")
        {
            var exceptionDetails = new StringBuilder();
            var originalException = ex;
            while (ex != null)
            {
                exceptionDetails.AppendLine(ex.Message);
                exceptionDetails.AppendLine(ex.StackTrace);

                ex = ex.InnerException;
            }

            var outputMessage = message + Environment.NewLine + exceptionDetails;
            var customMessage = FormatCallerName(outputMessage, callerName);

            // TODO: _fileLogger.Error(originalException, customMessage);
            DebugWrite(customMessage);
            _outputWindowPane?.Error(outputMessage, callerName);
        }

        public Logger WithOutputPane(IOutputWindowPane outputWindowPane)
        {
            _outputWindowPane = outputWindowPane;
            return this;
        }

        public Logger Build()
        {
            return this;
        }
        private void DebugWrite(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        private string FormatCallerName(string message, string callerName)
        {
            var customMessage = !string.IsNullOrEmpty(callerName) ? $"[{callerName}] {message}" : message;

            return customMessage;
        }
    }
}
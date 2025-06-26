using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Cody.Core.Logging;
using Cody.VisualStudio.Inf;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio.Completions
{
    [Export]
    public class LoggerFactory
    {
        private static readonly ConcurrentDictionary<string, ILog> _loggers = new ConcurrentDictionary<string, ILog>();

        public LoggerFactory()
        {
        }

        public ILog Create(string outputName = "Cody Completions")
        {
            return _loggers.GetOrAdd(outputName, CreateLogger);
        }

        private ILog CreateLogger(string outputName = "Cody Completions")
        {
            Logger logger = null;
            SentryLog sentryLog = null;
            try
            {
                sentryLog = new SentryLog();
                SentryLog.Initialize();
                logger = new Logger();

                var failToCreateWindowPaneLogger = false;

                logger = logger.WithSentryForErrors(sentryLog);
                if (!string.IsNullOrEmpty(outputName))
                {
                    try
                    {
                        var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                        var paneLogger = new WindowPaneLogger(outputWindow, outputName);

                        logger = logger.WithOutputPane(paneLogger);
                    }
                    catch
                    {
                        failToCreateWindowPaneLogger = true;
                    }
                }

                logger = logger.Build();
                if (failToCreateWindowPaneLogger) logger.Error("Could not create WindowPaneLogger.");
                else logger.Debug("Logger created.");

                return logger;
            }
            catch (Exception ex)
            {
                if (sentryLog != null && logger != null)
                {
                    logger.Error("Failed to create logger!", ex);
                }
            }

            return null;
        }
    }
}

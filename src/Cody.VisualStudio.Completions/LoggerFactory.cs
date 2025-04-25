using System;
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


        public LoggerFactory()
        {
            
        }

        public ILog Create(string outputName = null)
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

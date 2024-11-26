using Cody.Core.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#pragma warning disable VSTHRD010

namespace Cody.VisualStudio.Inf
{
    public class LoggerFactory
    {
        public ILog Create(string outputName = null)
        {
            if (outputName == null) outputName = WindowPaneLogger.DefaultCody;

            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Logger logger;
            WindowPaneLogger paneLogger = null;

            try
            {
                paneLogger = new WindowPaneLogger(outputWindow, outputName);
            }
            catch
            {
                // ignored
            }

            if (paneLogger != null)
            {
                logger = new Logger()
                    .WithOutputPane(paneLogger)
                    .Build();

                logger.Debug("Logger created.");
            }
            else
            {
                logger = new Logger()
                    .Build();

                logger.Error("Could not create WindowPaneLogger.");
            }

            return logger;
        }
    }
}

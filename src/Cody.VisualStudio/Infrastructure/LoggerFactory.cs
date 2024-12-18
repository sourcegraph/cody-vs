using Cody.Core.Common;
using Cody.Core.Inf;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio.Inf
{
    public class LoggerFactory
    {
        private IVersionService _versionService;

        public ILog Create(string outputName = null)
        {
            Logger logger = new Logger();
            SentryLog sentryLog = new SentryLog();
            bool failToCreateWindowPaneLogger = false;

            logger = logger.WithSentryForErrors(sentryLog);

            if(!string.IsNullOrEmpty(outputName))
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

            if(failToCreateWindowPaneLogger) logger.Error("Could not create WindowPaneLogger.");
            else logger.Debug("Logger created.");

            _versionService = new VersionService(logger);
            var version = _versionService.Full;
            var debugOrRelease = Configuration.IsDebug ? $"Debug (compiled: {_versionService.GetDebugBuildDate()})" : "Release";
            logger.Info($"Version: {version} {debugOrRelease} build");

            return logger;
        }

        public IVersionService GetVersionService()
        {
            return _versionService;
        }
    }
}

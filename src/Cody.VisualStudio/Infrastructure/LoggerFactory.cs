using Cody.Core.Inf;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#pragma warning disable VSTHRD010

namespace Cody.VisualStudio.Inf
{
    public class LoggerFactory
    {
        private IVersionService _versionService;

        public ILog Create(string outputName = null)
        {
            if (outputName == null) outputName = WindowPaneLogger.DefaultCody;

            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Logger logger;
            WindowPaneLogger paneLogger = null;
            SentryLog sentryLog = new SentryLog();

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
                    .WithSentryForErrors(sentryLog)
                    .Build();

                logger.Debug("Logger created.");

                var isDebug = false;
#if DEBUG
                isDebug = true;
#endif
                var build = "VS2022";

                _versionService = new VersionService();
                _versionService.AddBuildMetadata(build, isDebug);

                var version = _versionService.Full;
                var debugOrRelease = _versionService.IsDebug ? "Debug" : "Release";
                logger.Info($"Version: {version} {debugOrRelease} build");
            }
            else
            {
                logger = new Logger()
                    .Build();

                logger.Error("Could not create WindowPaneLogger.");
            }

            return logger;
        }

        public IVersionService GetVersionService()
        {
            return _versionService;

        }
    }
}

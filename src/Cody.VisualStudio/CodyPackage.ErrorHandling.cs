using Cody.Core.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Cody.VisualStudio
{
    public sealed partial class CodyPackage
    {
        private void InitializeErrorHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            SentryLog.Initialize();
        }



        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error($"Unhandled domain exception:{e.ExceptionObject}");
            Logger.Error($"Unhandled domain exception, is terminating:{e.IsTerminating}");

            var exception = e.ExceptionObject as Exception;
            Logger.Error("Unhandled domain exception occurred.", exception);

        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;
            Logger.Error("Unhandled exception occurred on the UI thread.", exception);

            if (!System.Diagnostics.Debugger.IsAttached)
                e.Handled = true;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                if (!System.Diagnostics.Debugger.IsAttached)
                    e.SetObserved();

                var thirdPartyException = e.Exception;
                var exceptionDetails = new StringBuilder();
                if (thirdPartyException != null)
                {
                    exceptionDetails.AppendLine(thirdPartyException.Message);
                    exceptionDetails.AppendLine(thirdPartyException.StackTrace);

                    if (thirdPartyException.InnerExceptions.Any())
                    {
                        foreach (var inner in thirdPartyException.InnerExceptions)
                        {
                            exceptionDetails.AppendLine(inner.Message);
                            exceptionDetails.AppendLine(inner.StackTrace);
                        }
                    }

                    if (!exceptionDetails.ToString().Contains("Cody")) return;
                }

                Logger.Error("Unhandled exception occurred on the non-UI thread.", e.Exception);
                foreach (var ex in e.Exception.InnerExceptions)
                {
                    Logger.Error("Inner exception", ex);
                }
            }
            catch
            {
                // catching everything because if not VS will freeze/crash on the exception
            }
        }
    }
}

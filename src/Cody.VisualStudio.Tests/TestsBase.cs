using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Cody.Core.Logging;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit.Abstractions;
using Thread = System.Threading.Thread;

namespace Cody.VisualStudio.Tests
{
    public abstract class TestsBase : ITestLogger
    {
        private readonly ITestOutputHelper _logger;

        protected CodyPackage CodyPackage;

        protected TestsBase(ITestOutputHelper output)
        {
            _logger = output;
            
            WriteLog("[TestBase] Initialized.");
        }

        public void WriteLog(string message, string type = "", [CallerMemberName] string callerName = "")
        {
            _logger.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{type}] [{callerName}] [ThreadId:{Thread.CurrentThread.ManagedThreadId}] {message}");
        }

        protected async Task<CodyPackage> GetPackageAsync()
        {
            var guid = CodyPackage.PackageGuidString;
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            var codyPackage = (CodyPackage)await shell.LoadPackageAsync(new Guid(guid)); // forces to load CodyPackage, even when the Tool Window is not selected

            CodyPackage = codyPackage;
            var logger = (Logger)CodyPackage.Logger;
            logger.WithTestLogger(this);

            return codyPackage;
        }

        protected async Task WaitForAsync(Func<bool> condition)
        {
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromMinutes(10);
            while (!condition.Invoke())
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                WriteLog("Chat not loaded ...");

                var nowTime = DateTime.Now;
                var currentSpan = nowTime - startTime;
                if (currentSpan >= timeout)
                {
                    var message = $"Chat timeout! It's loading for more than {currentSpan.TotalSeconds} s.";
                    WriteLog(message);
                    throw new Exception(message);
                }
            }

            if (condition.Invoke())
            {
                WriteLog($"Chat loaded.");
            }
        }

        protected async Task OnUIThread(Func<Task> task)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await task();
            });
        }

        protected async Task WaitForChat()
        {
            WriteLog("Waiting for the Chat ...");

            bool isChatLoaded = false;
            await CodyPackage.ShowToolWindowAsync();
            WriteLog("ShowToolWindowAsync called.");

            var viewModel = CodyPackage.MainViewModel;
            await WaitForAsync(() => viewModel.IsChatLoaded);

            isChatLoaded = viewModel.IsChatLoaded;
            WriteLog($"Chat loaded:{isChatLoaded}");
           
        }
    }
}

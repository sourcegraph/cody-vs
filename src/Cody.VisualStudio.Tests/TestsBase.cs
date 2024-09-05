using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public abstract class TestsBase
    {
        private readonly ITestOutputHelper _logger;

        protected CodyPackage CodyPackage;

        protected TestsBase(ITestOutputHelper output)
        {
            _logger = output;
            
            WriteLog("[TestBase] Initialized.");
        }

        protected void WriteLog(string message, [CallerMemberName] string callerName = "")
        {
            _logger.WriteLine($"[{callerName}] {message}");
        }

        protected async Task<CodyPackage> GetPackageAsync()
        {
            var guid = CodyPackage.PackageGuidString;
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            var codyPackage = (CodyPackage)await shell.LoadPackageAsync(new Guid(guid)); // forces to load CodyPackage, even when the Tool Window is not selected

            CodyPackage = codyPackage;

            return codyPackage;
        }

        protected async Task WaitForAsync(Func<bool> condition)
        {
            while (!condition.Invoke())
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                CodyPackage.Logger.Debug($"Chat not loaded ...");
            }

            CodyPackage.Logger.Debug($"Chat loaded.");
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
            bool isChatLoaded = false;
            await CodyPackage.ShowToolWindowAsync();

            var viewModel = CodyPackage.MainViewModel;
            await WaitForAsync(() => viewModel.IsChatLoaded);

            isChatLoaded = viewModel.IsChatLoaded;
            CodyPackage.Logger.Debug($"Chat loaded:{isChatLoaded}");
           
        }
    }
}

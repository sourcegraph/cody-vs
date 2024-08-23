using System;
using System.Threading.Tasks;
using System.Windows;
using Cody.UI.ViewModels;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio.Tests
{
    public abstract class TestsBase
    {
        protected async Task<CodyPackage> GetPackageAsync()
        {
            var guid = CodyPackage.PackageGuidString;
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            var codyPackage = (CodyPackage)await shell.LoadPackageAsync(new Guid(guid)); // forces to load CodyPackage, even when the Tool Window is not selected

            return codyPackage;
        }

        protected async Task WaitForAsync(Func<bool> condition)
        {
            while (!condition.Invoke())
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
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
            var codyPackage = await GetPackageAsync();
            await codyPackage.ShowToolWindowAsync();
            await OnUIThread((async () =>
            {
                var viewModel = (MainViewModel)codyPackage.MainView.DataContext;
                await WaitForAsync(() => viewModel.IsChatLoaded);
            }));
        }
    }
}

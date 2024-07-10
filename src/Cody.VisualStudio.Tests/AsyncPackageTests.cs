using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit;

namespace Cody.VisualStudio.Tests
{
    public class AsyncPackageTests
    {
        [VsTheory(Version = "2022")]
        [InlineData(CodyPackage.PackageGuidString, true)]
        public async Task LoadTestAsync(string guidString, bool expectedSuccess)
        {
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            Assert.NotNull(shell);

            var guid = Guid.Parse(guidString);

            if (expectedSuccess)
                await shell.LoadPackageAsync(ref guid);
            else
                await Assert.ThrowsAnyAsync<Exception>(async () => await shell.LoadPackageAsync(ref guid));
        }

        [VsTheory(Version = "2022")]
        [InlineData(CodyPackage.PackageGuidString)]
        public async Task InvokePackageAsync(string guidString)
        {
            var package = GetPackage();

            package.Logger.Debug("Hello World from VS Integration Tests :)");

            await Task.Delay(TimeSpan.FromSeconds(10));

            Assert.NotNull(package);
        }

        private CodyPackage GetPackage()
        {
            var vsShell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));
            IVsPackage package;
            var guidPackage = new Guid(CodyPackage.PackageGuidString);
            if (vsShell.IsPackageLoaded(ref guidPackage, out package) == Microsoft.VisualStudio.VSConstants.S_OK)
            {
                var currentPackage = (CodyPackage)package;
                return currentPackage;
            }

            return null;
        }
    }
}

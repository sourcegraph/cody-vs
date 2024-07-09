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
        async Task LoadTestAsync(string guidString, bool expectedSuccess)
        {
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            Assert.NotNull(shell);

            var guid = Guid.Parse(guidString);

            if (expectedSuccess)
                await shell.LoadPackageAsync(ref guid);
            else
                await Assert.ThrowsAnyAsync<Exception>(async () => await shell.LoadPackageAsync(ref guid));
        }
    }
}

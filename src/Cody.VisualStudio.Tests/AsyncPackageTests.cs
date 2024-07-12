using System;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
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
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            var guid = Guid.Parse(guidString);
            await shell.LoadPackageAsync(ref guid);

            var package = GetPackage();
            Assert.NotNull(package);

            package.Logger.Debug("Hello World from VS Integration Tests :)");
            await package.ShowToolWindow();

            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {

                var cdpAddress = $"http://127.0.0.1:{9222}";
                //var browser = await Playwright.Chromium.ConnectOverCDPAsync(cdpAddress);

                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.ConnectOverCDPAsync(cdpAddress);

                var context = browser.Contexts[0];
                var page = context.Pages[0];

                await page.GotoAsync("https://playwright.dev");
                var getStarted = page.GetByText("Get Started");

                //await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));

                var test = await getStarted.AllTextContentsAsync();

                var href = await page.EvaluateAsync<string>("document.location.href");

                package.Logger.Debug($"{href}");

                int status = await page.EvaluateAsync<int>(@"async () => {
                      const response = await fetch(location.href);
                      return response.status;
                }");

                package.Logger.Debug($"{status}");

            }
            catch (Exception ex)
            {
                var message = "Playwright failed!";
                package.Logger.Error(message, ex);
                Assert.Fail(message);
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
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

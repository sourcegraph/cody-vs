using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio.Tests
{
    public abstract class PlaywrightTestsBase
    {
        protected string CdpAddress = $"http://127.0.0.1:{9222}";
        
        protected IPlaywright Playwright;
        protected IBrowser Browser;
        
        protected IBrowserContext Context;

        protected CodyPackage CodyPackage;
        protected IPage Page;


        private async Task InitializeAsync()
        {
            CodyPackage = await GetPackageAsync(CodyPackage.PackageGuidString);
            CodyPackage.Logger.Debug("CodyPackage loaded.");

            CodyPackage.ShowToolWindow(this, EventArgs.Empty);
            CodyPackage.Logger.Debug("Tool Window activated.");

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.ConnectOverCDPAsync(CdpAddress);

            CodyPackage.Logger.Debug("Playwright initialized.");

            Context = Browser.Contexts[0];
            Page = Context.Pages[0];
        }

        private async Task<CodyPackage> GetPackageAsync(string guid)
        {
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            var codyPackage = (CodyPackage)await shell.LoadPackageAsync(new Guid(guid)); // forces to load CodyPackage, even when the Tool Window is not selected

            return codyPackage;
        }

        protected async Task WaitForPlaywrightAsync()
        {
            await InitializeAsync();
        }
        
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Cody.VisualStudio.Tests
{
    public abstract class PlaywrightTestsBase: TestsBase
    {
        protected string CdpAddress = $"http://127.0.0.1:{9222}";
        
        protected IPlaywright Playwright;
        protected IBrowser Browser;
        
        protected IBrowserContext Context;

        protected CodyPackage CodyPackage;
        protected IPage Page;


        private async Task InitializeAsync()
        {
            CodyPackage = await GetPackageAsync();
            CodyPackage.Logger.Debug("CodyPackage loaded.");

            await WaitForChat();
            CodyPackage.Logger.Debug("Chat initialized and loaded.");

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.ConnectOverCDPAsync(CdpAddress);

            CodyPackage.Logger.Debug("Playwright initialized.");

            Context = Browser.Contexts[0];
            Page = Context.Pages[0];
        }

        protected async Task WaitForPlaywrightAsync()
        {
            await InitializeAsync();
        }
        
    }
}

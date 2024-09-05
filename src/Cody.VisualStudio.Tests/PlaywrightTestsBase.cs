using System;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Shell;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public abstract class PlaywrightTestsBase: TestsBase
    {
        protected string CdpAddress = $"http://localhost:{9222}";
        
        protected IPlaywright Playwright;
        protected IBrowser Browser;
        
        protected IBrowserContext Context;

        protected IPage Page;

        protected PlaywrightTestsBase(ITestOutputHelper output) : base(output)
        {
        }

        private async Task InitializeAsync()
        {
            await DismissStartWindow();

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

        protected async Task DismissStartWindow()
        {
            await OnUIThread(() =>
            {
                try
                {
                    var dte = (DTE2)Package.GetGlobalService(typeof(DTE));
                    var mainWindow = dte.MainWindow;
                    if (!mainWindow.Visible) // Options -> General -> On Startup open: Start Window
                    {
                        WriteLog("Main IDE Window NOT visible! Bringing it to the front ...");
                        mainWindow.Visible = true;
                        WriteLog("Main IDE Window is now VISIBLE.");
                    }
                }
                catch (Exception ex)
                {
                    var message = "Cannot get MainWindow visible!";
                    WriteLog(message);

                    throw new Exception($"{message}", ex);
                }

                return Task.CompletedTask;
            });
        }

    }
}

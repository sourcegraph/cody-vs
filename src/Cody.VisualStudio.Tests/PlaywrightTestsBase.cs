using System;
using System.Threading;
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
        
        protected static IPlaywright Playwright;
        protected static IBrowser Browser;
        
        protected static IBrowserContext Context;

        protected static IPage Page;

        private static SemaphoreSlim _sync = new SemaphoreSlim(1);
        private static bool _isInitialized;

        protected PlaywrightTestsBase(ITestOutputHelper output) : base(output)
        {
        }

        private async Task InitializeAsync()
        {
            await _sync.WaitAsync();
            if (_isInitialized)
            {
                WriteLog("PlaywrightTestsBase already initialized!");
                return;
            }

            await DismissStartWindow();

            CodyPackage = await GetPackageAsync();
            WriteLog("CodyPackage loaded.");

            await WaitForChat();
            WriteLog("Chat initialized and loaded.");

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            WriteLog("Playwright created.");

            Browser = await Playwright.Chromium.ConnectOverCDPAsync(CdpAddress);

            WriteLog("Playwright connected to the browser.");

            Context = Browser.Contexts[0];
            Page = Context.Pages[0];

            _isInitialized = true;
            _sync.Release();

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

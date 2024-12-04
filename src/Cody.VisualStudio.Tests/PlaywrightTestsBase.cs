using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using EnvDTE;
using EnvDTE80;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Shell;
using Xunit;
using Xunit.Abstractions;
using Window = EnvDTE.Window;

namespace Cody.VisualStudio.Tests
{
    public abstract class PlaywrightTestsBase : TestsBase
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

            try
            {
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

                var accessToken = Environment.GetEnvironmentVariable("Access_Token_UI_Tests");
                if (accessToken != null)
                {
                    WriteLog("Using Access Token.");
                    CodyPackage.UserSettingsService.AccessToken = accessToken;
                }

                Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
                WriteLog("Playwright created.");

                Browser = await Playwright.Chromium.ConnectOverCDPAsync(CdpAddress);

                WriteLog("Playwright connected to the browser.");

                Context = Browser.Contexts[0];
                Page = Context.Pages[0];

                //var playwrightTimeout = (float)TimeSpan.FromMinutes(3).TotalMilliseconds;
                //Page.SetDefaultNavigationTimeout(playwrightTimeout);
                //Page.SetDefaultTimeout(playwrightTimeout);

                _isInitialized = true;
            }
            finally
            {
                _sync.Release();
            }
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

                    var startWindow = GetQuickStartWindow();
                    if (startWindow != null)
                    {
                        WriteLog($"Found `{startWindow.GetType()}` window.");
                        startWindow.Hide();
                        WriteLog($"`{startWindow.GetType()}` is hidden.");
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

        private System.Windows.Window GetQuickStartWindow()
        {
            return PresentationSource.CurrentSources.OfType<HwndSource>()
                .Select(h => h.RootVisual)
                .OfType<System.Windows.Window>()
                .SingleOrDefault(w => w.GetType().Name == "QuickStartWindow");
        }

        protected async Task<string> GetAccessToken()
        {
            await WaitForPlaywrightAsync();

            var accessToken = CodyPackage.UserSettingsService.AccessToken;

            return accessToken;
        }

        protected async Task SetAccessToken(string accessToken)
        {
            WriteLog("Preparing to set access token ...");

            CodyPackage.UserSettingsService.AccessToken = accessToken;
            await Task.Delay(TimeSpan.FromSeconds(2)); // wait for the Chat to response

            WriteLog("Access token set successfully");
        }

        protected async Task AssertTextIsPresent(string text)
        {
            // given

            var getStarted = Page.GetByText(text);
            var textContents = await getStarted.AllTextContentsAsync();

            // then
            Assert.Equal(text, textContents.First());
        }

        protected async Task ShowChatTab()
        {
            await Page.GetByTestId("tab-chat").ClickAsync();
            await Task.Delay(500);
        }

        protected async Task NewChat()
        {
            await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions {Name = "New Chat"}).ClickAsync();

            await Task.Delay(500);
        }

        protected async Task ShowHistoryTab()
        {
            await Page.GetByTestId("tab-history").ClickAsync();
            await Task.Delay(500);
        }

        protected async Task ShowPromptsTab() => await Page.GetByTestId("tab-prompts").ClickAsync();

        protected async Task ShowAccountTab() => await Page.GetByTestId("tab-account").ClickAsync();

        protected async Task ClickNewChat() => await Page.Locator("button span :text-is('New Chat')").ClickAsync();

        protected async Task EnterChatTextAndSend(string prompt)
        {
            var entryArea = Page.Locator("span[data-lexical-text='true']");
            var enterArea = Page.Locator("[data-keep-toolbar-open=true]").Last;

            await entryArea.FillAsync(prompt);
            await enterArea.PressAsync("Enter");

            var isStopVisible = false; 
            while (!isStopVisible)
            {
                isStopVisible = await Page.Locator("vscode-button").First.IsVisibleAsync();
                await Task.Delay(500);
            }
            await Task.Delay(500);

            await DismissStartWindow();
        }

        protected async Task<string[]> GetTodayChatHistory()
        {
            var todaySection = await Page.QuerySelectorAsync("div[id='history-today-content']");

            return (await todaySection.QuerySelectorAllAsync("button span"))
                .Select(async x => await x.TextContentAsync())
                .Select(x => x.Result)
                .ToArray();
        }

        protected async Task<IReadOnlyCollection<ContextTag>> GetChatContextTags()
        {
            var tagsList = new List<ContextTag>();
            await ShowChatTab();

            WriteLog("Searching for Chat ...");
            var chatBox = await Page.QuerySelectorAsync("[aria-label='Chat message']");
            if (chatBox == null)
            {
                WriteLog("Chat NOT found.");
                throw new Exception("ChatBox is null. Probably not authenticated.");
            }
            WriteLog("Chat found.");

            var list = await chatBox.QuerySelectorAllAsync("span[data-lexical-decorator='true']");
            foreach (var item in list)
            {
                var tag = new ContextTag();
                var content = await item.TextContentAsync();
                var parts = content.Split(':');
                tag.Name = parts.First();
                if (parts.Length > 1)
                {
                    var lines = parts[1].Split('-');
                    if (lines.Length > 1)
                    {
                        tag.StartLine = int.Parse(lines[0]);
                        tag.EndLine = int.Parse(lines[1]);
                    }
                    else
                    {
                        tag.StartLine = int.Parse(parts[1]);
                        tag.EndLine = int.Parse(parts[1]);
                    }
                }

                tagsList.Add(tag);
            }

            return tagsList;
        }
    }

    public class ContextTag
    {
        public string Name { get; set; }

        public int? StartLine { get; set; }

        public int? EndLine { get; set; }
    }
}

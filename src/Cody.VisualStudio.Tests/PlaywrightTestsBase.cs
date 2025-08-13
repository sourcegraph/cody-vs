using EnvDTE;
using EnvDTE80;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using SolutionEvents = Microsoft.VisualStudio.Shell.Events.SolutionEvents;
using Xunit;
using Xunit.Abstractions;

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
                    CodyPackage.UserSettingsService.ForceAccessTokenForUITests = true;
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

        protected async Task WaitForChatLoadingWhenLoggedIn()
        {
            WriteLog("Waiting for Cody chat to be fully initialized...");
            await WaitForAsync(async () =>
            {
                try
                {
                    var loadingElement = await Page.QuerySelectorAsync("text=Loading");
                    if (loadingElement != null)
                    {
                        WriteLog("'Loading' text found, waiting for it to disappear...");

                        return false;
                    }

                    WriteLog("'Loading' text not found, Cody is already initialized.");
                    return true;
                }
                catch (Exception ex)
                {
                    WriteLog($"Error while waiting for 'Loading' text to disappear: {ex.Message}");
                }

                return true;
            });
        }

        protected async Task WaitForLogOutState()
        {
            await Page.WaitForSelectorAsync("text=By signing in to Cody");
        }

        protected async Task WaitForLogInState()
        {
            await Page.WaitForSelectorAsync("[data-testid='new-chat-button']");
            await WaitForChatLoadingWhenLoggedIn();
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
                        WriteLog($"`{startWindow.GetType()}` has been hidden.");
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
            Assert.Contains(text, textContents);
            Assert.Equal(text, textContents.First());
        }

        protected async Task ShowChatTab()
        {
            await Page.GetByTestId("tab-chat").ClickAsync();
            await Task.Delay(500);
        }

        protected async Task NewChat()
        {
            await Page.ClickAsync("[data-testid='new-chat-button']");

            await Task.Delay(500);
        }

        protected async Task ShowHistoryTab()
        {
            await Page.ClickAsync("[data-testid='tab-history']");

            await Task.Delay(500);
        }

        protected async Task ShowPromptsTab() => await Page.GetByTestId("tab-prompts").ClickAsync();

        protected async Task ShowAccountTab() => await Page.GetByTestId("tab-account").ClickAsync();

        protected async Task ClickNewChat() => await Page.Locator("button span :text-is('New Chat')").ClickAsync();

        private DTE2 _dte;
        protected DTE2 Dte => _dte ?? (_dte = (DTE2)Package.GetGlobalService(typeof(EnvDTE.DTE)));

        protected async Task EnterChatTextAndSend(string prompt)
        {
            var entryArea = Page.Locator("span[data-lexical-text='true']");
            var enterArea = Page.Locator("[data-keep-toolbar-open=true]").Last;

            await entryArea.FillAsync(prompt);
            await enterArea.PressAsync("Enter");
            await Task.Delay(500);

            string state;
            do
            {
                state = await Page.Locator("button[type='submit']").Last.GetAttributeAsync("title");
                await Task.Delay(500);
            } while (state == "Stop");

            await Task.Delay(500);

            await DismissStartWindow();
        }

        protected async Task<bool> IsPresentInHistory(string entry)
        {
            var todaySection = await Page.QuerySelectorAsync($"div[data-value*='{entry}']");

            if (todaySection != null)
                return true;

            return false;
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

                WriteLog($"Found '{tag}' tag");

                tagsList.Add(tag);
            }

            return tagsList;
        }

        protected async Task OpenSolution(string path)
        {
            WriteLog($"Opening solution '{path}' ...");

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var backgroundLoadTcs = new TaskCompletionSource<bool>();
            EventHandler backgroundLoadHandler = (sender, e) => backgroundLoadTcs.TrySetResult(true);
            SolutionEvents.OnAfterBackgroundSolutionLoadComplete += backgroundLoadHandler;

            Dte.Solution.Open(path);

            // Wait for background load event
            await backgroundLoadTcs.Task;
            SolutionEvents.OnAfterBackgroundSolutionLoadComplete -= backgroundLoadHandler;

            WriteLog("Background solution load complete, performing additional verification...");

            // Wait for solution to be fully loaded by checking IsFullyLoaded property
            // and by waiting for projects to be accessible
            await WaitForAsync(() => {
                try
                {
                    var solutionService = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
                    if (solutionService == null) return Task.FromResult(false);

                    solutionService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object isOpen);
                    solutionService.GetProperty((int)__VSPROPID4.VSPROPID_IsSolutionFullyLoaded, out object isFullyLoaded);
                    var areProjectsAccessible = Dte.Solution.Projects.Count > 0;

                    WriteLog($"Solution status: Open={isOpen}, FullyLoaded={isFullyLoaded}, ProjectsAccessible={areProjectsAccessible}");
                    return Task.FromResult((bool)isOpen && (bool)isFullyLoaded && areProjectsAccessible);
                }
                catch (Exception ex)
                {
                    WriteLog($"Exception while checking solution status: {ex.Message}");
                    return Task.FromResult(false);
                }
            });

            WriteLog("Solution fully loaded and verified.");
            await Task.Delay(TimeSpan.FromSeconds(1));

            await CloseAllDocuments(path);
        }

        protected async Task CloseAllDocuments(string solutionPath)
        {
            try
            {
                WriteLog("Checking if there are opened documents to close ...");

                var documents = _dte.Documents.OfType<Document>();
                var docs = documents as Document[] ?? documents.ToArray();
                var areOpenedDocuments = docs.Any();
                if (areOpenedDocuments) WriteLog($"Closing {docs.Count()} opened documents...");
                foreach (var doc in docs)
                {
                    try
                    {
                        doc.Close(vsSaveChanges.vsSaveChangesYes);
                        await Task.Delay(TimeSpan.FromMilliseconds(100)); // allows to unblock UI thread if it's blocked by closing documents API calls
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"Cannot close document:{doc.FullName} exception:{ex.Message}");
                    }
                }

                WriteLog(areOpenedDocuments ? $"Documents closed." : $"No opened documents to close.");

                // HACK: webview shows last tag for a last opened file, even if this file is closed (bug)
                // All files are closed, so trigger clearing the tag for the last opened file
                var chatPrompt = Page.Locator("[data-lexical-editor=true]");
                await chatPrompt.ClearAsync();


                var tags = await GetChatContextTags();
                if (tags.Count > 0) throw new Exception("Chat's tags not removed properly after closing all files!");

            }
            catch (Exception ex)
            {
                WriteLog($"Failed at closing documents - exception:{ex.Message}");
            }
        }
    }

    public class ContextTag
    {
        public string Name { get; set; }

        public int? StartLine { get; set; }

        public int? EndLine { get; set; }

        public override string ToString()
        {
            if (StartLine.HasValue && EndLine.HasValue)
                return $"'{Name}' {StartLine}-{EndLine}";

            return $"{Name}";
        }
    }
}

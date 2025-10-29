using Microsoft.Playwright;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class SmartApplyTests : PlaywrightTestsBase, IDisposable
    {

        private readonly JoinableTaskContext _context = ThreadHelper.JoinableTaskContext;

        public SmartApplyTests(ITestOutputHelper output) : base(output)
        {
            var testName = $"{GetTestName()}_init";
            TakeScreenshot(testName);

            _context.Factory.Run(async () =>
            {
                await WaitForPlaywrightAsync();

                testName = $"{GetTestName()}_chatLoaded";
                TakeScreenshot(testName);

                await WaitForChatLoadingWhenLoggedIn();
            });

            testName = $"{GetTestName()}_chatInitialized";
            TakeScreenshot(testName);

        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Apply_Suggestion_Is_Modifying_Dummy_Document()
        {
            // TODO: This is a workaround. Without this test, the first SmartApply test fails
            // because file context chips don't appear in the chat input, causing the LLM to
            // respond without code suggestions. Running this dummy test first somehow fixes it.
            // The root cause needs investigation - likely a timing or initialization issue
            // in how file context is added to chat on first document open.
            await NewChat();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Point.cs"));
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Apply_Suggestion_Is_Modifying_Point_Document()
        {
            await NewChat();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Point.cs"));

            var originalText = await GetActiveDocumentText();

            await ApplyLastSuggestionFor("Suggest improvements");

            var modifiedText = await GetActiveDocumentText();

            Assert.NotEqual(modifiedText, originalText);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Apply_Suggestion_Is_Modifying_Manager_Document()
        {
            await NewChat();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs"));

            var originalText = await GetActiveDocumentText();

            await ApplyLastSuggestionFor("Suggest improvements in Print() method");

            var modifiedText = await GetActiveDocumentText();

            Assert.NotEqual(modifiedText, originalText);
        }

        private async Task ApplyLastSuggestionFor(string chatText)
        {
            var contextChipLocator = Page.Locator("[aria-label='Chat message'] span[data-lexical-decorator='true']");
            await contextChipLocator.First.WaitForAsync(new() { Timeout = 5000 });
            
            await EnterChatTextAndSend(chatText);

            var apply = Page.Locator("span", new() { HasText = "Apply" }).Last;

            await apply.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Attached });

            var hasHiddenClass = await apply.EvaluateAsync<bool>(@"element => element.classList.contains('tw-hidden')");
            if (hasHiddenClass)
                await apply.EvaluateAsync("element => element.classList.remove('tw-hidden')");

            await apply.ClickAsync(new() { Force = true });

            await EditAppliedAsync();
        }

        private async Task EditAppliedAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await CodyPackage.DocumentService.EditCompletion;

            WriteLog("Changes applied");
        }

        public void Dispose()
        {
            var testName = $"{GetTestName()}_end";
            TakeScreenshot(testName);
        }
    }
}

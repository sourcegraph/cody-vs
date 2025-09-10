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
    public class SmartApplyTests: PlaywrightTestsBase, IDisposable
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
        public async Task Apply_Suggestion_Is_Modifying_Point_Document()
        {
            // given
            await NewChat();

            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Point.cs"));

            var originalText = await GetActiveDocumentText();

            // when
            await ApplyLastSuggestion();

            var modifiedText = await GetActiveDocumentText();

            // then
            Assert.NotEqual(modifiedText, originalText);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Apply_Suggestion_Is_Modifying_Manager_Document()
        {
            // given
            await NewChat();

            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs"));

            var originalText = await GetActiveDocumentText();

            // when
            await ApplyLastSuggestion();

            var modifiedText = await GetActiveDocumentText();

            // then
            Assert.NotEqual(modifiedText, originalText);
        }

        private async Task ApplyLastSuggestion()
        {
            await EnterChatTextAndSend("Suggest improvements");

            var apply = Page.Locator("span", new() { HasText = "Apply" }).Last;

            // checking if Chat window is too narrow to show "Apply" text
            var hasHiddenClass = await apply.EvaluateAsync<bool>(@"element => element.classList.contains('tw-hidden')");
            if (hasHiddenClass)
                await apply.EvaluateAsync("element => element.classList.remove('tw-hidden')"); // force shows "Apply" text so it will be possible to click on it

            await apply.ClickAsync(new() {Force = true});

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

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
        public async Task Apply_Suggestion_Is_Modifying_Point_Document()
        {
            // given
            await NewChat();

            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Point.cs"));

            var originalText = await GetActiveDocumentText();

            // when
            await ApplyLastSuggestionFor("Suggest improvements");

            var modifiedText = await GetActiveDocumentText();

            // then
            Assert.NotEqual(modifiedText, originalText);
        }

        [VsFact(Version = VsVersion.VS2022, Skip = "Unstable")]
        public async Task Apply_Suggestion_Is_Modifying_Manager_Document()
        {
            // given
            await NewChat();

            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs"));

            var originalText = await GetActiveDocumentText();

            // when
            await ApplyLastSuggestionFor("Suggest improvements in Print() method");

            var modifiedText = await GetActiveDocumentText();

            // then
            Assert.NotEqual(modifiedText, originalText);
        }

        private async Task ApplyLastSuggestionFor(string chatText)
        {
            WriteLog($"Sending chat message: {chatText}");
            await EnterChatTextAndSend(chatText);
            WriteLog("Chat message sent");

            WriteLog("Waiting for chat response to complete...");
            // TODO: Wait for response completion indicator here
            
            WriteLog("Looking for Apply button...");
            var apply = Page.Locator("span", new() { HasText = "Apply" }).Last;

            WriteLog("Waiting for Apply button to exist in DOM...");
            await apply.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Attached });
            WriteLog("Apply button found in DOM");

            WriteLog("Checking if Apply button has hidden class...");
            // checking if Chat window is too narrow to show "Apply" text
            var hasHiddenClass = await apply.EvaluateAsync<bool>(@"element => element.classList.contains('tw-hidden')");
            WriteLog($"Apply button hidden class: {hasHiddenClass}");
            
            if (hasHiddenClass)
            {
                WriteLog("Removing hidden class from Apply button");
                await apply.EvaluateAsync("element => element.classList.remove('tw-hidden')"); // force shows "Apply" text so it will be possible to click on it
            }

            WriteLog("Clicking Apply button...");
            await apply.ClickAsync(new() { Force = true});
            WriteLog("Apply button clicked");

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

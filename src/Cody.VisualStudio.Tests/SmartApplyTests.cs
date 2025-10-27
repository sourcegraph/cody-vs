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
            WriteLog("TEST START: Point.cs");
            
            WriteLog("Step 1: Creating new chat");
            await NewChat();
            WriteLog("Step 1: Complete");

            WriteLog("Step 2: Opening solution");
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            WriteLog("Step 2: Complete");
            
            WriteLog("Step 3: Opening Point.cs");
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Point.cs"));
            WriteLog("Step 3: Complete");

            WriteLog("Step 4: Getting original text");
            var originalText = await GetActiveDocumentText();
            WriteLog($"Step 4: Complete - text length: {originalText.Length}");

            WriteLog("Step 5: Applying suggestion");
            await ApplyLastSuggestionFor("Suggest improvements");
            WriteLog("Step 5: Complete");

            WriteLog("Step 6: Getting modified text");
            var modifiedText = await GetActiveDocumentText();
            WriteLog($"Step 6: Complete - text length: {modifiedText.Length}");

            WriteLog("Step 7: Asserting changes");
            Assert.NotEqual(modifiedText, originalText);
            WriteLog("TEST COMPLETE: Point.cs");
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Apply_Suggestion_Is_Modifying_Manager_Document()
        {
            WriteLog("TEST START: Manager.cs");
            
            WriteLog("Step 1: Creating new chat");
            await NewChat();
            WriteLog("Step 1: Complete");

            WriteLog("Step 2: Opening solution");
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            WriteLog("Step 2: Complete");
            
            WriteLog("Step 3: Opening Manager.cs");
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs"));
            WriteLog("Step 3: Complete");

            WriteLog("Step 4: Getting original text");
            var originalText = await GetActiveDocumentText();
            WriteLog($"Step 4: Complete - text length: {originalText.Length}");

            WriteLog("Step 5: Applying suggestion");
            await ApplyLastSuggestionFor("Suggest improvements");
            WriteLog("Step 5: Complete");

            WriteLog("Step 6: Getting modified text");
            var modifiedText = await GetActiveDocumentText();
            WriteLog($"Step 6: Complete - text length: {modifiedText.Length}");

            WriteLog("Step 7: Asserting changes");
            Assert.NotEqual(modifiedText, originalText);
            WriteLog("TEST COMPLETE: Manager.cs");
        }

        private async Task ApplyLastSuggestionFor(string chatText)
        {
            WriteLog($"ApplyLastSuggestionFor: START - prompt='{chatText}'");
            
            WriteLog("ApplyLastSuggestionFor: Waiting for file context to be added...");
            await Task.Delay(1000);
            
            WriteLog("ApplyLastSuggestionFor: Checking for file context chips in chat input");
            var chatBox = await Page.QuerySelectorAsync("[aria-label='Chat message']");
            if (chatBox != null)
            {
                var contextChips = await chatBox.QuerySelectorAllAsync("span[data-lexical-decorator='true']");
                WriteLog($"ApplyLastSuggestionFor: Found {contextChips.Count} context chip(s)");
                foreach (var chip in contextChips)
                {
                    var chipText = await chip.TextContentAsync();
                    WriteLog($"ApplyLastSuggestionFor: Context chip: '{chipText}'");
                }
            }
            else
            {
                WriteLog("ApplyLastSuggestionFor: WARNING - Chat message area not found!");
            }
            
            WriteLog("ApplyLastSuggestionFor: Entering chat text and sending");
            await EnterChatTextAndSend(chatText);
            WriteLog("ApplyLastSuggestionFor: Chat text sent, response complete");

            WriteLog("ApplyLastSuggestionFor: Looking for Apply button");
            var apply = Page.Locator("span", new() { HasText = "Apply" }).Last;

            WriteLog("ApplyLastSuggestionFor: Waiting for Apply button to attach");
            await apply.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Attached });
            WriteLog("ApplyLastSuggestionFor: Apply button attached");

            WriteLog("ApplyLastSuggestionFor: Checking hidden class");
            var hasHiddenClass = await apply.EvaluateAsync<bool>(@"element => element.classList.contains('tw-hidden')");
            WriteLog($"ApplyLastSuggestionFor: hasHiddenClass={hasHiddenClass}");
            
            if (hasHiddenClass)
            {
                WriteLog("ApplyLastSuggestionFor: Removing hidden class");
                await apply.EvaluateAsync("element => element.classList.remove('tw-hidden')");
            }

            WriteLog("ApplyLastSuggestionFor: Clicking Apply button");
            await apply.ClickAsync(new() { Force = true});
            WriteLog("ApplyLastSuggestionFor: Apply button clicked");

            WriteLog("ApplyLastSuggestionFor: Waiting for edit to be applied");
            await EditAppliedAsync();
            WriteLog("ApplyLastSuggestionFor: COMPLETE");
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

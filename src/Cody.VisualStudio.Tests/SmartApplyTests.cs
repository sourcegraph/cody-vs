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
        public async Task Test1()
        {
            // given
            await NewChat();

            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Point.cs"));

            // when
            await EnterChatTextAndSend("Suggest improvements");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Apply" }).Last.ClickAsync();

            await CodyPackage.DocumentService.EditCompletion;
            await CodyPackage.DocumentService.EditCompletion;

            await Task.Delay(TimeSpan.FromDays(1));
        }

        public void Dispose()
        {
            var testName = $"{GetTestName()}_end";
            TakeScreenshot(testName);
        }
    }
}

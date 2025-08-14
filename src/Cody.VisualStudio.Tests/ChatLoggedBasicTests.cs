using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class ChatLoggedBasicTests : PlaywrightTestsBase, IDisposable
    {
        private readonly JoinableTaskContext _context = ThreadHelper.JoinableTaskContext;

        public ChatLoggedBasicTests(ITestOutputHelper output) : base(output)
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
        public async Task No_Tags_Added_To_Chat_Prompt_After_Solution_Is_Loaded()
        {
            // given
            await NewChat();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));

            // when
            var tags = await GetChatContextTags();

            // then
            Assert.Empty(tags);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Active_File_Name_And_Line_Selection_Is_Showing_In_Chat_Input()
        {
            // given
            await NewChat();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));

            // when
            const int startLine = 3; const int endLine = 5;
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs"), startLine, endLine);
            var tags = await GetChatContextTags();

            // then
            var firstTagName = tags.First().Name;
            var secondTag = tags.ElementAt(1);
            Assert.Equal("Manager.cs", firstTagName);
            Assert.Equal(startLine, secondTag.StartLine);
            Assert.Equal(endLine, secondTag.EndLine);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Active_File_Name_And_Line_Selection_Is_Changing_In_Chat_Input()
        {
            // given
            await NewChat();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));

            // when
            const int startLine = 7; const int endLine = 13;
            var filePath = SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs");
            await OpenDocument(filePath, startLine, endLine);
            var tags = await GetChatContextTags();

            var nameTag = tags.First().Name;
            var secondTag = tags.ElementAt(1);
            Assert.Equal("Manager.cs", nameTag);
            Assert.Equal(startLine, secondTag.StartLine);
            Assert.Equal(endLine, secondTag.EndLine);

            const int changedStartLine = 16; const int changedEndLine = 20;
            await OpenDocument(filePath, changedStartLine, changedEndLine);
            tags = await GetChatContextTags();
            secondTag = tags.ElementAt(1);

            // then
            Assert.Equal(changedStartLine, secondTag.StartLine);
            Assert.Equal(changedEndLine, secondTag.EndLine);

        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Active_File_Match_Current_Chat_Context()
        {
            // given
            await NewChat();

            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));

            // when
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Program.cs"));
            var tags = await GetChatContextTags();

            // then
            var firstTagName = tags.First().Name;
            Assert.Equal("Program.cs", firstTagName);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Can_Chat_Tool_Window_Be_Closed_And_Opened_Again()
        {
            await CloseCodyChatToolWindow();
            var isOpen = IsCodyChatToolWindowOpen();

            MakeScreenShot("closed");
            Assert.False(isOpen);

            await OpenCodyChatToolWindow();
            isOpen = IsCodyChatToolWindowOpen();

            MakeScreenShot("open");
            Assert.True(isOpen);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Entered_Prompt_Show_Up_In_Today_History()
        {
            //given
            await NewChat();
            var num = new Random().Next();
            var prompt = $"How to create const with value {num}?";

            MakeScreenShot("empty_prompt");
            await EnterChatTextAndSend(prompt);
            MakeScreenShot("filled_prompt");

            // when
            await ShowHistoryTab();
            var isPresentInHistory = await IsPresentInHistory(num.ToString());

            MakeScreenShot("history");

            // then
            Assert.True(isPresentInHistory);
        }

        public void Dispose()
        {
            var testName = $"{GetTestName()}_end";
            TakeScreenshot(testName);
        }
    }
}

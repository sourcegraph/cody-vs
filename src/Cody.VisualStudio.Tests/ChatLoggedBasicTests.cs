using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class ChatLoggedBasicTests : PlaywrightTestsBase, IDisposable
    {
        public ChatLoggedBasicTests(ITestOutputHelper output) : base(output)
        {
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Solution_Name_Is_Added_To_Chat_Input()
        {
            // given
            await WaitForPlaywrightAsync();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));

            // when
            var tags = await GetChatContextTags();

            // then
            Assert.Equal("ConsoleApp1", tags.Last().Name);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Active_File_Name_And_Line_Selection_Is_Showing_In_Chat_Input()
        {
            // given
            await WaitForPlaywrightAsync();
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
        public async Task Active_File_Match_Current_Chat_Context()
        {
            // given
            await WaitForPlaywrightAsync();
            await NewChat();

            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));

            // when
            const int startLine = 2; const int endLine = 3;
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Program.cs"), startLine, endLine);
            var tags = await GetChatContextTags();

            // then
            var firstTagName = tags.First().Name;
            var secondTag = tags.ElementAt(1);
            Assert.Equal("Program.cs", firstTagName);
            Assert.Equal(startLine, secondTag.StartLine);
            Assert.Equal(endLine, secondTag.EndLine);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Can_Chat_Tool_Window_Be_Closed_And_Opened_Again()
        {
            await WaitForPlaywrightAsync();

            await CloseCodyChatToolWindow();
            var isOpen = IsCodyChatToolWindowOpen();
            Assert.False(isOpen);

            await OpenCodyChatToolWindow();
            isOpen = IsCodyChatToolWindowOpen();
            Assert.True(isOpen);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Entered_Prompt_Show_Up_In_Today_History()
        {
            var num = new Random().Next();
            var prompt = $"How to create const with value {num}?";

            await WaitForPlaywrightAsync();
            TakeScreenshot($"{GetTestName()}_1");

            await EnterChatTextAndSend(prompt);

            await ShowHistoryTab();
            var chatHistoryEntries = await GetTodayChatHistory();

            Assert.Contains(chatHistoryEntries, x => x.Contains(prompt));
        }

        public void Dispose()
        {
            var testName = GetTestName();
            TakeScreenshot(testName);
        }
    }
}

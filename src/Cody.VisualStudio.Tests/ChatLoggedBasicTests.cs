using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cody.VisualStudio.Tests
{
    public class ChatLoggedBasicTests: PlaywrightInitializationTests
    {
        [VsFact(Version = VsVersion.VS2022)]
        public async Task Loads_Properly_InLoggedState()
        {
            // given
            var codyPackage = await GetPackageAsync();
            var settingsService = codyPackage.UserSettingsService;
            var accessToken = codyPackage.UserSettingsService.AccessToken;

            var text = "Cody Free or Cody Pro";
            IReadOnlyList<string> textContents;
            try
            {
                await WaitForPlaywrightAsync();
                codyPackage.UserSettingsService.AccessToken += "INVALID";
                await Task.Delay(TimeSpan.FromMilliseconds(500)); // wait for the Chat to response

                // when

                var getStarted = Page.GetByText(text);
                textContents = await getStarted.AllTextContentsAsync();
            }
            finally
            {
                settingsService.AccessToken = accessToken; // make it valid
            }

            // then
            Assert.Equal(text, textContents.First());
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Solution_Name_Is_Added_To_Chat_Input()
        {
            // given
            OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await WaitForPlaywrightAsync();

            // when
            var tags = await GetChatContextTags();

            // then
            Assert.Equal("ConsoleApp1", tags.Last().Name);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Active_File_Name_And_Line_Selection_Is_Showing_In_Chat_Input()
        {
            // given
            OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await WaitForPlaywrightAsync();

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
        public async Task Can_you_close_and_reopen_chat_tool_window()
        {
            await WaitForPlaywrightAsync();

            await CloseCodyChatToolWindow();
            var isOpen = IsCodyChatToolWindowOpen();
            Assert.False(isOpen);

            await OpenCodyChatToolWindow();
            isOpen = IsCodyChatToolWindowOpen();
            Assert.True(isOpen);
        }

        //[VsFact(Version = VsVersion.VS2022)]
        public async Task Does_chat_history_show_up_after_you_have_submitting_a_chat_close_and_reopen_window()
        {
            var num = new Random().Next();
            var prompt = $"How to create const with value {num}?";

            await WaitForPlaywrightAsync();

            await EnterChatTextAndSend(prompt);
            await CloseCodyChatToolWindow();

            await OpenCodyChatToolWindow();
            await ShowHistoryTab();
            var chatHistoryEntries = await GetTodayChatHistory();

            Assert.Contains(chatHistoryEntries, x => x.Contains(prompt));

        }
    }
}

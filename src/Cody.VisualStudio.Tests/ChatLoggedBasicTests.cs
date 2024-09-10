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
        public async Task Solution_name_is_added_to_chat_input()
        {
            OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await WaitForPlaywrightAsync();

            var tags = await GetChatContextTags();

            Assert.Equal("ConsoleApp1", tags.First().Name);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Active_file_name_and_line_selection_is_showing_in_chat_input()
        {
            const int startLine = 3; const int endLine = 5;

            OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await WaitForPlaywrightAsync();

            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs"), startLine, endLine);
            var tags = await GetChatContextTags();

            Assert.Equal("Manager.cs", tags.Last().Name);
            Assert.Equal(startLine, tags.Last().StartLine);
            Assert.Equal(endLine, tags.Last().EndLine);
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

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Does_chat_history_show_up_after_you_have_submitting_a_chat_close_and_reopen_window()
        {
            int num = new Random().Next();
            string propt = $"How to create const with value {num}?";

            await WaitForPlaywrightAsync();

            await EnterChatTextAndSend(propt);
            await CloseCodyChatToolWindow();

            await OpenCodyChatToolWindow();
            await ShowHistoryTab();
            var chatHistoryEntries = await GetTodaysChatHistory();

            Assert.Contains(chatHistoryEntries, x => x.Contains(propt));

        }
    }
}

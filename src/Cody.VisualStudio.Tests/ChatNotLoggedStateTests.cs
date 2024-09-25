using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class ChatNotLoggedStateTests : PlaywrightTestsBase
    {
        public ChatNotLoggedStateTests(ITestOutputHelper output) : base(output)
        {
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Loads_Properly_InNotLoggedState()
        {
            // given
            var codyPackage = await GetPackageAsync();
            var settingsService = codyPackage.UserSettingsService;
            settingsService.AccessToken = settingsService.AccessToken.Replace("INVALID", "");
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
                if (accessToken != null)
                    settingsService.AccessToken = accessToken; // make it valid
            }

            // then
            Assert.Equal(text, textContents.First());
        }
    }
}

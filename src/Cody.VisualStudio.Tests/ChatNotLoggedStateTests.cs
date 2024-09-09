using System;
using System.Linq;
using System.Threading.Tasks;
using Cody.VisualStudio.Options;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class ChatNotLoggedStateTests : PlaywrightTestsBase
    {
        public ChatNotLoggedStateTests(ITestOutputHelper output) : base(output)
        {
        }

        // WIP
        [VsFact(Version = VsVersion.VS2022)]
        public async Task Loads_Properly_InNotLoggedState()
        {
            // given
            var codyPackage = await GetPackageAsync();
            var settingsService = codyPackage.UserSettingsService;
            var accessToken = codyPackage.UserSettingsService.AccessToken;

            codyPackage.UserSettingsService.AccessToken = "";

            await WaitForPlaywrightAsync();
            

            // when
            var text = "Cody Free or Cody Pro";
            var getStarted = Page.GetByText(text);
            var textContents = await getStarted.AllTextContentsAsync();

            settingsService.AccessToken = accessToken; // make it valid

            // then
            Assert.Equal(text, textContents.First());
        }
    }
}

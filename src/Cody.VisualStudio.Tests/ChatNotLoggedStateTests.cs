using System;
using System.Linq;
using System.Threading.Tasks;
using Cody.VisualStudio.Options;
using Xunit;

namespace Cody.VisualStudio.Tests
{
    public class ChatNotLoggedStateTests : PlaywrightTestsBase
    {
        // WIP
        //[VsFact(Version = VsVersion.VS2022)]
        public async Task Loads_Properly_InNotLoggedState()
        {
            // given
            await GetPackageAsync();
            CodyPackage.ShowOptionPage(typeof(GeneralOptionsPage));
            await Task.Delay(TimeSpan.FromSeconds(1)); // HACK: properly wait for Options page

            var accessToken = CodyPackage.GeneralOptionsViewModel.AccessToken;
            CodyPackage.GeneralOptionsViewModel.AccessToken = $"{accessToken}INVALID"; // make it invalid

            await WaitForPlaywrightAsync();
            

            // when
            var text = "Cody Free or Cody Pro";
            var getStarted = Page.GetByText(text);
            var textContents = await getStarted.AllTextContentsAsync();

            CodyPackage.GeneralOptionsViewModel.AccessToken = $"{accessToken}"; // make it valid

            // then
            Assert.Equal(text, textContents.First());
        }
    }
}

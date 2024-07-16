using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cody.VisualStudio.Tests
{
    public class PlaywrightInitializationTests : PlaywrightTestsBase
    {

        [VsTheory(Version = VsVersion.VS2022)]
        [InlineData(CodyPackage.PackageGuidString)]
        public async Task Playwright_Conntects_OvercCDP(string guidString)
        {
            // given
            await WaitForPlaywrightAsync();

            // when
            // then
            Assert.NotNull(Playwright);
            Assert.NotNull(Browser);
            Assert.NotNull(Context);
            Assert.NotNull(Page);
        }

        [VsTheory(Version = VsVersion.VS2022)]
        [InlineData(CodyPackage.PackageGuidString)]
        public async Task Url_Redirection_Works(string guidString)
        {
            // given
            await WaitForPlaywrightAsync();
            var url = "https://playwright.dev/";

            // when
            await Page.GotoAsync(url);
            var redirectedUrl = await Page.EvaluateAsync<string>("document.location.href");

            // then
            Assert.True(url == redirectedUrl);
        }

        [VsTheory(Version = VsVersion.VS2022)]
        [InlineData(CodyPackage.PackageGuidString)]
        public async Task Searching_ForText_Works(string guidString)
        {
            // given
            await WaitForPlaywrightAsync();
            var url = "https://playwright.dev/";
            await Page.GotoAsync(url);

            // when
            var text = "Get started";
            var getStarted = Page.GetByText(text);
            var textContents = await getStarted.AllTextContentsAsync();

            // then
            Assert.Equal(text, textContents.First());
        }

        [VsTheory(Version = VsVersion.VS2022)]
        [InlineData(CodyPackage.PackageGuidString)]
        public async Task InvokeJS_Get_Status(string guidString)
        {
            // given
            await WaitForPlaywrightAsync();

            // when
            var status = await Page.EvaluateAsync<int>(@"async () => {
                      const response = await fetch(location.href);
                      return response.status;
                }");

            // then
            Assert.Equal(200, status);
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class PlaywrightInitializationTests : PlaywrightTestsBase
    {
        public PlaywrightInitializationTests(ITestOutputHelper output) : base(output)
        {
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Playwright_Connects_OvercCDP()
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

        //[VsFact(Version = VsVersion.VS2022)]
        public async Task Url_Redirection_Works()
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

        //[VsFact(Version = VsVersion.VS2022)]
        public async Task Searching_ForText_Works()
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

        //[VsFact(Version = VsVersion.VS2022)]
        public async Task InvokeJS_Get_Status()
        {
            // given
            await WaitForPlaywrightAsync();
            var url = "https://playwright.dev/";
            await Page.GotoAsync(url);

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

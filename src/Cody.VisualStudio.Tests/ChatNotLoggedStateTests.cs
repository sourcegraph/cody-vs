using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using Xunit;
using Xunit.Abstractions;
using Microsoft.VisualStudio.Shell;

namespace Cody.VisualStudio.Tests
{
    public class ChatNotLoggedStateTests : PlaywrightTestsBase, IDisposable
    {
        private readonly JoinableTaskContext _context = ThreadHelper.JoinableTaskContext;

        private string _accessToken;

        public ChatNotLoggedStateTests(ITestOutputHelper output) : base(output)
        {
            _context.Factory.Run(async () =>
            {
                await WaitForPlaywrightAsync();
                _accessToken = await GetAccessToken();

                if (_accessToken != null)
                    await SetAccessToken("INVALID");
            });

        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Cody_Free_Cody_Pro_Section_Is_Present()
        {
            // given
            var sectionText = "Cody Free or Cody Pro";
            var buttonText = "Sign In with GitHub";

            // then
            await AssertTestIsPresent(sectionText);
            await AssertTestIsPresent(buttonText);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Cody_Enterprise_Section_Is_Present()
        {
            // given
            var sectionText = "Cody Enterprise";
            var browserButtonText = " Sign In with Browser";
            var tokenButtonText = " Sign In with Access Token";

            // then
            await AssertTestIsPresent(sectionText);
            await AssertTestIsPresent(browserButtonText);
            await AssertTestIsPresent(tokenButtonText);
        }

        private async Task AssertTestIsPresent(string text)
        {
            // given

            var getStarted = Page.GetByText(text);
            var textContents = await getStarted.AllTextContentsAsync();

            // then
            Assert.Equal(text, textContents.First());
        }

        public async void Dispose()
        {
            try
            {
                var testName = GetTestName();
                TakeScreenshot(testName);

                if (_accessToken != null)
                    await SetAccessToken(_accessToken); // make it valid
            }
            catch (Exception ex)
            {
                WriteLog($"Dispose() for {GetTestName()} failed.");
            }
        }
    }
}

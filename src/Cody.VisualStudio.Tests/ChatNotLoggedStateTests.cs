using System;
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
                {
                    WriteLog("Making access token invalid ...");
                    await SetAccessToken("INVALID");

                    WriteLog("Invalid token set.");
                }
            });
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Cody_Free_Cody_Pro_Section_Is_Present()
        {
            // given
            var sectionText = "Cody Free or Cody Pro";
            var buttonText = "Sign In with GitHub";

            // then
            await AssertTextIsPresent(sectionText);
            await AssertTextIsPresent(buttonText);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Cody_Enterprise_Section_Is_Present()
        {
            // given
            var sectionText = "Cody Enterprise";
            var browserButtonText = " Sign In with Browser";
            var tokenButtonText = " Sign In with Access Token";

            // then
            await AssertTextIsPresent(sectionText);
            await AssertTextIsPresent(browserButtonText);
            await AssertTextIsPresent(tokenButtonText);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Logins_With_GitLab_Google_Are_Present()
        {
            // given
            var gitlabButtonText = "Sign In with GitLab";
            var googleButtonText = "Sign In with Google";

            // then
            await AssertTextIsPresent(gitlabButtonText);
            await AssertTextIsPresent(googleButtonText);
        }


        public async void Dispose()
        {
            try
            {
                var testName = GetTestName();
                TakeScreenshot(testName);

                if (_accessToken != null)
                {
                    WriteLog("Reverting the access token ...");
                    await SetAccessToken(_accessToken); // make it valid
                    WriteLog("The access token reverted.");
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Dispose() for {GetTestName()} failed.");
            }
        }
    }
}

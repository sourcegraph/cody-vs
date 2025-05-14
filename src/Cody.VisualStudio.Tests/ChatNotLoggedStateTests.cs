using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class ChatNotLoggedStateTests : PlaywrightTestsBase
    {
        private readonly JoinableTaskContext _context = ThreadHelper.JoinableTaskContext;

        private string _accessToken;

        public ChatNotLoggedStateTests(ITestOutputHelper output) : base(output)
        {
            _context.Factory.Run(async () =>
            {
                await WaitForPlaywrightAsync();
            });
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Cody_Free_Cody_Pro_Section_Is_Present()
        {
            // given
            var sectionText = "Free or Pro";
            var buttonText = "Continue with GitHub";

            // then
            await NotInLoggedState(async () =>
            {
                await AssertTextIsPresent(sectionText);
                await AssertTextIsPresent(buttonText);
            });

        }

        [VsFact(Version = VsVersion.VS2022, Skip = "Unstable")]
        public async Task Cody_Enterprise_Section_Is_Present()
        {
            // given
            var sectionText = "Enterprise";
            var browserButtonText = "Continue with a URL";

            // then
            await NotInLoggedState(async () =>
            {
                await AssertTextIsPresent(sectionText);
                await AssertTextIsPresent(browserButtonText);
            });
        }

        [VsFact(Version = VsVersion.VS2022, Skip = "need update to 1.66")]
        public async Task Logins_With_GitLab_Google_Are_Present()
        {
            // given
            var gitlabButtonText = "Continue with GitLab";
            var googleButtonText = "Continue with Google";

            // then
            await NotInLoggedState(async () =>
            {
                await AssertTextIsPresent(gitlabButtonText);
                await AssertTextIsPresent(googleButtonText);
            });
        }

        private async Task NotInLoggedState(Func<Task> action)
        {
            try
            {
                await UseInvalidToken();
                await action();
            }
            finally
            {
                await RevertToken();
            }
        }

        private async Task UseInvalidToken()
        {
            _accessToken = await GetAccessToken();
            if (_accessToken != null)
            {
                WriteLog("Making access token invalid ...");
                await SetAccessToken("INVALID");

                WriteLog("Invalid token set.");
            }
        }

        private async Task RevertToken()
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
    }
}

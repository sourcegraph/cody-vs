using Microsoft.Playwright;
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
                await OpenCodyChatToolWindow();
            });
        }

        [VsFact(Version = VsVersion.VS2022, Skip = "Unstable")]
        public async Task Cody_Enterprise_Section_Is_Present()
        {
            // given
            var sectionText = "Sign in to Sourcegraph";
            var browserButtonText = "Sourcegraph Instance URL";

            // then
            await NotInLoggedState(async () =>
            {
                await AssertTextIsPresent(sectionText);
                await AssertTextIsPresent(browserButtonText);
            });
        }

        private async Task NotInLoggedState(Func<Task> action)
        {
            try
            {
                MakeScreenShot("init");

                await UseInvalidToken();
                MakeScreenShot("invalid_token_set");
                await action();
                MakeScreenShot("action");
            }
            finally
            {
                await RevertToken();

                MakeScreenShot("token_reverted");
            }
        }

        private async Task UseInvalidToken()
        {
            _accessToken = await GetAccessToken();
            if (_accessToken != null)
            {
                WriteLog("Making access token invalid ...");

                await SetAccessToken("INVALID");
                await WaitForLogOutState();

                WriteLog("Invalid token set.");
            }
        }

        private async Task RevertToken()
        {
            if (_accessToken != null)
            {
                WriteLog("Reverting the access token ...");

                await SetAccessToken(_accessToken); // make it valid
                await WaitForLogInState();

                WriteLog("The access token reverted.");
            }
        }
    }
}

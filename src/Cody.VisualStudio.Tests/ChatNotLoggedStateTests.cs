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

        public ChatNotLoggedStateTests(ITestOutputHelper output) : base(output)
        {
            _context.Factory.Run(async () =>
            {
                await WaitForPlaywrightAsync();
            });
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Loads_Properly_InNotLoggedState()
        {
            // given

            var text = "Cody Free or Cody Pro";
            IReadOnlyList<string> textContents;
            string accessToken = null;
            try
            {
                //await WaitForPlaywrightAsync();

                accessToken = await GetAccessToken();
                if (accessToken != null)
                    await SetAccessToken("INVALID");

                // when
                var getStarted = Page.GetByText(text);
                textContents = await getStarted.AllTextContentsAsync();
            }
            finally
            {
                if (accessToken != null)
                    await SetAccessToken(accessToken); // make it valid
            }

            // then
            Assert.Equal(text, textContents.First());
        }

        public void Dispose()
        {
            var testName = GetTestName();
            TakeScreenshot(testName);
        }
    }
}

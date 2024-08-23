using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cody.VisualStudio.Tests
{
    public class ChatLoggedBasicTests: PlaywrightInitializationTests
    {
        [VsFact(Version = VsVersion.VS2022)]
        public async Task Loads_Properly_InLoggedState()
        {
            // given
            await WaitForPlaywrightAsync();

            // when
            var text = "Prompts & Commands";
            var getStarted = Page.GetByText(text);
            var textContents = await getStarted.AllTextContentsAsync();

            // then
            Assert.Equal(text, textContents.First());
        }
    }
}

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class CodyPackageTests : TestsBase
    {
        public CodyPackageTests(ITestOutputHelper output) : base(output)
        {
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task CodyPackage_Loaded_OnDemand()
        {
            // given
            // when
            var codyPackage = await GetPackageAsync();

            // then
            Assert.NotNull(codyPackage);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Logger_Initialized_And_Info_MethodCalled()
        {
            // given
            var codyPackage = await GetPackageAsync();

            // when
            var logger = codyPackage.Logger;
            Assert.NotNull(logger);

            logger.Info("Hello World from integration tests!");

            // then
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task CodyToolWindow_Activated()
        {
            // given
            var codyPackage = await GetPackageAsync();

            // when
            await codyPackage.ShowToolWindowAsync();

            // then
            Assert.NotNull(codyPackage.CodyWebView);
        }
    }
}

using System;
using System.Threading.Tasks;
using Xunit;

namespace Cody.VisualStudio.Tests
{
    public class CodyPackageTests : TestsBase
    {

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
            codyPackage.ShowToolWindow(this, EventArgs.Empty);

            // then
            Assert.NotNull(codyPackage.MainView);
        }
    }
}

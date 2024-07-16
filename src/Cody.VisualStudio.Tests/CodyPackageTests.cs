using System;
using System.Threading.Tasks;
using Xunit;

namespace Cody.VisualStudio.Tests
{
    public class CodyPackageTests : TestsBase
    {

        [VsTheory(Version = VsVersion.VS2022)]
        [InlineData(CodyPackage.PackageGuidString)]
        public async Task CodyPackage_Loaded_OnDemand(string guidString)
        {
            // given
            // when
            var codyPackage = await GetPackageAsync(guidString);

            // then
            Assert.NotNull(codyPackage);
        }

        [VsTheory(Version = VsVersion.VS2022)]
        [InlineData(CodyPackage.PackageGuidString)]
        public async Task Logger_Initialized_And_Info_MethodCalled(string guidString)
        {
            // given
            var codyPackage = await GetPackageAsync(guidString);

            // when
            var logger = codyPackage.Logger;
            Assert.NotNull(logger);

            logger.Info("Hello World from integration tests!");

            // then
        }


        [VsTheory(Version = VsVersion.VS2022)]
        [InlineData(CodyPackage.PackageGuidString)]
        public async Task CodyToolWindow_Activated(string guidString)
        {
            // given
            var codyPackage = await GetPackageAsync(guidString);

            // when
            codyPackage.ShowToolWindow(this, EventArgs.Empty);

            // then
            Assert.NotNull(codyPackage.MainView);
        }
    }
}

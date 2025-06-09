using Cody.Core.Ide;
using Cody.Core.Inf;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Moq;
using NUnit.Framework;

namespace Cody.Core.Tests
{
    public class CustomConfigurationTests
    {
        private ConfigurationService _sut;

        private Mock<IVersionService> _versionServiceMock;
        private Mock<IVsVersionService> _vsVersionServiceMock;
        private Mock<ISolutionService> _solutionServiceMock;
        private Mock<IUserSettingsService> _userSettingsServiceMock;
        private Mock<ILog> _loggerMock;


        [SetUp]
        public void Setup()
        {
            _versionServiceMock = new Mock<IVersionService>();
            _vsVersionServiceMock = new Mock<IVsVersionService>();
            _solutionServiceMock = new Mock<ISolutionService>();
            _userSettingsServiceMock = new Mock<IUserSettingsService>();
            _loggerMock = new Mock<ILog>();

            _sut = new ConfigurationService(_versionServiceMock.Object, _vsVersionServiceMock.Object, _solutionServiceMock.Object, _userSettingsServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Simple_JSON_Data_Should_Be_Serializable()
        {
            // given
            var entry1 = "'cody.autocomplete.enabled'";
            var entry1Value = false;
            var entry2 = "'cody.experimental.urlContext'";
            var entry2Value = true;
            var configurationJson =
                            $@"{{
                                 {entry1}: {entry1Value.ToString().ToLower()},
                                 {entry2}: {entry2Value.ToString().ToLower()}
                             }}";
            _userSettingsServiceMock.Setup(m => m.CustomConfiguration).Returns(configurationJson);

            // when
            var config = _sut.GetCustomConfiguration();

            // then
            var entry1Key = entry1.Replace(@"'", string.Empty);
            var entry2Key = entry2.Replace(@"'", string.Empty);

            Assert.That(config, Contains.Key(entry1Key).WithValue(entry1Value));
            Assert.That(config, Contains.Key(entry2Key).WithValue(entry2Value));

        }

        [Test]
        public void Complex_JSON_Data_Should_Be_Serializable()
        {
            // given
            var key1 = "cody.autocomplete.enabled";
            var key2 = "cody.customHeaders";
            var key3 = "cody.excludeFiles";
            var configurationJson = $@"{{
                        	""{key1}"": true,
	                        ""{key2}"": {{
		                        ""Accept-Encoding"": ""identity""
	                        }},
	                        ""{key3}"": [
		                        "".json"",
		                        ""test.ts""
	                        ],
                          }}";
            _userSettingsServiceMock.Setup(m => m.CustomConfiguration).Returns(configurationJson);

            // when
            var config = _sut.GetCustomConfiguration();

            // then
            Assert.That(config, Contains.Key(key1));
            Assert.That(config, Contains.Key(key2));
            Assert.That(config, Contains.Key(key3));
        }


        [Test]
        public void CustomConfiguration_Malformed_JSON_Should_Return_Null()
        {
            // given
            var configurationJson =
                            @"{
                                 cody.autocomplete.enabled: true
                             }";
            _userSettingsServiceMock.Setup(m => m.CustomConfiguration).Returns(configurationJson);

            // when
            var config = _sut.GetCustomConfiguration();

            // then
            Assert.That(config, Is.Empty);
        }
    }
}

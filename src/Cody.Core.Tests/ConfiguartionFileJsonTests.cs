using Cody.Core.Common;
using Cody.Core.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Tests
{
    public class ConfiguartionFileJsonTests
    {
        private Mock<ILog> _loggerMock;

        private string _tempJsonFile;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILog>();
            Configuration.Initialize(_loggerMock.Object);

            var rawJson = @"{
	                    ""AgentDebug"": true,
	                    ""AgentVerboseDebug"": true,
	                    ""AllowNodeDebug"": true,
	                    
	                    ""AgentDirectory"": ""C:/dev/cody/agent/dist"",
	                    
	                    ""RemoteAgentPort"": 3113,
	                    
	                    ""Trace"": true,
	                    ""TraceFile"": ""C:/tmp/cody.log"",
	                    
	                    ""ShowCodyAgentOutput"": true,
	                    ""ShowCodyNotificationsOutput"": true
                    }";
            _tempJsonFile = Path.GetTempFileName();
            File.WriteAllText(_tempJsonFile, rawJson);
        }

        [Test]
        public void Parsing_Json_Dev_Config_Should_Not_Call_LoggerError_When_Accessing_RemoteAgentPort()
        {
            // given

            // when
            Configuration.AddFromJsonFile(_tempJsonFile);
            int? remoteAgentPort = Configuration.RemoteAgentPort;

            // then
            _loggerMock.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.That(remoteAgentPort, Is.Not.Null);
        }

        [TearDown]
        public void CleanUp()
        {
            if (File.Exists(_tempJsonFile))
                File.Delete(_tempJsonFile);
        }
    }
}

using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Xsd
{
    [TestFixture]
    public class ParseXsdClassesTest
    {
        private const string Directory = @"XsdClassesXml";
        private ILogHandler logHandler;
        private DelftConfigXmlFileParser delftConfigXmlParser;

        [SetUp]
        public void SetUp()
        {
            logHandler = MockRepository.GenerateMock<ILogHandler>();
            delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler.VerifyAllExpectations();

            logHandler = null;
            delftConfigXmlParser = null;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingTimeSeriesImportXmlFilesDoesNotThrown()
        {
            // Given
            const string fileName = "timeseries_import.xml";
            string path = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), Directory, fileName));
            Assert.True(File.Exists(path), $"File path '{path}' should exist.");

            SetExpectationReportedInfoMessageLogHandler(fileName);

            // When
            var dataAccessModel = delftConfigXmlParser.Read<TimeSeriesCollectionComplexType>(path);

            // Then
            Assert.NotNull(dataAccessModel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingToolsConfigXmlFilesDoesNotThrow()
        {
            // Given
            const string fileName = "rtcToolsConfig.xml";
            string path = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), Directory, fileName));
            Assert.True(File.Exists(path), $"File path '{path}' should exist.");

            SetExpectationReportedInfoMessageLogHandler(fileName);

            // When
            var dataAccessModel = delftConfigXmlParser.Read<RtcToolsConfigXML>(path);

            // Then
            Assert.NotNull(dataAccessModel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingRuntimeConfigXmlFilesDoesNotThrow()
        {
            // Given
            const string fileName = "rtcRuntimeConfig.xml";
            string path = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), Directory, fileName));
            Assert.True(File.Exists(path), $"File path '{path}' should exist.");

            SetExpectationReportedInfoMessageLogHandler(fileName);

            // When
            var dataAccessModel = delftConfigXmlParser.Read<RtcRuntimeConfigXML>(path);

            // Then
            Assert.NotNull(dataAccessModel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingDataConfigXmlFilesDoesNotThrow()
        {
            // Given
            const string fileName = "rtcDataConfig.xml";
            string path = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), Directory, fileName));
            Assert.True(File.Exists(path), $"File path '{path}' should exist.");

            SetExpectationReportedInfoMessageLogHandler(fileName);

            // When
            var dataAccessModel = delftConfigXmlParser.Read<RTCDataConfigXML>(path);

            // Then
            Assert.NotNull(dataAccessModel);
        }

        private void SetExpectationReportedInfoMessageLogHandler(string fileName)
        {
            var attributeString = "Attribute: \"xsi:schemaLocation\"";

            string argExpectation = Arg<string>.Matches(arg => arg.Contains(fileName) && arg.Contains(attributeString));

            logHandler.Expect(obj => obj.ReportInfo(argExpectation))
                      .Repeat.Once().Message(
                          "ReportInfo method was not called with an argument containing all of the following strings: " +
                          $"\"{attributeString}\" \"{fileName}\"");
        }
    }
}
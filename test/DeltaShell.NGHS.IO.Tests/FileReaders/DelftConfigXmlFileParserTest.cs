using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Handlers;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Xml;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class DelftConfigXmlFileParserTest
    {
        private string XmlFileDirectory = @"FileReaders\ConfigXmlReader";

        #region General Exception tests

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
            logHandler = null;
            delftConfigXmlParser = null;
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ConfigurationPathDoesNotExist()
        {
            delftConfigXmlParser.Read<dimrXML>(null);
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ConfigurationFilePathIsEmpty()
        {
            delftConfigXmlParser.Read<dimrXML>("");
        }

        [Test]
        public void ConfigurationFilePathIsUnknown()
        {
            var unknownFilePath = @"unknownpathtofile.xml";
            var dimrFileSource = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, unknownFilePath);
            var exception = Assert.Throws<FileNotFoundException>(() => delftConfigXmlParser.Read<dimrXML>(dimrFileSource));
            Assert.AreEqual(exception.Message, $"Configuration file {unknownFilePath} cannot be found");
        }

        #endregion

        #region Dimr tests

        [Test]
        [Category(TestCategory.DataAccess)]
        public void DimrConfigFileWithMissingDocumentationTagThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, "dimrWithMissingDocTag.xml");
            var exception = Assert.Throws<XmlException>(() => delftConfigXmlParser.Read<dimrXML>(pathWithInvalidConfigurationFile));
            Assert.AreEqual("Error during parsing : The 'documentation' start tag on line 3 position 4 does not match the end tag of 'dimrConfig'. Line 192, position 3.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithEmptyBodyThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, "invalidDimrEmptyBody.xml");
            var exception = Assert.Throws<XmlException>(() => delftConfigXmlParser.Read<dimrXML>(pathWithInvalidConfigurationFile));
            Assert.AreEqual("Error during parsing : Root element is missing.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithInvalidHeaderThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, "invalidDimrMissingHeader.xml");
            var exception = Assert.Throws<XmlException>(() => delftConfigXmlParser.Read<dimrXML>(pathWithInvalidConfigurationFile));
            Assert.AreEqual("Error during parsing : <abc xmlns='http://schemas.deltares.nl/dimr'> was not expected.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithUnknownRootName()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, "invalidDimrUnknownRootName.xml");
            var exception = Assert.Throws<XmlException>(() => delftConfigXmlParser.Read<dimrXML>(pathWithInvalidConfigurationFile));
            Assert.AreEqual("Error during parsing : <InvalidRoot xmlns='http://schemas.deltares.nl/dimr'> was not expected.", exception.Message);
        }

        #endregion

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFile()
        {
            var dimrSourcePath = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory));
            var dimrConfigurationFile = Path.Combine(dimrSourcePath, "dimr.xml");
            var dimrXmlObject = delftConfigXmlParser.Read<dimrXML>(dimrConfigurationFile);
            Assert.IsNotNull(dimrXmlObject);

            Assert.IsNotNull(dimrXmlObject.component);
            Assert.IsNotNull(dimrXmlObject.control);
            Assert.IsNotNull(dimrXmlObject.coupler);
            Assert.IsNotNull(dimrXmlObject.documentation);
            Assert.IsNull(dimrXmlObject.UnKnownAttributes);
            Assert.IsNull(dimrXmlObject.UnKnownElements);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithExtraElementsOnRootLevelInLogMessage()
        {
            // Given
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();
            SetExpectationReportedInfoMessageLogHandler(new[] {"test"});

            // When
            delftConfigXmlParser.Read<dimrXML>(dimrConfigurationFile);

            // Then
            logHandler.VerifyAllExpectations();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownAttributesOnCouplerInLogMessages()
        {
            // Given
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();
            SetExpectationReportedInfoMessageLogHandler(new[]
            {
                "Attribute",
                "abc"
            });

            // When
            delftConfigXmlParser.Read<dimrXML>(dimrConfigurationFile);

            // Then
            logHandler.VerifyAllExpectations();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownElementsOnCouplerInLogMessages()
        {
            // Given
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();
            SetExpectationReportedInfoMessageLogHandler(new[]
            {
                "Element",
                "test",
                "abc",
                "abcsourcename",
                "logger",
                "dimrwithextrainfo.xml",
                "Attribute",
            });

            // When
            delftConfigXmlParser.Read<dimrXML>(dimrConfigurationFile);

            // Then
            logHandler.VerifyAllExpectations();
        }

        #region Helper Methods

        private string DimrConfigFileWithExtraCategory()
        {
            var xmlFileDirectory = @"FileReaders\ConfigXmlReader";
            var dimrFileName = "dimrwithextrainfo.xml";
            var dimrFile = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), xmlFileDirectory, dimrFileName));

            return dimrFile;
        }

        private void SetExpectationReportedInfoMessageLogHandler(IList<string> expectedStrings)
        {
            var argExpectation = Arg<string>.Matches(arg => expectedStrings.All(arg.Contains));

            logHandler.Expect(obj => obj.ReportInfo(argExpectation))
                .Repeat.Once().Message(
                    "ReportInfo method was not called with an argument containing all of the following strings: " +
                    $"'{"\"" + string.Join("\" \"", expectedStrings) + "\""}'");
        }

        #endregion
    }
}

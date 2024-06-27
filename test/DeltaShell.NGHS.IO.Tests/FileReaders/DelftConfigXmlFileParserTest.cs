using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.IO.FileReaders;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class DelftConfigXmlFileParserTest
    {
        private string XmlFileDirectory = @"FileReaders\ConfigXmlReader";

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFile()
        {
            string dimrSourcePath = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory));
            string dimrConfigurationFile = Path.Combine(dimrSourcePath, "dimr.xml");
            var dimrXmlObject = delftConfigXmlParser.Read<dimrXML>(dimrConfigurationFile);
            Assert.IsNotNull(dimrXmlObject);

            Assert.IsNotNull(dimrXmlObject.component);
            Assert.IsNotNull(dimrXmlObject.control);
            Assert.IsNotNull(dimrXmlObject.coupler);
            Assert.IsNotNull(dimrXmlObject.documentation);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithExtraElementsOnRootLevelInLogMessage()
        {
            // Given
            string dimrConfigurationFile = DimrConfigFileWithExtraCategory();

            // When
            delftConfigXmlParser.Read<dimrXML>(dimrConfigurationFile);

            // Then
            AssertExpectationReportedInfoMessage(new[] { "test" });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownAttributesOnCouplerInLogMessages()
        {
            // Given
            string dimrConfigurationFile = DimrConfigFileWithExtraCategory();

            // When
            delftConfigXmlParser.Read<dimrXML>(dimrConfigurationFile);

            // Then
            AssertExpectationReportedInfoMessage(new[]
            {
                "Attribute",
                "abc"
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownElementsOnCouplerInLogMessages()
        {
            // Given
            string dimrConfigurationFile = DimrConfigFileWithExtraCategory();

            // When
            delftConfigXmlParser.Read<dimrXML>(dimrConfigurationFile);

            // Then
            AssertExpectationReportedInfoMessage(new[]
            {
                "Element",
                "test",
                "abc",
                "abcsourcename",
                "dimrwithextrainfo.xml",
                "Attribute"
            });
        }

        #region General Exception tests

        private ILogHandler logHandler;
        private DelftConfigXmlFileParser delftConfigXmlParser;

        [SetUp]
        public void SetUp()
        {
            logHandler = Substitute.For<ILogHandler>();
            delftConfigXmlParser = new DelftConfigXmlFileParser(logHandler);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            delftConfigXmlParser = null;
        }

        [Test]
        public void ConfigurationPathDoesNotExist()
        {
            Assert.That(() => delftConfigXmlParser.Read<dimrXML>(null), Throws.InstanceOf<FileNotFoundException>());
        }

        [Test]
        public void ConfigurationFilePathIsEmpty()
        {
            Assert.That(() => delftConfigXmlParser.Read<dimrXML>(""), Throws.InstanceOf<FileNotFoundException>());
        }

        [Test]
        public void ConfigurationFilePathIsUnknown()
        {
            var unknownFilePath = @"unknownpathtofile.xml";
            string dimrFileSource = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, unknownFilePath);
            var exception = Assert.Throws<FileNotFoundException>(() => delftConfigXmlParser.Read<dimrXML>(dimrFileSource));
            Assert.AreEqual(exception.Message, $"Configuration file {unknownFilePath} cannot be found");
        }

        #endregion

        #region Dimr tests

        [Test]
        [Category(TestCategory.DataAccess)]
        public void DimrConfigFileWithMissingDocumentationTagThrowsXmlException()
        {
            string pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, "dimrWithMissingDocTag.xml");
            var exception = Assert.Throws<XmlException>(() => delftConfigXmlParser.Read<dimrXML>(pathWithInvalidConfigurationFile));
            Assert.AreEqual("Error during parsing : The 'documentation' start tag on line 3 position 4 does not match the end tag of 'dimrConfig'. Line 192, position 3.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithEmptyBodyThrowsXmlException()
        {
            string pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, "invalidDimrEmptyBody.xml");
            var exception = Assert.Throws<XmlException>(() => delftConfigXmlParser.Read<dimrXML>(pathWithInvalidConfigurationFile));
            Assert.AreEqual("Error during parsing : Root element is missing.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithInvalidHeaderThrowsXmlException()
        {
            string pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, "invalidDimrMissingHeader.xml");
            var exception = Assert.Throws<XmlException>(() => delftConfigXmlParser.Read<dimrXML>(pathWithInvalidConfigurationFile));
            Assert.AreEqual("Error during parsing : <abc xmlns='http://schemas.deltares.nl/dimr'> was not expected.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithUnknownRootName()
        {
            string pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetTestDataDirectory(), XmlFileDirectory, "invalidDimrUnknownRootName.xml");
            var exception = Assert.Throws<XmlException>(() => delftConfigXmlParser.Read<dimrXML>(pathWithInvalidConfigurationFile));
            Assert.AreEqual("Error during parsing : <InvalidRoot xmlns='http://schemas.deltares.nl/dimr'> was not expected.", exception.Message);
        }

        #endregion

        #region Helper Methods

        private string DimrConfigFileWithExtraCategory()
        {
            var xmlFileDirectory = @"FileReaders\ConfigXmlReader";
            var dimrFileName = "dimrwithextrainfo.xml";
            string dimrFile = Path.GetFullPath(Path.Combine(TestHelper.GetTestDataDirectory(), xmlFileDirectory, dimrFileName));

            return dimrFile;
        }

        private void AssertExpectationReportedInfoMessage(IList<string> expectedStrings)
        {
            logHandler.Received().ReportInfo(Arg.Is<string>(arg => expectedStrings.All(arg.Contains)));
        }

        #endregion
    }
}
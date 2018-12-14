using System;
using System.IO;
using System.Xml;
using DelftTools.TestUtils;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileReaders;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    class DelftConfigXmlFileParserTest
    {
        private string XmlFileDirectory = @"FileReaders\ConfigXmlReader";

        #region General Exception tests
        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ConfigurationPathDoesNotExist()
        {
            DelftConfigXmlFileParser.Read<dimrXML>(null);
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ConfigurationFilePathIsEmpty()
        {
            DelftConfigXmlFileParser.Read<dimrXML>("");
        }

        [Test]
        public void ConfigurationFilePathIsUnknown()
        {
            var unknownFilePath = @"unknowpathtofile.xml";
            var dimrFileSource = Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory, unknownFilePath);
            
            var exception = Assert.Throws<FileNotFoundException>(() => { DelftConfigXmlFileParser.Read<dimrXML>(dimrFileSource); });
            Assert.AreEqual(exception.Message, $"Configuration file {unknownFilePath} cannot be found");
        }
        #endregion

        #region Dimr tests
        [Test]
        [Category(TestCategory.DataAccess)]
        public void DimrConfigFileWithMissingDocumentationTagThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory, "dimrWithMissingDocTag.xml");
            
            var exception = Assert.Throws<XmlException>(() => { DelftConfigXmlFileParser.Read<dimrXML>(pathWithInvalidConfigurationFile); });
            Assert.AreEqual("Error during parsing : The 'documentation' start tag on line 3 position 4 does not match the end tag of 'dimrConfig'. Line 192, position 3.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithEmptyBodyThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory, "invalidDimrEmptyBody.xml");
            var exception = Assert.Throws<XmlException>(() => { DelftConfigXmlFileParser.Read<dimrXML>(pathWithInvalidConfigurationFile); });
            Assert.AreEqual("Error during parsing : Root element is missing.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithInvalidHeaderThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory, "invalidDimrMissingHeader.xml");

            var exception = Assert.Throws<XmlException>(() => { DelftConfigXmlFileParser.Read<dimrXML>(pathWithInvalidConfigurationFile); });
            Assert.AreEqual("Error during parsing : <abc xmlns='http://schemas.deltares.nl/dimr'> was not expected.", exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void InvalidDimrConfigurationFileWithUnknownRootName()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory, "invalidDimrUnknownRootName.xml");

            var exception = Assert.Throws<XmlException>(() => { DelftConfigXmlFileParser.Read<dimrXML>(pathWithInvalidConfigurationFile); });
            Assert.AreEqual("Error during parsing : <InvalidRoot xmlns='http://schemas.deltares.nl/dimr'> was not expected.", exception.Message);
        }
        #endregion

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFile()
        {
            var dimrSourcePath = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory));
            var dimrConfigurationFile = Path.Combine(dimrSourcePath, "dimr.xml");
            var dimrXmlObject = DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile);
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
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "test");

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownAttributesOnCouplerInLogMessages()
        {
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "Attribute");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "abc");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownElementsOnCouplerInLogMessages()
        {
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "Element");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "test");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "abc");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "abcsourcename");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "logger");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read<dimrXML>(dimrConfigurationFile), "dimrwithextrainfo.xml");
        }

        #region Helper Methods
        private string DimrConfigFileWithExtraCategory()
        {
            var xmlFileDirectory = @"FileReaders\ConfigXmlReader";
            var dimrFileName = "dimrwithextrainfo.xml";
            var dimrFile = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), xmlFileDirectory, dimrFileName));

            return dimrFile;
        }
        #endregion
    }
}

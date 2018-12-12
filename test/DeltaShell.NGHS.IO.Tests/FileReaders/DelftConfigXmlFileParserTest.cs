using System.IO;
using System.Linq;
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
        private const string UnableToReadTheFileDueToInvalidFileFormat = "Unable to read the file due to invalid file format";
        private string dimrSourcePath;
        private string XmlFileDirectory = @"FileReaders\ConfigXmlReader";

        [SetUp]
        public void Setup()
        {
            dimrSourcePath = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory));
        }

        #region General Exception tests
        [Test]
        [ExpectedException(typeof(FileReadingException))]
        public void ConfigurationPathDoesNotExist()
        {
            DelftConfigXmlFileParser.Read(null);
        }

        [Test]
        [ExpectedException(typeof(FileReadingException))]
        public void ConfigurationFilePathIsEmpty()
        {
            DelftConfigXmlFileParser.Read("");
        }

        [Test]
        [ExpectedException(typeof(XmlException), ExpectedMessage = UnableToReadTheFileDueToInvalidFileFormat)]
        public void ConfigurationFilePathIsUnknown()
        {
            var unknownFilePath = @"\unknowpathtofile";
            var dimrFileSource = Path.Combine(unknownFilePath, dimrSourcePath);
            DelftConfigXmlFileParser.Read(dimrFileSource);
        }
        #endregion

        #region Dimr tests
        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(XmlException), ExpectedMessage = UnableToReadTheFileDueToInvalidFileFormat)]
        public void DimrConfigFileWithMissingDocumentationTagThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(dimrSourcePath, "dimrWithMissingDocTag.xml");
            DelftConfigXmlFileParser.Read(pathWithInvalidConfigurationFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(XmlException), ExpectedMessage = UnableToReadTheFileDueToInvalidFileFormat)]
        public void InvalidDimrConfigurationFileWithEmptyBodyThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), "invalidDimrEmptyBody.xml"));
            DelftConfigXmlFileParser.Read(pathWithInvalidConfigurationFile);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(XmlException), ExpectedMessage = UnableToReadTheFileDueToInvalidFileFormat)]
        public void InvalidDimrConfigurationFileWithInvalidHeaderThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), "invalidDimrMissingHeader.xml"));
            DelftConfigXmlFileParser.Read(pathWithInvalidConfigurationFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(XmlException), ExpectedMessage = UnableToReadTheFileDueToInvalidFileFormat)]
        public void InvalidDimrConfigurationFileWithUnknownRootName()
        {
            var pathWithInvalidConfigurationFile = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), "invalidDimrUnknownRootName.xml"));
            DelftConfigXmlFileParser.Read(pathWithInvalidConfigurationFile);
        }
        #endregion

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFile()
        {
            var dimrConfigurationFile = Path.Combine(dimrSourcePath, "dimr.xml");
            var dataAccesModel = DelftConfigXmlFileParser.Read(dimrConfigurationFile);
            Assert.IsNotNull(dataAccesModel);

            var dimrXmlObject = (dimrXML)dataAccesModel;
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

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "test");

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownAttributesOnCouplerInLogMessages()
        {
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "Attribute");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "abc");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownElementsOnCouplerInLogMessages()
        {
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "Element");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "test");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "abc");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "abcsourcename");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "logger");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "dimrwithextrainfo.xml");
        }

        #region RTC tests

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingStateImportXmlFilesDoesNotThrow()
        {
            var fileName = "state_import.xml";

            var directory = @"FileReaders\ConfigXmlReader\rtc";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), fileName);

            var treeVectorFile = dataAccesModel as TreeVectorFileXML;

            Assert.NotNull(treeVectorFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingTimeSeriesImportXmlFilesDoesNotThrown()
        {
            var fileName = "timeseries_import.xml";

            var directory = @"FileReaders\ConfigXmlReader\rtc";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), fileName);

            var timeSeriesCollection = dataAccesModel as TimeSeriesCollectionComplexType;

            Assert.NotNull(timeSeriesCollection);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingToolsConfigXmlFilesDoesNotThrow()
        {
            var fileName = "rtcToolsConfig.xml";

            var directory = @"FileReaders\ConfigXmlReader\rtc";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), fileName);

            var rtcToolsConfig = dataAccesModel as RtcToolsConfigXML;

            Assert.NotNull(rtcToolsConfig);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingRuntimeConfigXmlFilesDoesNotThrow()
        {
            var fileName = "rtcRuntimeConfig.xml";

            var directory = @"FileReaders\ConfigXmlReader\rtc";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), fileName);

            var rtcRuntimeConfig = dataAccesModel as RtcRuntimeConfigXML;

            Assert.NotNull(rtcRuntimeConfig);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingDataConfigXmlFilesDoesNotThrow()
        {       
            var fileName = "rtcDataConfig.xml";

            var directory = @"FileReaders\ConfigXmlReader\rtc";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), "Attribute: \"xsi:schemaLocation\"");
            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(path), fileName);

            var rtcDataConfig = (RTCDataConfigXML)dataAccesModel;

            Assert.NotNull(rtcDataConfig);
        }
        #endregion

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

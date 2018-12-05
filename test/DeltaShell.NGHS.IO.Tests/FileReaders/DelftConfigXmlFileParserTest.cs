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
        public void GetDimrConfigurationFileWithExtraElementsOnRootLevel()
        {
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();
            var dataAccesModel = DelftConfigXmlFileParser.Read(dimrConfigurationFile);

            Assert.IsNotNull(dataAccesModel);

            var dimrXmlObject = (dimrXML)dataAccesModel;

            Assert.AreEqual(dimrXmlObject.UnKnownElements.Count, 1);

            //Root level
            Assert.AreEqual(dimrXmlObject.UnKnownElements.ElementAt(0).Name, "test");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownAttributesOnCoupler()
        {
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();
            var dataAccesModel = DelftConfigXmlFileParser.Read(dimrConfigurationFile);

            Assert.IsNotNull(dataAccesModel);
            var dimrXmlObject = (dimrXML)dataAccesModel;


            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "Attribute");

            //Coupler
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(0).UnKnownAttributes.ElementAt(0).Name, "abc");
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(0).UnKnownAttributes.ElementAt(0).Value, "2");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownElementsOnCoupler()
        {
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();
            var dataAccesModel = DelftConfigXmlFileParser.Read(dimrConfigurationFile);

            Assert.IsNotNull(dataAccesModel);

            var dimrXmlObject = (dimrXML)dataAccesModel;

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DelftConfigXmlFileParser.Read(dimrConfigurationFile), "Element");

            //logger
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(0).UnKnownElements.ElementAt(0).Name, "logger");
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(1).UnKnownElements.ElementAt(0).Name, "logger");
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(0).item.ElementAt(0).UnKnownElements.ElementAt(0).Name, "abcsourcename");
        }

        #region RTC tests

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingStateImportXmlFilesDoesNotThrow()
        {
            var fileName = "state_import.xml";

            var directory = @"FileReaders\ConfigXmlReader\RtcXmlFiles";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            var treeVectorFile = dataAccesModel as TreeVectorFileXML;

            Assert.NotNull(treeVectorFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingTimeSeriesImportXmlFilesDoesNotThrown()
        {
            var fileName = "timeseries_import.xml";

            var directory = @"FileReaders\ConfigXmlReader\RtcXmlFiles";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            var timeSeriesCollection = dataAccesModel as TimeSeriesCollectionComplexType;

            Assert.NotNull(timeSeriesCollection);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingToolsConfigXmlFilesDoesNotThrow()
        {
            var fileName = "rtcToolsConfig.xml";

            var directory = @"FileReaders\ConfigXmlReader\RtcXmlFiles";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            var rtcToolsConfig = dataAccesModel as RtcToolsConfigXML;

            Assert.NotNull(rtcToolsConfig);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingRuntimeConfigXmlFilesDoesNotThrow()
        {
            var fileName = "rtcRuntimeConfig.xml";

            var directory = @"FileReaders\ConfigXmlReader\RtcXmlFiles";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

            var rtcRuntimeConfig = dataAccesModel as RtcRuntimeConfigXML;

            Assert.NotNull(rtcRuntimeConfig);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingDataConfigXmlFilesDoesNotThrow()
        {       
            var fileName = "rtcDataConfig.xml";

            var directory = @"FileReaders\ConfigXmlReader\RtcXmlFiles";
            var path = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), directory, fileName));

            Assert.True(File.Exists(path));

            var dataAccesModel = DelftConfigXmlFileParser.Read(path);

            Assert.IsNotNull(dataAccesModel);

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

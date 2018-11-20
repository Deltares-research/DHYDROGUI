using System.IO;
using System.Linq;
using System.Xml;
using DelftTools.TestUtils;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.ConfigXml;
using NUnit.Framework;
using QuickGraph;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    class DelftConfigXmlFileReaderTest
    {
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
            DelftConfigXmlFileReader.Read(null);
        }

        [Test]
        [ExpectedException(typeof(FileReadingException))]
        public void ConfigurationFilePathIsEmpty()
        {
            DelftConfigXmlFileReader.Read("");
        }

        [Test]
        [ExpectedException(typeof(XmlException), ExpectedMessage = "Unable to parse file")]
        public void ConfigurationFilePathIsUnknown()
        {
            var unknownFilePath = @"\unknowpathtofile";
            var dimrFileSource = Path.Combine(unknownFilePath, dimrSourcePath);
            DelftConfigXmlFileReader.Read(dimrFileSource);
        }
        #endregion

        #region Dimr tests
        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(XmlException), ExpectedMessage = "Unable to parse file")]
        public void DimrConfigFileWithMissingDocumentationTagThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.Combine(dimrSourcePath, "dimrWithMissingDocTag.xml");
            DelftConfigXmlFileReader.Read(pathWithInvalidConfigurationFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(XmlException), ExpectedMessage = "Unable to parse file")]
        public void InvalidDimrConfigurationFileWithEmptyBodyThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), "invalidDimrEmptyBody.xml"));
            DelftConfigXmlFileReader.Read(pathWithInvalidConfigurationFile);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(XmlException), ExpectedMessage = "Unable to parse file")]
        public void InvalidDimrConfigurationFileWithInvalidHeaderThrowsXmlException()
        {
            var pathWithInvalidConfigurationFile = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), "invalidDimrMissingHeader.xml"));
            DelftConfigXmlFileReader.Read(pathWithInvalidConfigurationFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(XmlException), ExpectedMessage = "Unable to parse file")]
        public void InvalidDimrConfigurationFileWithUnknownRootName()
        {
            var pathWithInvalidConfigurationFile = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), "invalidDimrUnknownRootName.xml"));
            DelftConfigXmlFileReader.Read(pathWithInvalidConfigurationFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFile()
        {


            var dimrConfigurationFile = Path.Combine(dimrSourcePath, "dimr.xml");
            var dataAccesModel = DelftConfigXmlFileReader.Read(dimrConfigurationFile);
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
            var dataAccesModel = DelftConfigXmlFileReader.Read(dimrConfigurationFile);

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
            var dataAccesModel = DelftConfigXmlFileReader.Read(dimrConfigurationFile);

            Assert.IsNotNull(dataAccesModel);
            var dimrXmlObject = (dimrXML)dataAccesModel;
           
            //Coupler
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(0).UnKnownAttributes.ElementAt(0).Name, "abc");
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(0).UnKnownAttributes.ElementAt(0).Value, "2");

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDimrConfigurationFileWithUnknownElementsOnCoupler()
        {
            var dimrConfigurationFile = DimrConfigFileWithExtraCategory();
            var dataAccesModel = DelftConfigXmlFileReader.Read(dimrConfigurationFile);

            Assert.IsNotNull(dataAccesModel);

            var dimrXmlObject = (dimrXML)dataAccesModel;

            //logger
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(0).UnKnownElements.ElementAt(0).Name, "logger");
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(1).UnKnownElements.ElementAt(0).Name, "logger");
            Assert.AreEqual(dimrXmlObject.coupler.ElementAt(0).item.ElementAt(0).UnKnownElements.ElementAt(0).Name, "abcsourcename");
        }
        #endregion


        #region RTC tests
        //TODO: RTC tests (use filesnames and sourcespath below for the rtc tests)
        //"rtcDataConfig.xml";
        //"rtcRuntimeConfig.xml";
        //"rtcToolsConfig.xml";
        //"state_import.xml";
        //"timeseries_import.xml";
        //rtcSourcePath = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory, "rtc"));
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

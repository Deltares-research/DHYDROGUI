using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.TestUtils;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileConverters;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class DelftXmlFileConverterTest
    {
        [Test]
        public void ConvertDimr()
        {
            var xmlFilePath = GetXmlFilePath();
            var xmlConfigFile = XDocument.Load(Path.Combine(xmlFilePath, "dimr.xml"));
            var reader = xmlConfigFile.CreateReader();

            var result = DelftXmlFileConverter.Convert(reader, "dimrConfig", new List<string>());
            Assert.IsNotNull(result);

            var dimrXML = (dimrXML) result;
            Assert.IsNotNull(dimrXML.component);
            Assert.IsNotNull(dimrXML.control);
            Assert.IsNotNull(dimrXML.coupler);
            Assert.IsNotNull(dimrXML.documentation);
            Assert.IsNull(dimrXML.UnKnownAttributes);
            Assert.IsNull(dimrXML.UnKnownElements);
        }

        [Test]
        public void ConvertDimrWithExtraElementsOnRootLevel()
        {
            var xmlFilePath = GetXmlFilePath();
            var xmlConfigFile = XDocument.Load(Path.Combine(xmlFilePath, "dimr.xml"));
            var reader = xmlConfigFile.CreateReader();

            var result = DelftXmlFileConverter.Convert(reader, "dimrConfig", new List<string>());
            Assert.IsNotNull(result);

            var dimrXml = (dimrXML) result;
            Assert.AreEqual(dimrXml.UnKnownElements.Count, 1);

            //Root level
            Assert.AreEqual(dimrXml.UnKnownElements.ElementAt(0).Name, "test");
        }

        //todo: RTC converting tests

        public string GetXmlFilePath()
        {
            string XmlFileDirectory = @"FileReaders\ConfigXmlReader";
            var xmlFilePath = Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), XmlFileDirectory));

            return xmlFilePath;
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

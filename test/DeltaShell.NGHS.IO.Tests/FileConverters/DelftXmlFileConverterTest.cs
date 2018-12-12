using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DelftTools.TestUtils;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileConverters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.FileConverters
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

            var dimrXml = DelftXmlFileConverter.Convert<dimrXML>(reader, new List<string>());
            Assert.IsNotNull(dimrXml);
            
            Assert.IsNotNull(dimrXml.component);
            Assert.IsNotNull(dimrXml.control);
            Assert.IsNotNull(dimrXml.coupler);
            Assert.IsNotNull(dimrXml.documentation);
            Assert.IsNull(dimrXml.UnKnownAttributes);
            Assert.IsNull(dimrXml.UnKnownElements);
        }

        [Test]
        public void ConvertDimrWithExtraElementsOnRootLevel()
        {
            var xmlFilePath = GetXmlFilePath();
            var xmlConfigFile = XDocument.Load(Path.Combine(xmlFilePath, "dimrwithextrainfo.xml"));
            var reader = xmlConfigFile.CreateReader();

            var dimrXml = DelftXmlFileConverter.Convert<dimrXML>(reader, new List<string>());
            Assert.IsNotNull(dimrXml);

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
        
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Reader cannot be null")]
        public void NoFileShouldThrowException()
        {
            DelftXmlFileConverter.Convert<dimrXML>(null, new List<string>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Unsupported Features cannot be null")]
        public void NoUnsupportedFeaturesShouldThrowException()
        {
            var xmlReader = MockRepository.GenerateStrictMock<XmlReader>();
            DelftXmlFileConverter.Convert<dimrXML>(xmlReader, null);
        }

        [Test]
        public void ValidateDimrFileWithNoAdditionalInformation()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "ConfigXmlReader", "dimr.xml"));
            var dimrXml = XDocument.Load(dimrPath);
            var reader = dimrXml?.Root?.CreateReader();
            var unsupportedFeatures = new List<string>();

            DelftXmlFileConverter.Convert<dimrXML>(reader, unsupportedFeatures);

            Assert.That(unsupportedFeatures.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateDimrFileWithUnsupportedElementsAndAttribute()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "ConfigXmlReader", "dimrwithextrainfo.xml"));
            var reader = XDocument.Load(dimrPath)?.Root?.CreateReader();

            var unsupportedFeatures = new List<string>();
            DelftXmlFileConverter.Convert<dimrXML>(reader, unsupportedFeatures);

            Assert.That(unsupportedFeatures.Count, Is.EqualTo(5));
            Assert.That(unsupportedFeatures.ElementAt(0), Is.EqualTo("Element: test"));
            Assert.That(unsupportedFeatures.ElementAt(1), Is.EqualTo("Attribute: abc"));
            Assert.That(unsupportedFeatures.ElementAt(2), Is.EqualTo("Element: abcsourcename"));
            Assert.That(unsupportedFeatures.ElementAt(3), Is.EqualTo("Element: logger"));
            Assert.That(unsupportedFeatures.ElementAt(4), Is.EqualTo("Element: logger"));
        }
    }
}

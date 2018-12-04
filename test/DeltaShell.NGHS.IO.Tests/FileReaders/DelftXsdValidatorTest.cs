using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using DelftTools.TestUtils;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileConverters;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class DelftXsdValidatorTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Serializer cannot be null")]
        public void IfNoSerializerIsFoundThrowException()
        {
            var unsupportedFeatures = new List<string>();

            DelftXsdValidator.CollectUnsupportedFeatures(null, unsupportedFeatures);

            Assert.That(unsupportedFeatures.Count, Is.EqualTo(0));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Unsupported features collection cannot be null")]
        public void IfNoUnsupportedFeaturesCollectionIsFoundThrowException()
        {
            DelftXsdValidator.CollectUnsupportedFeatures(new XmlSerializer(typeof(dimrXML)), null);
        }

        [Test]
        public void ValidateDimrFileWithNoAdditionalInformation()
        {
            var reader = CreateDimrXmlReader();
            var unsupportedFeatures = new List<string>();
           
            var dimrSerializer = new XmlSerializer(typeof(dimrXML));

            DelftXsdValidator.CollectUnsupportedFeatures(dimrSerializer, unsupportedFeatures);
            if (reader != null) dimrSerializer.Deserialize(reader);

            Assert.That(unsupportedFeatures.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateDimrFileWithUnsupportedElementsAndAttribute()
        {
            var reader = CreateDimrXmlReaderWithFaultyFormat();
            var unsupportedFeatures = new List<string>();
           
            var dimrSerializer = new XmlSerializer(typeof(dimrXML));

            DelftXsdValidator.CollectUnsupportedFeatures(dimrSerializer, unsupportedFeatures);
            if (reader != null) dimrSerializer.Deserialize(reader);

            Assert.That(unsupportedFeatures.Count, Is.EqualTo(5));
            Assert.That(unsupportedFeatures.ElementAt(0), Is.EqualTo("Element: test"));
            Assert.That(unsupportedFeatures.ElementAt(1), Is.EqualTo("Attribute: abc"));
            Assert.That(unsupportedFeatures.ElementAt(2), Is.EqualTo("Element: abcsourcename"));
            Assert.That(unsupportedFeatures.ElementAt(3), Is.EqualTo("Element: logger"));
            Assert.That(unsupportedFeatures.ElementAt(4), Is.EqualTo("Element: logger"));
        }

        private static XmlReader CreateDimrXmlReader()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "ConfigXmlReader", "dimr.xml"));
            var dimrXml = XDocument.Load(dimrPath);
            var reader = dimrXml?.Root?.CreateReader();
            return reader;
        }

        private static XmlReader CreateDimrXmlReaderWithFaultyFormat()
        {
            var dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "ConfigXmlReader", "dimrwithextrainfo.xml"));
            var dimrXml = XDocument.Load(dimrPath);
            var reader = dimrXml?.Root?.CreateReader();
            return reader;
        }

    }
}

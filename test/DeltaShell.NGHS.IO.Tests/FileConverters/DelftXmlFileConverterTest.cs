using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        [Category(TestCategory.DataAccess)]
        public void ConvertDimr()
        {
            string xmlFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\ConfigXmlReader", "dimr.xml");
            using (StreamReader reader = File.OpenText(xmlFilePath))
            {
                var dimrXml = DelftXmlFileConverter.Convert<dimrXML>(reader, new List<string>());
                Assert.IsNotNull(dimrXml);

                Assert.IsNotNull(dimrXml.component);
                Assert.IsNotNull(dimrXml.control);
                Assert.IsNotNull(dimrXml.coupler);
                Assert.IsNotNull(dimrXml.documentation);
                Assert.IsNull(dimrXml.UnKnownAttributes);
                Assert.IsNull(dimrXml.UnKnownElements);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConvertDimrWithExtraElementsOnRootLevel()
        {
            string xmlFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\ConfigXmlReader", "dimrwithextrainfo.xml");
            using (StreamReader reader = File.OpenText(xmlFilePath))
            {
                var dimrXml = DelftXmlFileConverter.Convert<dimrXML>(reader, new List<string>());
                Assert.IsNotNull(dimrXml);

                Assert.AreEqual(dimrXml.UnKnownElements.Count, 1);

                //Root level
                Assert.AreEqual(dimrXml.UnKnownElements.ElementAt(0).Name, "test");
            }
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
            var xmlReader = MockRepository.GenerateStrictMock<StreamReader>();
            DelftXmlFileConverter.Convert<dimrXML>(xmlReader, null);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ValidateDimrFileWithNoAdditionalInformation()
        {
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "ConfigXmlReader", "dimr.xml"));
            using (StreamReader reader = File.OpenText(dimrPath))
            {
                var unsupportedFeatures = new List<string>();

                DelftXmlFileConverter.Convert<dimrXML>(reader, unsupportedFeatures);

                Assert.That(unsupportedFeatures.Count, Is.EqualTo(2));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ValidateDimrFileWithUnsupportedElementsAndAttribute()
        {
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "ConfigXmlReader", "dimrwithextrainfo.xml"));
            using (StreamReader reader = File.OpenText(dimrPath))
            {
                var unsupportedFeatures = new List<string>();
                DelftXmlFileConverter.Convert<dimrXML>(reader, unsupportedFeatures);

                Assert.That(unsupportedFeatures.Count, Is.EqualTo(5));
                Assert.That(unsupportedFeatures.ElementAt(0), Is.EqualTo("Element: \"test\" at line 45 position 4"));
                Assert.That(unsupportedFeatures.ElementAt(1), Is.EqualTo("Attribute: \"abc\" at line 55 position 17"));
                Assert.That(unsupportedFeatures.ElementAt(2), Is.EqualTo("Element: \"abcsourcename\" at line 60 position 8"));
                Assert.That(unsupportedFeatures.ElementAt(3), Is.EqualTo("Element: \"logger\" at line 126 position 5"));
                Assert.That(unsupportedFeatures.ElementAt(4), Is.EqualTo("Element: \"logger\" at line 194 position 5"));
            }
        }
    }
}
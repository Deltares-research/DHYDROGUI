using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Dimr.DimrXsd;
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
            }
        }

        [Test]
        public void NoFileShouldThrowException()
        {
            Assert.That(() => DelftXmlFileConverter.Convert<dimrXML>(null, new List<string>()), 
                        Throws.ArgumentException.With.Message.EqualTo("Reader cannot be null"));
        }

        [Test]
        public void NoUnsupportedFeaturesShouldThrowException()
        {
            var xmlReader = MockRepository.GenerateStrictMock<StreamReader>();
            Assert.That(() => DelftXmlFileConverter.Convert<dimrXML>(xmlReader, null), 
                        Throws.ArgumentException.With.Message.EqualTo("Unsupported Features cannot be null"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ValidateDimrFileWithNoUnsupportedElementsAndAttributes()
        {
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "ConfigXmlReader", "dimr.xml"));
            using (StreamReader reader = File.OpenText(dimrPath))
            {
                var unsupportedFeatures = new List<string>();

                DelftXmlFileConverter.Convert<dimrXML>(reader, unsupportedFeatures);

                Assert.That(unsupportedFeatures.Count, Is.EqualTo(0));
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

                Assert.That(unsupportedFeatures.Count, Is.EqualTo(3));
                Assert.That(unsupportedFeatures.ElementAt(0), Is.EqualTo("Element: \"test\" at line 45 position 4"));
                Assert.That(unsupportedFeatures.ElementAt(1), Is.EqualTo("Attribute: \"abc\" at line 55 position 17"));
                Assert.That(unsupportedFeatures.ElementAt(2), Is.EqualTo("Element: \"abcsourcename\" at line 60 position 8"));
            }
        }
    }
}
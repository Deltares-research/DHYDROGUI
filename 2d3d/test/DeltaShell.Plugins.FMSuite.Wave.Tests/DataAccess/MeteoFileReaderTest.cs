using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class MeteoFileReaderTest
    {
        private readonly string testDataPath = TestHelper.GetTestFilePath(nameof(MeteoFileReaderTest));

        [Test]
        public void Read_WithoutFilePath_ThrowsArgumentNullException()
        {
            void Call() => new MeteoFileReader().Read(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("filePath"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenMeteoFile_WhenRead_ThenExpectedPropertiesReturned()
        {
            string testFile = Path.Combine(testDataPath, "test.wnd");

            IEnumerable<MeteoFileProperty> properties = new MeteoFileReader().Read(testFile).ToArray();

            Assert.That(properties.Count(), Is.EqualTo(4));
            AssertMeteoFileProperty(properties.ElementAt(0), "FileVersion", "1.03");
            AssertMeteoFileProperty(properties.ElementAt(1), "dx", "2.44E+04");
            AssertMeteoFileProperty(properties.ElementAt(2), "quantity1", "y_wind");
            AssertMeteoFileProperty(properties.ElementAt(3), "unit1", "m s-1");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenMeteoFileWithInvalidLines_WhenRead_ThenExpectedPropertiesReturned()
        {
            string testFile = Path.Combine(testDataPath, "test_with_invalid_lines.wnd");

            IEnumerable<MeteoFileProperty> properties = new MeteoFileReader().Read(testFile).ToArray();

            Assert.That(properties.Count(), Is.EqualTo(1));
            AssertMeteoFileProperty(properties.ElementAt(0), "FileVersion", "1.03");
        }

        private static void AssertMeteoFileProperty(MeteoFileProperty meteoFileProperty, string expectedProperty, string expectedValue)
        {
            Assert.That(meteoFileProperty.Property, Is.EqualTo(expectedProperty));
            Assert.That(meteoFileProperty.Value, Is.EqualTo(expectedValue));
        }
    }
}
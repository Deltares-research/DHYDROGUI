using System;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class MeteoFilePropertyTest
    {
        [Test]
        public void Constructor_WithoutProperty_ThrowsArgumentNullException()
        {
            void Call() => new MeteoFileProperty(null, "value");

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void Constructor_WithoutValue_ThrowsArgumentNullException()
        {
            void Call() => new MeteoFileProperty("property", null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("value"));
        }

        [Test]
        public void GivenValidParameters_WhenCreatingMeteoFileProperty_ThenPropertiesSet()
        {
            var meteoFileProperty = new MeteoFileProperty("property name", "value 1");

            Assert.That(meteoFileProperty.Property, Is.EqualTo("property name"));
            Assert.That(meteoFileProperty.Value, Is.EqualTo("value 1"));
        }
    }
}
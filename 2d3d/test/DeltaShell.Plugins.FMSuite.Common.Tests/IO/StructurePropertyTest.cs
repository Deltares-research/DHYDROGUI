using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructurePropertyTest
    {
        [Test]
        public void Constructor()
        {
            var definition = new StructurePropertyDefinition { DataType = typeof(string) };
            var value = "some_property_value";
            var property = new StructureProperty(definition, value);

            Assert.That(property.PropertyDefinition, Is.SameAs(definition));
            Assert.That(property.Value, Is.SameAs(value));
        }

        [Test]
        public void Clone_ReturnsCorrectInstance()
        {
            var definition = new StructurePropertyDefinition { DataType = typeof(string) };
            var property = new StructureProperty(definition, "some_property_value") { LineNumber = 7 };

            var clone = (StructureProperty)property.Clone();

            Assert.That(clone, Is.Not.SameAs(property));
            Assert.That(clone.PropertyDefinition, Is.SameAs(property.PropertyDefinition));
            Assert.That(clone.Value, Is.SameAs(property.Value));
            Assert.That(clone.LineNumber, Is.EqualTo(property.LineNumber));
        }
    }
}
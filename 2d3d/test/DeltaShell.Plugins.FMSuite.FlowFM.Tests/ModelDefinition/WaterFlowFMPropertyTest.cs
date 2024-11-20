using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelDefinition
{
    [TestFixture]
    public class WaterFlowFMPropertyTest
    {
        [Test]
        public void Constructor()
        {
            var definition = new WaterFlowFMPropertyDefinition { DataType = typeof(string) };
            var value = "some_property_value";
            var property = new WaterFlowFMProperty(definition, value);

            Assert.That(property.PropertyDefinition, Is.SameAs(definition));
            Assert.That(property.Value, Is.SameAs(value));
        }

        [Test]
        public void Clone_ReturnsCorrectInstance()
        {
            var definition = new WaterFlowFMPropertyDefinition { DataType = typeof(string) };
            var property = new WaterFlowFMProperty(definition, "some_property_value") { LineNumber = 7 };

            var clone = (WaterFlowFMProperty)property.Clone();

            Assert.That(clone, Is.Not.SameAs(property));
            Assert.That(clone.PropertyDefinition, Is.SameAs(property.PropertyDefinition));
            Assert.That(clone.Value, Is.SameAs(property.Value));
            Assert.That(clone.LineNumber, Is.EqualTo(property.LineNumber));
        }
    }
}
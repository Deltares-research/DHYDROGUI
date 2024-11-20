using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Utils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects.StructureObjects.StructureFormulas
{
    [TestFixture]
    public class GateOpeningDirectionTest
    {
        [Test]
        [TestCase(GateOpeningDirection.FromLeft, "From left")]
        [TestCase(GateOpeningDirection.FromRight, "From right")]
        [TestCase(GateOpeningDirection.Symmetric, "Symmetric")]
        public void GateOpeningDirection_HasExpectedDescription(GateOpeningDirection value, string expectedString)
        {
            // Setup
            var converter = new EnumDescriptionAttributeTypeConverter(typeof(GateOpeningDirection));

            // Call
            string description = converter.ConvertToInvariantString(value);

            // Assert
            Assert.That(description, Is.EqualTo(expectedString));
        }
    }
}
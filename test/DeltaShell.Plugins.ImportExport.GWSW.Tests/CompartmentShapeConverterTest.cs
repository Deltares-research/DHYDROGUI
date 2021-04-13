using DelftTools.Hydro;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class CompartmentShapeConverterTest
    {

        [Test]
        [TestCase(null, CompartmentShape.Unknown)]
        [TestCase("unkNoWn", CompartmentShape.Unknown)]
        [TestCase("rnd", CompartmentShape.Round)]
        [TestCase("rOund", CompartmentShape.Round)]
        [TestCase("RHK", CompartmentShape.Rectangular)]
        [TestCase("rectangular", CompartmentShape.Rectangular)]
        public void GivenCompartmentShapeString_WhenCallingWaterTypeConverter_ThenReturnsCorrectCompartmentShapeType(
            string compartmentShapeString, CompartmentShape expectedCompartmentShape)
        {
            var actualCompartmentShape = CompartmentShapeConverter.ConvertStringToCompartmentShape(compartmentShapeString);
            Assert.That(actualCompartmentShape, Is.EqualTo(expectedCompartmentShape));
        }

        [Test]
        public void GivenInvalidCompartmentShapeString_WhenCallingCompartmentShapeConverter_ThenAddsMessageToLogAndSetsCompartmentToUnknown()
        {
            var invalidCompartmentShapeString = "InvalidCompartmentShape";
            var expectedCompartmentShape = CompartmentShape.Unknown;
            var expectedMessage = $"Shape {invalidCompartmentShapeString} is not a valid shape. Setting the shape to 'unknown'";

            CompartmentShape actualCompartmentShape = CompartmentShapeConverter.ConvertStringToCompartmentShape(invalidCompartmentShapeString);

            Assert.That(actualCompartmentShape, Is.EqualTo(expectedCompartmentShape));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => actualCompartmentShape = CompartmentShapeConverter.ConvertStringToCompartmentShape(invalidCompartmentShapeString), expectedMessage);
            
        }
    }
}
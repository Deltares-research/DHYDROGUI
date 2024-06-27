using DelftTools.Hydro;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using NSubstitute;
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
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var actualCompartmentShape = CompartmentShapeConverter.ConvertStringToCompartmentShape(compartmentShapeString, logHandler);
            Assert.That(actualCompartmentShape, Is.EqualTo(expectedCompartmentShape));
        }

        [Test]
        public void GivenInvalidCompartmentShapeString_WhenCallingCompartmentShapeConverter_ThenAddsMessageToLogAndSetsCompartmentToUnknown()
        {
            var invalidCompartmentShapeString = "InvalidCompartmentShape";
            var expectedCompartmentShape = CompartmentShape.Unknown;
            var expectedMessage = $"Shape {invalidCompartmentShapeString} is not a valid shape. Setting the shape to 'unknown'";
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            CompartmentShape actualCompartmentShape = CompartmentShapeConverter.ConvertStringToCompartmentShape(invalidCompartmentShapeString, logHandler);

            Assert.That(actualCompartmentShape, Is.EqualTo(expectedCompartmentShape));
            logHandler.Received().ReportWarningFormat(GWSW.Properties.Resources.Shape__0__is_not_a_valid_shape_Setting_shape_to_unknown, invalidCompartmentShapeString);
        }
    }
}
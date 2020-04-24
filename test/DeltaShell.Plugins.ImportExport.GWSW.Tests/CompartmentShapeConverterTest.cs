using DelftTools.Hydro;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class CompartmentShapeConverterTest
    {

        [Test]
        [TestCase("unkNoWn", CompartmentShape.Unknown)]
        [TestCase("rnd", CompartmentShape.Square)]
        [TestCase("RHK", CompartmentShape.Rectangular)]
        public void GivenCompartmentShapeString_WhenCallingWaterTypeConverter_ThenReturnsCorrectCompartmentShapeType(
            string compartmentShapeString, CompartmentShape expectedCompartmentShape)
        {
            var actualCompartmentShapeType = CompartmentShapeConverter.ConvertStringToCompartmentShape(compartmentShapeString);
            Assert.That(actualCompartmentShapeType, Is.EqualTo(expectedCompartmentShape));
        }
    }
}
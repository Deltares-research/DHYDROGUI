using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts
{
    [TestFixture]
    public class GreenhouseDataTest
    {
        [Test]
        public void Constructor_SetsCatchmentModelDataOnCatchment()
        {
            // Setup
            var catchment = new Catchment();

            // Call
            var data = new GreenhouseData(catchment);

            // Assert
            Assert.That(catchment.ModelData, Is.SameAs(data));
        }
        
        [Test]
        public void GivenGreenhouseData_ChangingAreaSum_ShouldChangeDependentGeometry()
        {
            //Arrange
            var greenHouseCatchment = new Catchment
            {
                CatchmentType = CatchmentType.GreenHouse,
            };
            var greenhouseData = new GreenhouseData(greenHouseCatchment);

            // check if no geometry is set
            Assert.IsNull(greenHouseCatchment.Geometry);
            Assert.IsTrue(greenHouseCatchment.IsGeometryDerivedFromAreaSize);

            // Act & Assert
            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from3000to4000] = 2000;
            
            Assert.IsAssignableFrom<Polygon>(greenHouseCatchment.Geometry);
            Assert.AreEqual(2000,greenHouseCatchment.GeometryArea, 1e-7);
            Assert.IsTrue(greenHouseCatchment.IsGeometryDerivedFromAreaSize);

            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from1500to2000] = 3000;

            Assert.IsAssignableFrom<Polygon>(greenHouseCatchment.Geometry);
            Assert.AreEqual(5000, greenHouseCatchment.GeometryArea, 1e-7);
            Assert.IsTrue(greenHouseCatchment.IsGeometryDerivedFromAreaSize);
        }

        [Test]
        public void GivenGreenhouseData_ChangingAreaSum_ShouldNotChangeIndependentGeometry()
        {
            //Arrange
            var greenHouseCatchment = new Catchment
            {
                CatchmentType = CatchmentType.GreenHouse,
                Geometry = new Polygon(new LinearRing(new []
                {
                    new Coordinate(0,0),
                    new Coordinate(10,0),
                    new Coordinate(10,10),
                    new Coordinate(0,10),
                    new Coordinate(0,0)
                }))
            };
            var greenhouseData = new GreenhouseData(greenHouseCatchment);
            var geometryArea = greenHouseCatchment.GeometryArea;

            // Act & Assert
            Assert.IsFalse(greenHouseCatchment.IsGeometryDerivedFromAreaSize);
            Assert.AreEqual(100, geometryArea);

            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from3000to4000] = 2000;

            Assert.AreEqual(100, greenHouseCatchment.GeometryArea, 1e-7);
            Assert.IsFalse(greenHouseCatchment.IsGeometryDerivedFromAreaSize);

            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from1500to2000] = 3000;

            Assert.AreEqual(100, greenHouseCatchment.GeometryArea, 1e-7);
            Assert.IsFalse(greenHouseCatchment.IsGeometryDerivedFromAreaSize);
        }
    }
}
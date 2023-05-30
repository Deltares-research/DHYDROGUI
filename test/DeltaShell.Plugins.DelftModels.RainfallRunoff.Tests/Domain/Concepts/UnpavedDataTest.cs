using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts
{
    [TestFixture]
    public class UnpavedDataTest
    {
        [Test]
        public void GivenUnpavedData_ChangingAreaSum_ShouldChangeDependentGeometry()
        {
            //Arrange
            var catchment = new Catchment { CatchmentType = CatchmentType.Unpaved };
            var unpavedData = new UnpavedData(catchment);

            // check if no geometry is set
            Assert.IsNull(catchment.Geometry);
            Assert.IsTrue(catchment.IsGeometryDerivedFromAreaSize);

            // Act & Assert
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Corn] = 2000;

            Assert.IsAssignableFrom<Polygon>(catchment.Geometry);
            Assert.AreEqual(2000, catchment.GeometryArea, 1e-7);
            Assert.IsTrue(catchment.IsGeometryDerivedFromAreaSize);

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grass] = 3000;

            Assert.IsAssignableFrom<Polygon>(catchment.Geometry);
            Assert.AreEqual(5000, catchment.GeometryArea, 1e-7);
            Assert.IsTrue(catchment.IsGeometryDerivedFromAreaSize);
        }

        [Test]
        public void GivenUnpavedData_ChangingAreaSum_ShouldNotChangeIndependentGeometry()
        {
            //Arrange
            var catchment = new Catchment
            {
                CatchmentType = CatchmentType.Unpaved,
                Geometry = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0),
                    new Coordinate(10, 10),
                    new Coordinate(0, 10),
                    new Coordinate(0, 0)
                }))
            };
            var unpavedData = new UnpavedData(catchment);
            var geometryArea = catchment.GeometryArea;

            // Act & Assert
            Assert.IsFalse(catchment.IsGeometryDerivedFromAreaSize);
            Assert.AreEqual(100, geometryArea);

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Corn] = 2000;

            Assert.AreEqual(100, catchment.GeometryArea, 1e-7);
            Assert.IsFalse(catchment.IsGeometryDerivedFromAreaSize);

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grass] = 3000;

            Assert.AreEqual(100, catchment.GeometryArea, 1e-7);
            Assert.IsFalse(catchment.IsGeometryDerivedFromAreaSize);
        }

        [Test]
        public void GivenUnpavedData_Clone_Test()
        {
            var useLocalBoundaryData = true;

            //Arrange
            var catchment = new Catchment
            {
                CatchmentType = CatchmentType.Unpaved,
                Geometry = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0),
                    new Coordinate(10, 10),
                    new Coordinate(0, 10),
                    new Coordinate(0, 0)
                })),
            };
            var unpavedData = new UnpavedData(catchment) { UseLocalBoundaryData = useLocalBoundaryData };

            // Act & Assert
            var clone = unpavedData.Clone() as UnpavedData;

            Assert.AreEqual(useLocalBoundaryData, clone?.UseLocalBoundaryData);
        }
    }
}
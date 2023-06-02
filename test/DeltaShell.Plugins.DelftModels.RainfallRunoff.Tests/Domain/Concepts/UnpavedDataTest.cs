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
        public void Constructor_SetsCorrectModelDataOnCatchment()
        {
            // Setup
            var catchment = new Catchment();
            
            // Call
            var unpavedData = new UnpavedData(catchment);
            
            // Assert
            Assert.That(catchment.ModelData, Is.SameAs(unpavedData));
            Assert.That(unpavedData.Catchment, Is.SameAs(catchment));
        }

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

            // Arrange
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
            var unpavedData = new UnpavedData(catchment);
            unpavedData.BoundarySettings.UseLocalBoundaryData = useLocalBoundaryData;

            // Act
            var clone = unpavedData.Clone() as UnpavedData;

            // Assert
            Assert.AreEqual(useLocalBoundaryData, clone?.BoundarySettings.UseLocalBoundaryData);
        }

        [Test]
        public void DoAfterLinking_WhenAnotherUnpavedCatchmentToSameLateralExists_SetsSameBoundaryData()
        {
            // Setup
            var lateralSource = new LateralSource();

            UnpavedData unpavedCatchment = CreateUnpavedCatchmentLinkedToLateralSource(lateralSource);
            UnpavedData unpavedCatchment2 = CreateUnpavedCatchmentLinkedToLateralSource(lateralSource);
            
            // Precondition
            Assert.That(unpavedCatchment2.BoundarySettings, Is.Not.SameAs(unpavedCatchment.BoundarySettings));
            
            // Call
            unpavedCatchment2.DoAfterLinking(lateralSource);

            // Assert
            Assert.That(unpavedCatchment2.BoundarySettings, Is.SameAs(unpavedCatchment.BoundarySettings));
        }

        private static UnpavedData CreateUnpavedCatchmentLinkedToLateralSource(ILateralSource lateralSource)
        {
            var catchment = new Catchment();
            var unpavedCatchment = new UnpavedData(catchment);

            var link = new HydroLink(catchment, lateralSource);
            lateralSource.Links.Add(link);
            catchment.Links.Add(link);

            return unpavedCatchment;
        }

        [Test]
        public void DoAfterUnlinking_WhenAnotherUnpavedCatchmentToSameLateralExisted_CreatesCloneOfBoundaryData()
        {
            // Setup
            var lateralSource = new LateralSource();

            UnpavedData unpavedCatchment = CreateUnpavedCatchmentLinkedToLateralSource(lateralSource);
            UnpavedData unpavedCatchment2 = CreateUnpavedCatchmentLinkedToLateralSource(lateralSource);

            var boundaryData = new RainfallRunoffBoundaryData()
            {
                IsConstant = true,
                Value = 123
            };
            const bool useLocalBoundaryData = true;
            unpavedCatchment.BoundarySettings.BoundaryData = boundaryData;
            unpavedCatchment.BoundarySettings.UseLocalBoundaryData = useLocalBoundaryData;
            
            unpavedCatchment2.DoAfterLinking(lateralSource);
            
            // Precondition
            Assert.That(unpavedCatchment2.BoundarySettings, Is.SameAs(unpavedCatchment.BoundarySettings));
            
            // Call
            unpavedCatchment.Catchment.Links.RemoveAt(0); // remove the first and only link
            unpavedCatchment.DoAfterUnlinking();
            
            // Assert
            Assert.That(unpavedCatchment2.BoundarySettings, Is.Not.SameAs(unpavedCatchment.BoundarySettings));

            RainfallRunoffBoundaryData boundaryData2 = unpavedCatchment2.BoundarySettings.BoundaryData;
            Assert.That(boundaryData2.IsConstant, Is.EqualTo(boundaryData.IsConstant));
            Assert.That(boundaryData2.Value, Is.EqualTo(boundaryData.Value));
            Assert.That(unpavedCatchment2.BoundarySettings.UseLocalBoundaryData, Is.EqualTo(useLocalBoundaryData));
        }
    }
}
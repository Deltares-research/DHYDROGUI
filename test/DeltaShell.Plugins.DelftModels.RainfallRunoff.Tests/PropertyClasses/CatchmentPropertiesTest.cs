using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.PropertyClasses
{
    [TestFixture]
    public class CatchmentPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenCatchmentProperties_OpeningInPropertiesWindow_ShouldNotCrash()
        {
            var catchment = new Catchment{CatchmentType = CatchmentType.Unpaved};
            var catchmentData = new UnpavedData(catchment){CalculationArea = 20000};
            var catchmentProperties = new CatchmentProperties
            {
                Data = catchment,
                CatchmentData = catchmentData
            };

            Assert.DoesNotThrow(() => WindowsFormsTestHelper.ShowPropertyGridForObject(catchmentProperties));
        }

        [Test]
        public void GivenCatchmentProperties_SettingCalculationArea_ShouldGiveErrorMessageIfCatchmentDataIsNotSet()
        {
            //Arrange
            var catchment = new Catchment { CatchmentType = CatchmentType.Unpaved };
            var properties = new CatchmentProperties { Data = catchment };

            // Act & Assert
            TestHelper.AssertLogMessageIsGenerated(()=> properties.ComputationArea = 1000, $"Could not set {catchment.Name} computation area", 1);
        }

        [Test]
        public void GivenCatchmentProperties_GetSet_GetsReroutedToCatchmentOrCatchmentData()
        {
            //Arrange
            var polygon = new Polygon(new LinearRing(new[]
            {
                new Coordinate(0,0),
                new Coordinate(0,10),
                new Coordinate(10,10),
                new Coordinate(10,0),
                new Coordinate(0,0)
            }));

            var catchment = new Catchment
            {
                CatchmentType = CatchmentType.Unpaved,
                IsGeometryDerivedFromAreaSize = true,
                Geometry = polygon
            };

            var catchmentData = new UnpavedData(catchment) {CalculationArea = 10};

            var properties = new CatchmentProperties { Data = catchment, CatchmentData = catchmentData };
            
            // Act & Assert

            Assert.AreEqual(catchment.Name, properties.Name);
            properties.Name = "test";
            Assert.AreEqual(catchment.Name, properties.Name);

            Assert.AreEqual(catchment.LongName, properties.LongName);
            properties.LongName = "test";
            Assert.AreEqual(catchment.LongName, properties.LongName);

            properties.ComputationArea = 100;
            Assert.AreEqual(catchmentData.CalculationArea, properties.ComputationArea);

            Assert.AreEqual(catchment.Geometry.Area, properties.GeometryArea);
            Assert.AreEqual(catchment.CatchmentType, properties.CatchmentType);
            Assert.AreEqual(catchment.IsGeometryDerivedFromAreaSize, properties.IsDefaultGeometry);
        }
    }
}
using DelftTools.Hydro.Area.Objects;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class BridgePillarFmModelFeatureCoordinateDataSyncExtensionsTest
    {
        [Test]
        public void
            Test_FixedWeirFmModelFeatureCoordinateDataSyncExtensions_GivenModelFeatureCoordinateData_WhenFeatureIsAdded_ThenDefaultValuesCouldBeFound()
        {
            var lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10)
            });

            var brigdePillar = new BridgePillar() {Geometry = lineGeomery};

            var data = new ModelFeatureCoordinateData<BridgePillar> {Feature = brigdePillar};
            data.UpdateDataColumns();

            Assert.AreEqual(-999, data.DataColumns[0].ValueList[0]);
            Assert.AreEqual(-999, data.DataColumns[0].ValueList[1]);

            Assert.AreEqual(1, data.DataColumns[1].ValueList[0]);
            Assert.AreEqual(1, data.DataColumns[1].ValueList[1]);
        }
    }
}
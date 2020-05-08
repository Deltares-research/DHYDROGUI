using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Helpers
{
    [TestFixture]
    public class CrossSectionConverterTest
    {
        [Test]
        public void ConvertXYZCrossSectionToYz()
        {
            var crossSection = new CrossSectionDefinitionXYZ();
            crossSection.Geometry =
                new LineString(new Coordinate[]
                {
                    new Coordinate(-3, 0, 0),
                    new Coordinate(0, 4, -5),
                    new Coordinate(3, 0, 0)
                });

            crossSection.XYZDataTable[1].DeltaZStorage = 1;
            /*crossSection.SetWithHfswData(new[]
                                             {
                                                 new HeightFlowStorageWidth(0, 10, 10),
                                                 new HeightFlowStorageWidth(10, 20, 16)
                                             });*/

            CrossSectionDefinitionYZ yzCrossSection = CrossSectionConverter.ConvertToYz(crossSection);

            var yQ = new[]
            {
                0,
                5,
                10
            };
            int[] z = new[]
            {
                0,
                -5,
                0
            };
            var deltaZStorage = new[]
            {
                0,
                1,
                0
            };

            Assert.AreEqual(yQ, yzCrossSection.YZDataTable.Select(r => r.Yq).ToArray());

            Assert.AreEqual(z, yzCrossSection.YZDataTable.Select(r => r.Z).ToArray());
            Assert.AreEqual(deltaZStorage, yzCrossSection.YZDataTable.Select(r => r.DeltaZStorage).ToArray());
        }
    }
}
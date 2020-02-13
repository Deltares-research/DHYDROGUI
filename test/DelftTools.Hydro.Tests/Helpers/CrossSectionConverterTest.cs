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
                                   {new Coordinate(-3, 0, 0), new Coordinate(0, 4, -5), new Coordinate(3, 0, 0)});

            crossSection.XYZDataTable[1].DeltaZStorage = 1;
            

            var yzCrossSection = CrossSectionConverter.ConvertToYz(crossSection);

            var yQ = new[] {0, 5, 10};
            var z= new[] {0,-5, 0};
            var deltaZStorage = new[] {0, 1, 0};

            Assert.AreEqual(yQ,yzCrossSection.YZDataTable.Select(r=>r.Yq).ToArray());
            Assert.AreEqual(z, yzCrossSection.YZDataTable.Select(r => r.Z).ToArray());
        }
    }
}
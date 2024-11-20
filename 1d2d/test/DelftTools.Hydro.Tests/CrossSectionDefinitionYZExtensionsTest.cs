using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionDefinitionYzExtensionsTest
    {
        [Test]
        public void GivenCrossSectionDefinitionYZWithoutYAndZCoordinates_WhenSetYzCoordinateValues_ThenCoordinatesArePlacedInCrossSectionDefinition()
        {
            var cs = new CrossSectionDefinitionYZ();
            IList<double> yCoordinates = new List<double>()
            {
                0.0,
                1.1
            };
            IList<double> zCoordinates = new List<double>()
            {
                2.0,
                2.2
            };

            Assert.That(cs.YZDataTable.Count, Is.EqualTo(0));

            cs.SetYzValues(yCoordinates, zCoordinates);
            
            Assert.That(cs.YZDataTable.Count, Is.Not.EqualTo(0));

            for (var i = 0; i < cs.YZDataTable.Count; i++)
            {
                Assert.That(cs.YZDataTable[i].Yq, Is.EqualTo(yCoordinates[i]));
                Assert.That(cs.YZDataTable[i].Z, Is.EqualTo(zCoordinates[i]));
            }
        }
    }
}
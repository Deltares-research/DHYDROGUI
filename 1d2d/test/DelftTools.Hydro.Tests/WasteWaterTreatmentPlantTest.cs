using DelftTools.TestUtils;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class WasteWaterTreatmentPlantTest
    {
        [Test]
        public void Clone()
        {
            var wwtp = new WasteWaterTreatmentPlant {Geometry = new Point(15, 15), Name = "aa", Basin = new DrainageBasin()};
            wwtp.Attributes.Add("Milage",15);

            var clone = wwtp.Clone();

            ReflectionTestHelper.AssertPublicPropertiesAreEqual(wwtp, clone);
        }
    }
}
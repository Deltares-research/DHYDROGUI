using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.GroupableFeatures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects
{
    [TestFixture(typeof(BridgePillar))]
    [TestFixture(typeof(FixedWeir))]
    [TestFixture(typeof(LandBoundary2D))]
    [TestFixture(typeof(ObservationCrossSection2D))]
    [TestFixture(typeof(ThinDam2D))]
    public class AreaObjectTest<T> where T : new()
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var feat = new T();

            // Assert
            Assert.That(feat, Is.InstanceOf<GroupableFeature2D>());

        }
        
    }
}
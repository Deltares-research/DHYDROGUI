using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class BoundaryMdwBlockTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var result = new BoundaryMdwBlock();

            // Assert
            Assert.That(result.Name, Is.Null);
            Assert.That(result.XStartCoordinate, Is.EqualTo(0));
            Assert.That(result.YStartCoordinate, Is.EqualTo(0));
            Assert.That(result.XEndCoordinate, Is.EqualTo(0));
            Assert.That(result.YEndCoordinate, Is.EqualTo(0));
            Assert.That(result.PeakEnhancementFactor, Is.EqualTo(0));
            Assert.That(result.Spreading, Is.EqualTo(0));
            Assert.That(result.Distances, Is.Null);
            Assert.That(result.WaveHeights, Is.Null);
            Assert.That(result.Periods, Is.Null);
            Assert.That(result.Directions, Is.Null);
            Assert.That(result.DirectionalSpreadings, Is.Null);
            Assert.That(result.OrientationType, Is.Null);
            Assert.That(result.DistanceDirType, Is.Null);
        }
    }
}
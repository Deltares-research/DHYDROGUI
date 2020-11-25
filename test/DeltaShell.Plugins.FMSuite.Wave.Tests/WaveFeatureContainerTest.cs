using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveFeatureContainerTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var container = new WaveFeatureContainer();

            // Assert
            Assert.That(container, Is.InstanceOf<IWaveFeatureContainer>());
            Assert.That(container.Obstacles, Is.Empty);
            Assert.That(container.ObservationPoints, Is.Empty);
            Assert.That(container.ObservationCrossSections, Is.Empty);
        }
    }
}
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class BoundaryContainerTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var container = new BoundaryContainer();

            // Assert
            Assert.That(container, Is.InstanceOf<IBoundaryContainer>());
            Assert.That(container.Boundaries, Is.Not.Null);
            Assert.That(container.Boundaries, Is.Empty);
        }
    }
}
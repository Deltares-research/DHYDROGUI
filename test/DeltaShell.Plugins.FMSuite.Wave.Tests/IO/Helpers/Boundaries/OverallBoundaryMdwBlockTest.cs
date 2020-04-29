using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class OverallBoundaryMdwBlockTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var result = new OverallBoundaryMdwBlock();

            // Assert
            Assert.That(result.OverallSpectrumFile, Is.Null);
        }
    }
}
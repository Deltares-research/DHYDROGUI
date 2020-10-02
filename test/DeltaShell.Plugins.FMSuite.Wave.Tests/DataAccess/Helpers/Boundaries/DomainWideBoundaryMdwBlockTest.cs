using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class DomainWideBoundaryMdwBlockTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var result = new DomainWideBoundaryMdwBlock();

            // Assert
            Assert.That(result.DomainWideSpectrumFile, Is.Null);
        }
    }
}
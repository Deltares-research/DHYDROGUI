using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class SimpleBoundaryProviderTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var boundaries = new[]
            {
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>(),
                Substitute.For<IWaveBoundary>()
            };

            // Call
            var provider = new SimpleBoundaryProvider(boundaries);

            // Assert
            Assert.That(provider.Boundaries, Is.Not.Null);
            Assert.That(provider.Boundaries, Is.EquivalentTo(boundaries));
        }

        [Test]
        public void Constructor_WaveBoundariesNull_ThrowsArgumentNullException()
        {
            // Setup
            IWaveBoundary[] boundaries = null;

            // Call | Assert
            void Call() => new SimpleBoundaryProvider(boundaries);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundaries"));
        }
    }
}
using System;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class BoundaryMdwBlockTest
    {
        [Test]
        public void Constructor_NameNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundaryMdwBlock(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("name"));
        }

        [Test]
        public void Constructor_NameEmpty_ThrowsArgumentException()
        {
            // Call
            void Call() => new BoundaryMdwBlock(string.Empty);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("name"));
            Assert.That(exception.Message, Is.StringStarting("Argument cannot be empty."));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            const string name = "boundary_name";

            // Call
            var result = new BoundaryMdwBlock(name);

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.Definition, Is.Null);
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
        }
    }
}
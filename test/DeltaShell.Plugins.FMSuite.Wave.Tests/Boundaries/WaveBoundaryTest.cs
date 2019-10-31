using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class WaveBoundaryTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            // Call
            var waveBoundary = new WaveBoundary(geometricDefinition);

            // Assert
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));
        }

        [Test]
        public void Constructor_GeometricDefinitionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaveBoundary(null);
            
            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception, 
                        Has.Property("ParamName").EqualTo("geometricDefinition"));
        }
    }
}
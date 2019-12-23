using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NSubstitute;
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
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            // Call
            var waveBoundary = new WaveBoundary(geometricDefinition, conditionDefinition);

            // Assert
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition),
                        "Expected a different GeometricDefinition:");
            Assert.That(waveBoundary.ConditionDefinition, Is.SameAs(conditionDefinition),
                        "Expected a different ConditionDefinition:");
        }

        [Test]
        public void Constructor_GeometricDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            // Call
            void Call() => new WaveBoundary(null, conditionDefinition);
            
            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception, 
                        Has.Property("ParamName").EqualTo("geometricDefinition"));
        }

        [Test]
        public void Constructor_ConditionDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            // Call
            void Call() => new WaveBoundary(geometricDefinition, null);
            
            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception, 
                        Has.Property("ParamName").EqualTo("conditionDefinition"));
        }
    }
}
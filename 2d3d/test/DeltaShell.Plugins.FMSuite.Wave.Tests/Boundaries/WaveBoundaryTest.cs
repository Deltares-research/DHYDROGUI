using System;
using System.Collections.Generic;
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
        private readonly Random random = new Random(37);

        private static IEnumerable<TestCaseData> InvalidNames
        {
            get
            {
                yield return new TestCaseData("");
                yield return new TestCaseData(null);
            }
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var boundaryName = $"RandomBoundaryName({random.Next()})";

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            // Call
            var waveBoundary = new WaveBoundary(boundaryName,
                                                geometricDefinition,
                                                conditionDefinition);

            // Assert
            Assert.That(waveBoundary, Is.InstanceOf<IWaveBoundary>());

            Assert.That(waveBoundary.Name, Is.EqualTo(boundaryName),
                        "Expected a different Name:");
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition),
                        "Expected a different GeometricDefinition:");
            Assert.That(waveBoundary.ConditionDefinition, Is.SameAs(conditionDefinition),
                        "Expected a different ConditionDefinition:");
        }

        [Test]
        [TestCaseSource(nameof(InvalidNames))]
        public void Constructor_InvalidName_ThrowsArgumentNullException(string invalidName)
        {
            // Setup
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            // Call
            void Call() => new WaveBoundary(invalidName, geometricDefinition, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception, Has.Property("ParamName")
                                      .EqualTo("name"));
        }

        [Test]
        public void Constructor_GeometricDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryName = $"RandomBoundaryName({random.Next()})";
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            // Call
            void Call() => new WaveBoundary(boundaryName, null, conditionDefinition);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception, Has.Property("ParamName")
                                      .EqualTo("geometricDefinition"));
        }

        [Test]
        public void Constructor_ConditionDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryName = $"RandomBoundaryName({random.Next()})";
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            // Call
            void Call() => new WaveBoundary(boundaryName, geometricDefinition, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception, Has.Property("ParamName")
                                      .EqualTo("conditionDefinition"));
        }
    }
}
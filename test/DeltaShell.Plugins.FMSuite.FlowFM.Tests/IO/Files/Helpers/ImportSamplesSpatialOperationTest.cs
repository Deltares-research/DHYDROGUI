using System;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using NUnit.Framework;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers
{
    [TestFixture]
    public class ImportSamplesSpatialOperationTest
    {
        private readonly IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization {ConfigureMembers = true})
                                                         .Customize(new RandomBooleanSequenceCustomization())
                                                         .Customize(new RandomEnumCustomization());

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var operation = new ImportSamplesSpatialOperation();

            // Assert
            Assert.That(operation.RelativeSearchCellSize, Is.EqualTo(1));
            Assert.That(operation.MinSamplePoints, Is.EqualTo(1));
            Assert.That(operation.AveragingMethod, Is.EqualTo(GridCellAveragingMethod.ClosestPoint));
            Assert.That(operation.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Averaging));
            Assert.That(operation.HasMainInput, Is.False);
        }

        [Test]
        public void CreateOperations_ReturnsCorrectResult()
        {
            // Setup
            var operation = fixture.Create<ImportSamplesSpatialOperation>();

            // Call
            Tuple<ImportSamplesOperation, InterpolateOperation> result = operation.CreateOperations();

            // Assert
            ImportSamplesOperation importOperation = result.Item1;
            Assert.That(importOperation.Name, Is.EqualTo(operation.Name));
            Assert.That(importOperation.Dirty, Is.True);
            Assert.That(importOperation.Enabled, Is.EqualTo(operation.Enabled));
            Assert.That(importOperation.FilePath, Is.EqualTo(operation.FilePath));

            InterpolateOperation interpolateOperation = result.Item2;
            Assert.That(interpolateOperation.Name, Is.EqualTo("Interpolate"));
            Assert.That(interpolateOperation.Dirty, Is.True);
            Assert.That(interpolateOperation.Enabled, Is.EqualTo(operation.Enabled));
            Assert.That(interpolateOperation.RelativeSearchCellSize, Is.EqualTo(operation.RelativeSearchCellSize));
            Assert.That(interpolateOperation.MinNumSamples, Is.EqualTo(operation.MinSamplePoints));
            Assert.That(interpolateOperation.GridCellAveragingMethod, Is.EqualTo(operation.AveragingMethod));
            Assert.That(interpolateOperation.InterpolationMethod, Is.EqualTo(operation.InterpolationMethod));
            Assert.That(interpolateOperation.OperationType, Is.EqualTo(operation.Operand));
        }
    }
}
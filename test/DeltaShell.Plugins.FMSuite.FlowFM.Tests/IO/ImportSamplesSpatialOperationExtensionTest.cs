using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ImportSamplesSpatialOperationExtensionTest
    {
        [Test]
        public void CreateOperations_CreatesCorrectOperations(
            [Values] bool enabled,
            [Values] SpatialInterpolationMethod interpolationMethod,
            [Values] GridCellAveragingMethod averagingMethod,
            [Values] PointwiseOperationType operand)
        {
            const string name = "some_name";
            const string filePath = "some_file_path";
            const double relativeSearchSize = 1.23;
            const int minSamplePoints = 4;

            var importSamplesSpatialOperationExtension = new ImportSamplesSpatialOperationExtension
            {
                Name = name,
                FilePath = filePath,
                Enabled = enabled,
                InterpolationMethod = interpolationMethod,
                AveragingMethod = averagingMethod,
                RelativeSearchCellSize = relativeSearchSize,
                MinSamplePoints = 4,
                Operand = operand
            };

            // Call
            Tuple<ImportSamplesOperation, InterpolateOperation> convertedOperations = importSamplesSpatialOperationExtension.CreateOperations();

            // Assert
            ImportSamplesOperation importSamplesOperation = convertedOperations.First;
            Assert.That(importSamplesOperation.Name, Is.EqualTo(name));
            Assert.That(importSamplesOperation.Dirty, Is.True);
            Assert.That(importSamplesOperation.Enabled, Is.EqualTo(enabled));
            Assert.That(importSamplesOperation.FilePath, Is.EqualTo(filePath));

            InterpolateOperation interpolateOperation = convertedOperations.Second;
            Assert.That(interpolateOperation.Name, Is.EqualTo("Interpolation"));
            Assert.That(interpolateOperation.Dirty, Is.True);
            Assert.That(interpolateOperation.Enabled, Is.EqualTo(enabled));
            Assert.That(interpolateOperation.RelativeSearchCellSize, Is.EqualTo(relativeSearchSize));
            Assert.That(interpolateOperation.MinNumSamples, Is.EqualTo(minSamplePoints));
            Assert.That(interpolateOperation.GridCellAveragingMethod, Is.EqualTo(averagingMethod));
            Assert.That(interpolateOperation.InterpolationMethod, Is.EqualTo(interpolationMethod));
            Assert.That(interpolateOperation.OperationType, Is.EqualTo(operand));
        }
    }
}
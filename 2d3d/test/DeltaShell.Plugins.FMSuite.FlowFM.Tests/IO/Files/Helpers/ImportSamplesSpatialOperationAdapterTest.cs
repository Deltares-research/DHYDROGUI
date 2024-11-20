using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using NUnit.Framework;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers
{
    [TestFixture]
    public class ImportSamplesSpatialOperationAdapterTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            var importSamplesOperation = new ImportSamplesOperation(false)
            {
                Name = "importXyz",
                FilePath = "samples.xyz",
                Enabled = true
            };

            var interpolateOperation = new InterpolateOperation
            {
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                GridCellAveragingMethod = GridCellAveragingMethod.SimpleAveraging,
                RelativeSearchCellSize = 2.0
            };

            interpolateOperation.LinkInput(InterpolateOperation.InputSamplesName, importSamplesOperation.Output);

            var adapter = new ImportSamplesSpatialOperationAdapter(interpolateOperation);

            Assert.That(adapter.Name, Is.EqualTo("importXyz"));
            Assert.That(adapter.FilePath, Is.EqualTo("samples.xyz"));
            Assert.That(adapter.Enabled, Is.True);
            Assert.That(adapter.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Triangulation));
            Assert.That(adapter.AveragingMethod, Is.EqualTo(GridCellAveragingMethod.SimpleAveraging));
            Assert.That(adapter.RelativeSearchCellSize, Is.EqualTo(2.0));
        }

        [Test]
        public void FilePath_WhenSetToNewLocation_ImportSamplesOperationFilePathIsEqual()
        {
            var importSamplesOperation = new ImportSamplesOperation(false);
            var interpolateOperation = new InterpolateOperation();

            interpolateOperation.LinkInput(InterpolateOperation.InputSamplesName, importSamplesOperation.Output);

            var adapter = new ImportSamplesSpatialOperationAdapter(interpolateOperation);

            const string filePath = "chezy/chezy.xyz";
            adapter.FilePath = filePath;

            Assert.That(adapter.FilePath, Is.EqualTo(filePath));
            Assert.That(importSamplesOperation.FilePath, Is.EqualTo(filePath));
        }
    }
}
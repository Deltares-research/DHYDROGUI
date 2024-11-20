using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    /// <summary>
    /// Adapter class that adapts an <see cref="InterpolateOperation"/> to an <see cref="ImportSamplesSpatialOperation"/>.
    /// </summary>
    internal sealed class ImportSamplesSpatialOperationAdapter : ImportSamplesSpatialOperation
    {
        private readonly ImportSamplesOperation importSamplesOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportSamplesSpatialOperationAdapter"/> class.
        /// </summary>
        /// <param name="interpolateOperation">The interpolate operation to adapt.</param>
        public ImportSamplesSpatialOperationAdapter(InterpolateOperation interpolateOperation)
        {
            importSamplesOperation = GetImportSamplesSourceOperation(interpolateOperation);

            Name = importSamplesOperation.Name;
            Enabled = importSamplesOperation.Enabled;
            InterpolationMethod = interpolateOperation.InterpolationMethod;
            AveragingMethod = interpolateOperation.GridCellAveragingMethod;
            RelativeSearchCellSize = interpolateOperation.RelativeSearchCellSize;
        }

        /// <inheritdoc/>
        public override string FilePath
        {
            get => importSamplesOperation.FilePath;
            set => importSamplesOperation.FilePath = value;
        }

        private static ImportSamplesOperation GetImportSamplesSourceOperation(ISpatialOperation interpolateOperation)
        {
            return (ImportSamplesOperation)interpolateOperation.GetInput(InterpolateOperation.InputSamplesName).Source.Operation;
        }
    }
}
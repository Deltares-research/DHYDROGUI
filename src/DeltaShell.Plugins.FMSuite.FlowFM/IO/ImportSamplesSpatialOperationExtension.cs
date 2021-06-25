using System.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class ImportSamplesSpatialOperationExtension : ImportSamplesOperation
    {
        public virtual double RelativeSearchCellSize { get; set; }
        public virtual GridCellAveragingMethod AveragingMethod { get; set; }
        public virtual int MinSamplePoints { get; set; }
        public virtual SpatialInterpolationMethod InterpolationMethod { get; set; }
        public virtual PointwiseOperationType Operand { get; set; }
        
        public ImportSamplesSpatialOperationExtension() : base(false)
        {
            RelativeSearchCellSize = 1;
            MinSamplePoints = 1;
            AveragingMethod = GridCellAveragingMethod.ClosestPoint;
            InterpolationMethod = SpatialInterpolationMethod.Averaging;
        }

        public virtual DelftTools.Utils.Tuple<ImportSamplesOperation, InterpolateOperation> CreateOperations()
        {
            var importSamplesOperation = CreateImportSamplesOperation();

            var interpolateOperation = new InterpolateOperation
            {
                Name = "Interpolate",
                CoordinateSystem = CoordinateSystem,
                Dirty = true,
                Enabled = Enabled,
                RelativeSearchCellSize = RelativeSearchCellSize,
                MinNumSamples = MinSamplePoints,
                GridCellAveragingMethod = AveragingMethod,
                InterpolationMethod = InterpolationMethod,
                OperationType = Operand
            };
            interpolateOperation.Mask.Provider = new FeatureCollection(new List<Feature>(), typeof(Feature));
            interpolateOperation.LinkInput(InterpolateOperation.InputSamplesName, importSamplesOperation.Output);
            return new DelftTools.Utils.Tuple<ImportSamplesOperation, InterpolateOperation>(importSamplesOperation, interpolateOperation);
        }

        protected virtual ImportSamplesOperation CreateImportSamplesOperation()
        {
            return new ImportSamplesOperation(false)
            {
                Name = Name,
                CoordinateSystem = CoordinateSystem,
                Dirty = true,
                Enabled = Enabled,
                FilePath = FilePath,
            };
        }
    }
}
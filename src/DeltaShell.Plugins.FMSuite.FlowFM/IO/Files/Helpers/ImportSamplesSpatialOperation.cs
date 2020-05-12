using System;
using System.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    public class ImportSamplesSpatialOperation : ImportSamplesOperation
    {
        public double RelativeSearchCellSize { get; set; }
        public GridCellAveragingMethod AveragingMethod { get; set; }
        public SpatialInterpolationMethod InterpolationMethod { get; set; }

        public ImportSamplesSpatialOperation() : base(false)
        {
            RelativeSearchCellSize = 1;
            AveragingMethod = GridCellAveragingMethod.ClosestPoint;
            InterpolationMethod = SpatialInterpolationMethod.Averaging;
        }

        public Tuple<ImportSamplesOperation, InterpolateOperation> CreateOperations()
        {
            var importSamplesOperation = new ImportSamplesOperation(false)
            {
                Name = Name,
                CoordinateSystem = CoordinateSystem,
                Dirty = true,
                Enabled = Enabled,
                FilePath = FilePath,
            };

            var interpolateOperation = new InterpolateOperation
            {
                Name = "Interpolation",
                CoordinateSystem = CoordinateSystem,
                Dirty = true,
                Enabled = Enabled,
                RelativeSearchCellSize = RelativeSearchCellSize,
                GridCellAveragingMethod = AveragingMethod,
                InterpolationMethod = InterpolationMethod
            };
            interpolateOperation.Mask.Provider = new FeatureCollection(new List<Feature>(), typeof(Feature));
            interpolateOperation.LinkInput(InterpolateOperation.InputSamplesName, importSamplesOperation.Output);
            return new Tuple<ImportSamplesOperation, InterpolateOperation>(
                importSamplesOperation, interpolateOperation);
        }
    }
}
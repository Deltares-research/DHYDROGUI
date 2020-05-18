using System;
using System.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    /// <summary>
    /// Spatial operation that imports the samples.
    /// </summary>
    /// <seealso cref="ImportSamplesOperation" />
    public class ImportSamplesSpatialOperation : ImportSamplesOperation
    {
        /// <summary>
        /// Gets or sets the relative search cell size.
        /// </summary>
        public double RelativeSearchCellSize { get; set; }

        /// <summary>
        /// Gets or sets the averaging method.
        /// </summary>
        public GridCellAveragingMethod AveragingMethod { get; set; }

        /// <summary>
        /// Gets or sets the interpolation method.
        /// </summary>
        public SpatialInterpolationMethod InterpolationMethod { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportSamplesSpatialOperation"/> class.
        /// </summary>
        public ImportSamplesSpatialOperation() : base(false)
        {
            RelativeSearchCellSize = 1;
            AveragingMethod = GridCellAveragingMethod.ClosestPoint;
            InterpolationMethod = SpatialInterpolationMethod.Averaging;
        }

        /// <summary>
        /// Creates an <see cref="ImportSamplesOperation"/> with an <see cref="InterpolateOperation"/>.
        /// </summary>
        /// <returns>
        /// A tuple containing an <see cref="ImportSamplesOperation"/> with an <see cref="InterpolateOperation"/>.
        /// </returns>
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
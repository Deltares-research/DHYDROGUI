namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data
{
    /// <summary>
    /// Represent one initial condition or parameter field section from the initial field file.
    /// </summary>
    public sealed class InitialField
    {
        /// <summary>
        /// The quantity.
        /// </summary>
        public InitialFieldQuantity Quantity { get; set; } = InitialFieldQuantity.None;

        /// <summary>
        /// Name of the file containing the field data values.
        /// </summary>
        public string DataFile { get; set; }

        /// <summary>
        /// Type of the data file.
        /// </summary>
        public InitialFieldDataFileType DataFileType { get; set; } = InitialFieldDataFileType.None;

        /// <summary>
        /// Type of (spatial) operation method.
        /// </summary>
        public InitialFieldInterpolationMethod InterpolationMethod { get; set; } = InitialFieldInterpolationMethod.None;

        /// <summary>
        /// Type of operand; how the data is combined with existing data for this quantity.
        /// This is an optional setting. Defaults to <see cref="InitialFieldOperand.Override"/>.
        /// </summary>
        public InitialFieldOperand Operand { get; set; } = InitialFieldOperand.Override;

        /// <summary>
        /// Type of averaging.
        /// This is an optional setting and only relevant when <see cref="InterpolationMethod"/> is
        /// <see cref="InitialFieldInterpolationMethod.Averaging"/>.
        /// Defaults to <see cref="InitialFieldAveragingType.Mean"/>.
        /// </summary>
        public InitialFieldAveragingType AveragingType { get; set; } = InitialFieldAveragingType.Mean;

        /// <summary>
        /// Relative search cell size for averaging.
        /// This is an optional setting and only relevant when <see cref="InterpolationMethod"/> is
        /// <see cref="InitialFieldInterpolationMethod.Averaging"/>.
        /// Defaults to 1.01.
        /// </summary>
        public double AveragingRelSize { get; set; } = 1.01;

        /// <summary>
        /// Minimum number of points in averaging. Must be ≥ 1.
        /// This is an optional setting and only relevant when <see cref="InterpolationMethod"/> is
        /// <see cref="InitialFieldInterpolationMethod.Averaging"/>.
        /// Defaults to 1.
        /// </summary>
        public int AveragingNumMin { get; set; } = 1;

        /// <summary>
        /// Percentile value for which data values to include in averaging. 0.0 means off.
        /// This is an optional setting and only relevant when <see cref="InterpolationMethod"/> is
        /// <see cref="InitialFieldInterpolationMethod.Averaging"/>.
        /// Defaults to 0.0.
        /// </summary>
        public double AveragingPercentile { get; set; }

        /// <summary>
        /// Option for (spatial) extrapolation.
        /// This is an optional setting. Defaults to <c>false</c>.
        /// </summary>
        public bool ExtrapolationMethod { get; set; }

        /// <summary>
        /// Target location of interpolation.
        /// This is an optional setting. Defaults to <see cref="InitialFieldLocationType.All"/>.
        /// </summary>
        public InitialFieldLocationType LocationType { get; set; } = InitialFieldLocationType.All;

        /// <summary>
        /// The constant value to be set inside for all model points inside a polygon.
        /// This is an optional setting, but required when <see cref="DataFileType"/> is
        /// <see cref="InitialFieldDataFileType.Polygon"/>
        /// Defaults to <see cref="double.NaN"/>.
        /// </summary>
        public double Value { get; set; } = double.NaN;

        /// <summary>
        /// Name from the corresponding spatial operation on the model.
        /// </summary>
        public string SpatialOperationName { get; set; }

        /// <summary>
        /// Name from the corresponding spatial operation quantity on the model.
        /// </summary>
        public string SpatialOperationQuantity { get; set; }
    }
}
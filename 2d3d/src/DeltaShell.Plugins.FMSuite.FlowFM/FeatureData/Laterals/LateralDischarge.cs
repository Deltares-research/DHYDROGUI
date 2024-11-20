namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals
{
    /// <summary>
    /// Class representing the lateral discharge data.
    /// Lateral discharge data can be constant, time series, or real-time (external source).
    /// </summary>
    public sealed class LateralDischarge
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="LateralDischarge"/> class.
        /// </summary>
        public LateralDischarge()
        {
            Type = LateralDischargeType.Constant;
            TimeSeries = new LateralDischargeFunction();
        }

        /// <summary>
        /// Get or set the lateral discharge type.
        /// </summary>
        public LateralDischargeType Type { get; set; }

        /// <summary>
        /// Get or set the constant lateral discharge.
        /// </summary>
        public double Constant { get; set; }

        /// <summary>
        /// Get the lateral discharge time series function.
        /// </summary>
        public LateralDischargeFunction TimeSeries { get; }
    }
}
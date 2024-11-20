namespace DeltaShell.NGHS.IO
{
    /// <summary>
    /// A variable that can either be used as constant value,
    /// a time series or to be steered externally (RTC for example).
    /// </summary>
    public class Steerable
    {
        public double ConstantValue { get; set; }

        public string TimeSeriesFilename { get; set; }

        public SteerableMode Mode { get; set; }
    }

    public enum SteerableMode
    {
        /// <summary>
        /// The value is used as a time-constant value.
        /// </summary>
        ConstantValue, 
        /// <summary>
        /// The value is used as a time-varying value.
        /// </summary>
        TimeSeries, 
        /// <summary>
        /// The value is driven by some external force.
        /// </summary>
        External
    }
}
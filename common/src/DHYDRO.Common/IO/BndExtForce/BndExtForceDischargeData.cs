namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Represents a discharge definition in the new style external forcings file (*_bnd.ext).
    /// </summary>
    public sealed class BndExtForceDischargeData
    {
        /// <summary>
        /// Gets or sets the line number where the discharge data is located.
        /// </summary>
        public int LineNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the type of discharge.
        /// </summary>
        public BndExtForceDischargeType DischargeType { get; set; }

        /// <summary>
        /// Gets or sets the discharge scalar value.
        /// </summary>
        public double ScalarValue { get; set; }

        /// <summary>
        /// Gets or sets the name of the time series file (*.bc).
        /// </summary>
        public string TimeSeriesFile { get; set; }
    }
}
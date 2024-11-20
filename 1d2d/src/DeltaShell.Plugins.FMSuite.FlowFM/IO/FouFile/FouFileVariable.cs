namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Represents a variable in the statistical analysis configuration file (*.fou).
    /// </summary>
    public sealed class FouFileVariable
    {
        /// <summary>
        /// Gets or sets the quantity on which the analysis is to be performed.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Gets or sets the analysis start time in TUnit [seconds, minutes, hours] after the reference date.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// Gets or sets the analysis start time in TUnit [seconds, minutes, hours] after the reference date.
        /// </summary>
        public double StopTime { get; set; }

        /// <summary>
        /// Gets or sets the number of cycles within the analysis time frame.
        /// Alternatively, the length of the running mean filter.
        /// </summary>
        public int NumberOfCycles { get; set; }

        /// <summary>
        /// Gets or sets the nodal amplification factor. The default value is <c>1</c>.
        /// </summary>
        public int AmplificationFactor { get; set; } = 1;

        /// <summary>
        /// Gets or sets the phase shift in degrees.
        /// </summary>
        public int PhaseShift { get; set; }

        /// <summary>
        /// Gets or sets the layer number for the analysis of 3D quantities. This is an optional value.
        /// </summary>
        public int? LayerNumber { get; set; }

        /// <summary>
        /// Gets or sets the type of the analysis. This is an optional value.
        /// </summary>
        public string AnalysisType { get; set; }
    }
}
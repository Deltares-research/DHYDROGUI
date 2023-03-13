namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Represents a variable for statistical analysis.
    /// </summary>
    public sealed class FouFileVariable
    {
        /// <summary>
        /// Gets or sets the variable name.
        /// </summary>
        public string Name { get; set; }

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
        /// </summary>
        public int NumberOfCycles { get; set; }

        /// <summary>
        /// Gets or sets the nodal amplification factor.
        /// </summary>
        public int AmplificationFactor { get; set; } = 1;

        /// <summary>
        /// Gets or sets the astronomical argument.
        /// </summary>
        public int AstronomicalArgument { get; set; }

        /// <summary>
        /// Gets or sets the optional layer number for the fourier analysis.
        /// </summary>
        public int? LayerNumber { get; set; }

        /// <summary>
        /// Gets or sets the optional flag for the computation of elliptic parameters.
        /// </summary>
        public string EllipticParameters { get; set; }
    }
}
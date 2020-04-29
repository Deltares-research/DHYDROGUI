namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData
{
    /// <summary>
    /// <see cref="WaveBoundaryParameters"/> defines the parameters
    /// of a <see cref="IWaveBoundary"/>.
    /// </summary>
    public class WaveBoundaryParameters
    {
        /// <summary>
        /// Get or set the height.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Get or set the period.
        /// </summary>
        public double Period { get; set; } = 1.0;

        /// <summary>
        /// Get or set the direction.
        /// </summary>
        public double Direction { get; set; }

        /// <summary>
        /// Get or set the spreading.
        /// </summary>
        public double Spreading { get; set; }
    }
}
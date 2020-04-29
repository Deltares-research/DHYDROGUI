namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData
{
    /// <summary>
    /// <see cref="WaveBoundarySpectralData"/> defines the spectral data
    /// of a <see cref="IWaveBoundary"/>.
    /// </summary>
    public class WaveBoundarySpectralData
    {
        /// <summary>
        /// Get or set the <see cref="WaveSpectrumShapeType"/>.
        /// </summary>
        /// <value>
        /// The type of spectrum shape.
        /// </value>
        public WaveSpectrumShapeType ShapeType { get; set; }

        /// <summary>
        /// Get or set the <see cref="WavePeriodType"/>.
        /// </summary>
        /// <value>
        /// The period type.
        /// </value>
        public WavePeriodType PeriodType { get; set; }

        /// <summary>
        /// Get or set the <see cref="WaveDirectionalSpreadingType"/>.
        /// </summary>
        /// <value>
        /// The type of the directional spreading.
        /// </value>
        public WaveDirectionalSpreadingType DirectionalSpreadingType { get; set; }

        /// <summary>
        /// Get or set the peak enhancement factor.
        /// </summary>
        public double PeakEnhancementFactor { get; set; }

        /// <summary>
        /// Get or set the gaussian spreading value.
        /// </summary>
        public double GaussianSpreadingValue { get; set; } = 0.1;
    }
}
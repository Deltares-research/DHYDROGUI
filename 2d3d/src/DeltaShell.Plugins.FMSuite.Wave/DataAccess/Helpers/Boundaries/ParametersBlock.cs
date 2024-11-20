namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Describes one block with boundary parameters.
    /// </summary>
    public class ParametersBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParametersBlock"/> class.
        /// </summary>
        /// <param name="waveHeight"> The wave height. </param>
        /// <param name="period"> The period. </param>
        /// <param name="direction"> The direction. </param>
        /// <param name="directionalSpreading"> The directional spreading. </param>
        public ParametersBlock(double waveHeight, double period, double direction,
                               double directionalSpreading)
        {
            WaveHeight = waveHeight;
            Period = period;
            Direction = direction;
            DirectionalSpreading = directionalSpreading;
        }

        /// <summary>
        /// Gets the height of the wave.
        /// </summary>
        public double WaveHeight { get; }

        /// <summary>
        /// Gets the period.
        /// </summary>
        public double Period { get; }

        /// <summary>
        /// Gets the direction.
        /// </summary>
        public double Direction { get; }

        /// <summary>
        /// Gets the directional spreading.
        /// </summary>
        public double DirectionalSpreading { get; }
    }
}
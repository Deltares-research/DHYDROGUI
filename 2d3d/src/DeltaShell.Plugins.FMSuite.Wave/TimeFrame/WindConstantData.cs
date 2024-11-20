using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.Wave.TimeFrame
{
    /// <summary>
    /// <see cref="WindConstantData"/> defines the constant wind data
    /// of a wave model.
    /// </summary>
    [Entity]
    public class WindConstantData
    {
        /// <summary>
        /// Gets or sets the wind speed in meters per second.
        /// </summary>
        public double Speed { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the wind direction in degrees.
        /// </summary>
        public double Direction { get; set; } = 0.0;
    }
}
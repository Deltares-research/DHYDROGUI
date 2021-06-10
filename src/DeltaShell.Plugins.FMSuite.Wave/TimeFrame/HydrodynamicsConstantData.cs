using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.Wave.TimeFrame
{
    /// <summary>
    /// <see cref="HydrodynamicsConstantData"/> defines the constant hydrodynamics data
    /// of a wave model.
    /// </summary>
    [Entity]
    public sealed class HydrodynamicsConstantData
    {
        /// <summary>
        /// Gets or sets the water level in meters.
        /// </summary>
        /// 
        public double WaterLevel { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the velocity in the x-axis in meters per second.
        /// </summary>
        public double VelocityX { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the velocity in the x-axis in meters per second.
        /// </summary>
        public double VelocityY { get; set; } = 0.0;
    }
}
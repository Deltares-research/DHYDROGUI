namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="JonswapShape"/> defines the Jonswap type of
    /// <see cref="IBoundaryConditionShape"/> with a <see cref="PeakEnhancementFactor"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionShape" />
    public class JonswapShape : IBoundaryConditionShape
    {
        /// <summary>
        /// Gets the peak enhancement factor of this <see cref="JonswapShape"/>.
        /// </summary>
        /// <value>
        /// The peak enhancement factor.
        /// </value>
        public double PeakEnhancementFactor { get; set; }

        /// <summary>
        /// Name that should be written as value for "SpShapeType" property in Mdw.
        /// </summary>
        public string XmlName { get; } = "Jonswap";
    }
}
namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="GaussShape"/> defines the Gaussian type of
    /// <see cref="IBoundaryConditionShape"/> with a <see cref="GaussianSpread"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionShape" />
    public class GaussShape : IBoundaryConditionShape
    {
        /// <summary>
        /// Gets or sets the Gaussian spread.
        /// </summary>
        /// <value>
        /// The Gaussian spread
        /// </value>
        public double GaussianSpread { get; set; }

        /// <summary>
        /// Name that should be written as value for "SpShapeType" property in Mdw.
        /// </summary>
        public string XmlName { get; } = "Gauss";
    }
}
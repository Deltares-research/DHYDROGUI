using DeltaShell.Plugins.FMSuite.Wave.IO;

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
        /// Method for accepting visitors of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor"></param>
        public void AcceptVisitor(IShapeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
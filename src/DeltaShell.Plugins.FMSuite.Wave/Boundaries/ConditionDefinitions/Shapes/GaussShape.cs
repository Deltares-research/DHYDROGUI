
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="GaussShape"/> defines the Gaussian type of
    /// <see cref="IBoundaryConditionShape"/> with a <see cref="GaussianSpread"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionShape"/>
    public class GaussShape : IBoundaryConditionShape
    {
        /// <summary>
        /// Gets or sets the Gaussian spread.
        /// </summary>
        /// <value>
        /// The Gaussian spread
        /// </value>
        public double GaussianSpread { get; set; }

        public void AcceptVisitor(IShapeVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}
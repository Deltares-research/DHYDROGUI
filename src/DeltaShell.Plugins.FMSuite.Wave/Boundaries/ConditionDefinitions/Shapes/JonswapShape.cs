
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="JonswapShape"/> defines the Jonswap type of
    /// <see cref="IBoundaryConditionShape"/> with a <see cref="PeakEnhancementFactor"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionShape"/>
    public class JonswapShape : IBoundaryConditionShape
    {
        /// <summary>
        /// Gets the peak enhancement factor of this <see cref="JonswapShape"/>.
        /// </summary>
        /// <value>
        /// The peak enhancement factor.
        /// </value>
        public double PeakEnhancementFactor { get; set; }

        public void AcceptVisitor(IShapeVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}
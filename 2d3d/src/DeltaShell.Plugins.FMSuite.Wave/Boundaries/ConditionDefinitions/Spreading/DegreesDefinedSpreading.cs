
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="PowerDefinedSpreading"/> defines the spreading
    /// defined by the degrees value contained in <see cref="DegreesSpreading"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionSpreading"/>
    public class DegreesDefinedSpreading : IBoundaryConditionSpreading
    {
        /// <summary>
        /// Gets or sets the degrees of spreading.
        /// </summary>
        public double DegreesSpreading { get; set; } = WaveSpreadingConstants.DegreesDefaultSpreading;

        public void AcceptVisitor(ISpreadingVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}
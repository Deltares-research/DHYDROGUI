
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="PowerDefinedSpreading"/> defines the spreading
    /// defined by the power value contained in <see cref="SpreadingPower"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionSpreading"/>
    public class PowerDefinedSpreading : IBoundaryConditionSpreading
    {
        /// <summary>
        /// Gets or sets the spreading power.
        /// </summary>
        public double SpreadingPower { get; set; } = WaveSpreadingConstants.PowerDefaultSpreading;

        public void AcceptVisitor(ISpreadingVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}
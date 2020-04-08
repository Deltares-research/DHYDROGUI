using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="PowerDefinedSpreading"/> defines the spreading
    /// defined by the power value contained in <see cref="SpreadingPower"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionSpreading" />
    public class PowerDefinedSpreading : IBoundaryConditionSpreading
    {
        /// <summary>
        /// Gets or sets the spreading power.
        /// </summary>
        public double SpreadingPower { get; set; } = WaveSpreadingConstants.PowerDefaultSpreading;

        /// <summary>
        /// Method for accepting visitors of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor"></param>
        public void AcceptVisitor(ISpreadingVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
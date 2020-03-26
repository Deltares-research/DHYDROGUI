using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="PowerDefinedSpreading"/> defines the spreading
    /// defined by the degrees value contained in <see cref="DegreesSpreading"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionSpreading" />
    public class DegreesDefinedSpreading : IBoundaryConditionSpreading
    {
        /// <summary>
        /// Gets or sets the degrees of spreading.
        /// </summary>
        public double DegreesSpreading { get; set; } = WaveSpreadingConstants.DegreesDefaultSpreading;

        /// <summary>
        /// Method for accepting visitors of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor"></param>
        public void AcceptVisitor(IDataComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
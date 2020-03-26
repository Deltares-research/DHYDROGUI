using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="PiersonMoskowitzShape"/> defines the Pierson-Moskowitz <see cref="IBoundaryConditionShape"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionShape" />
    /// <remarks>
    /// No data is associated with this <see cref="IBoundaryConditionShape"/>.
    /// </remarks>
    public class PiersonMoskowitzShape : IBoundaryConditionShape
    {
        /// <summary>
        /// Method for accepting visitors of the visitor design pattern,
        /// used for the export.
       /// </summary>
        /// <param name="visitor"></param>
        public void AcceptVisitor(IBoundaryConditionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
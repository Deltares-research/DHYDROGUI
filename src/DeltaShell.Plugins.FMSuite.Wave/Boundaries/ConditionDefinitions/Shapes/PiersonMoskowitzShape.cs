using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="PiersonMoskowitzShape"/> defines the Pierson-Moskowitz <see cref="IBoundaryConditionShape"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionShape"/>
    /// <remarks>
    /// No data is associated with this <see cref="IBoundaryConditionShape"/>.
    /// </remarks>
    public class PiersonMoskowitzShape : IBoundaryConditionShape
    {
        public void AcceptVisitor(IShapeVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}
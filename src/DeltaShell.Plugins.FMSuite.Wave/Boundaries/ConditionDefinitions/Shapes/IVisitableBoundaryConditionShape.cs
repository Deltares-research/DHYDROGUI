using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    public interface IVisitableBoundaryConditionShape
    {
        /// <summary>
        /// Method needed for visitor design pattern.
        /// </summary>
        /// <param name="visitor"></param>
        void AcceptVisitor(IShapeVisitor visitor);
    }
}
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    public interface IVisitableBoundaryConditionSpreading
    {
        /// <summary>
        /// Method needed for visitor design pattern.
        /// </summary>
        /// <param name="visitor"></param>
        void AcceptVisitor(ISpreadingVisitor visitor);
    }
}
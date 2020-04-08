using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents
{
    public interface IVisitableBoundaryConditionDataComponent
    {
        /// <summary>
        /// Method needed for visitor design pattern.
        /// </summary>
        /// <param name="visitor"></param>
        void AcceptVisitor(IDataComponentVisitor visitor);
    }
}
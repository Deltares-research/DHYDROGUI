using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters
{
    public interface IVisitableBoundaryConditionParameters
    {
        /// <summary>
        /// Method needed for visitor design pattern.
        /// </summary>
        /// <param name="visitor"></param>
        void AcceptVisitor(IParametersVisitor visitor);
    }
}
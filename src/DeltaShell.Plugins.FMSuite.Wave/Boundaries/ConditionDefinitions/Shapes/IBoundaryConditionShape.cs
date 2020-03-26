using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="IBoundaryConditionShape"/> defines the different shapes used
    /// within the <see cref="IWaveBoundaryConditionDefinition"/>.
    /// </summary>
    public interface IBoundaryConditionShape
    {
        /// <summary>
        /// Method needed for visitor design pattern.
        /// </summary>
        /// <param name="visitor"></param>
        void AcceptVisitor(IBoundaryConditionVisitor visitor);
    }
}
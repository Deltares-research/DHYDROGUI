using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="IBoundaryConditionSpreading"/> defines the different shapes used
    /// within the <see cref="IWaveBoundaryConditionDefinition"/>.
    /// </summary>
    /// <remarks>
    /// This interface acts as a discriminated union for its implementing types.
    /// As such, this interface is empty, and other classes will use it to type
    /// cast to the implementing types.
    /// </remarks>
    public interface IBoundaryConditionSpreading
    {
        /// <summary>
        /// Method needed for visitor design pattern.
        /// </summary>
        /// <param name="visitor"></param>
        void AcceptVisitor(IDataComponentVisitor visitor);
    }
}
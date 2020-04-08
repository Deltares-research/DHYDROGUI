using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public interface IDataComponentVisitor
    {
        void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IBoundaryConditionParameters;

        void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IBoundaryConditionParameters;
    }
}
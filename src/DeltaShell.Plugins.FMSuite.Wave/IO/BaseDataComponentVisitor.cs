using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Base class for all visitors of wave boundaries. It visits the data component object inside a wave boundary.
    /// All methods are defined empty and virtual, so that visitors should only override methods, which are useful for them.
    /// </summary>
    public class BaseDataComponentVisitor : IDataComponentVisitor
    {
        public virtual void Visit<T>(UniformDataComponent<T> uniformDataComponent)
            where T : IBoundaryConditionParameters {}

        public virtual void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent)
            where T : IBoundaryConditionParameters {}

        public virtual void Visit<T>(ConstantParameters<T> constantParameters)
            where T : IBoundaryConditionSpreading, new() {}

        public virtual void Visit<T>(TimeDependentParameters<T> timeDependentParameters)
            where T : IBoundaryConditionSpreading, new() {}

        public virtual void Visit(DegreesDefinedSpreading degreesDefinedSpreading) {}

        public virtual void Visit(PowerDefinedSpreading powerDefinedSpreading) {}
    }
}
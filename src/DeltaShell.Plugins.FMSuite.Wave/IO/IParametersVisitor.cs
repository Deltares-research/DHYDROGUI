using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public interface IParametersVisitor
    {
        void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new();

        void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new();
    }
}
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public interface IBoundaryConditionVisitor
    { 
        void Visit(IWaveBoundaryConditionDefinition waveBoundaryConditionDefinition);
    }
}
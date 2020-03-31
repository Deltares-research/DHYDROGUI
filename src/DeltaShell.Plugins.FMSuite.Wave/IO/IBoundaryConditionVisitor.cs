using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public interface IBoundaryConditionVisitor : IDataComponentVisitor
    {
        void Visit(GaussShape gaussShape);
        
        void Visit(JonswapShape jonswapShape);
        
        void Visit(PiersonMoskowitzShape piersonMoskowitzShape);
        void Visit(IWaveBoundaryConditionDefinition waveBoundaryConditionDefinition);
    }
}
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public interface ISpreadingVisitor
    {
        void Visit(DegreesDefinedSpreading degreesDefinedSpreading);

        void Visit(PowerDefinedSpreading powerDefinedSpreading);
    }
}
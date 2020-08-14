using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Dimr
{
    public interface IDimrStateAwareModel
    {
        void PrepareRestart();
        void WriteRestartFiles();
        void FinalizeRestart();
    }
}
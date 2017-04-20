using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Dimr
{
    public interface IDimrStateAwareModel : IStateAwareModelEngine
    {
        void PrepareRestart();
        void WriteRestartFiles();
        void FinalizeRestart();
    }
}
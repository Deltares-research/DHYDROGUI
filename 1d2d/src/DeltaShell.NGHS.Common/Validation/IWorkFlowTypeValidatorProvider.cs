using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.NGHS.Common.Validation
{
    public interface IWorkFlowTypeValidatorProvider
    {
        IWorkFlowTypeValidator CreateWorkFlowTypeValidator(IActivity activity);
    }
}
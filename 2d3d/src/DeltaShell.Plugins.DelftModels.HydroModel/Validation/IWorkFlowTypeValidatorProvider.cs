using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Validation
{
    public interface IWorkFlowTypeValidatorProvider
    {
        IWorkFlowTypeValidator CreateWorkFlowTypeValidator(IActivity activity);
    }
}
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Validation
{
    public class WorkFlowTypeValidator : IWorkFlowTypeValidator
    {
        private bool valid = false;
        public WorkFlowTypeValidator(IActivity activity)
        {
            valid = true;
        }

        #region Implementation of IWorkFlowTypeValidator

        public bool Valid()
        {
            return valid;
        }

        #endregion
    }
}
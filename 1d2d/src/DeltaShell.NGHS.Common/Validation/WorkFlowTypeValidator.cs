using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.NGHS.Common.Validation
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
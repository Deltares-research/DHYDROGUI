using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Common.Validation;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class RainfallRunoffInWorkFlowTypeValidator : IWorkFlowTypeValidator
    {
        private bool valid = false;
        public RainfallRunoffInWorkFlowTypeValidator(IActivity activity)
        {
            valid = false;
        }

        #region Implementation of IWorkFlowTypeValidator

        public bool Valid()
        {
            return valid;
        }

        #endregion
    }
}
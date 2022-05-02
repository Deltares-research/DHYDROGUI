using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Common.Validation;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class RainfallRunoffInWorkFlowTypeValidatorProvider : IWorkFlowTypeValidatorProvider
    {
        #region Implementation of IWorkFlowTypeValidatorProvider

        public IWorkFlowTypeValidator CreateWorkFlowTypeValidator(IActivity activity)
        {
            var compositeActivity = activity as ICompositeActivity;
            if (compositeActivity == null) return null;
            if (!(compositeActivity is SequentialActivity)) return null;
            return compositeActivity.GetActivitiesOfType<IRainfallRunoffModel>().Any() ? new RainfallRunoffInWorkFlowTypeValidator(activity) : null;
        }

        #endregion
    }
}
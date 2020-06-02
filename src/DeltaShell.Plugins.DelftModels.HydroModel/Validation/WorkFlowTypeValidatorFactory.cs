using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Validation
{
    public static class WorkFlowTypeValidatorFactory
    {
        private static readonly List<IWorkFlowTypeValidatorProvider> WorkFlowTypeValidators = new List<IWorkFlowTypeValidatorProvider>();

        public static IWorkFlowTypeValidator GetWorkFlowTypeValidator(IActivity activity)
        {
            IWorkFlowTypeValidator workFlowTypeValidator = WorkFlowTypeValidators.Select(p => p.CreateWorkFlowTypeValidator(activity)).FirstOrDefault(c => c != null);
            return workFlowTypeValidator ?? new WorkFlowTypeValidator(activity);
        }
    }
}
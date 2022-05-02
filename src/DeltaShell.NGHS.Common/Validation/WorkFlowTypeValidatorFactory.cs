using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.NGHS.Common.Validation
{
    public static class WorkFlowTypeValidatorFactory
    {
        public static List<IWorkFlowTypeValidatorProvider> WorkFlowTypeValidators { get; } = new List<IWorkFlowTypeValidatorProvider>();

        public static IWorkFlowTypeValidator GetWorkFlowTypeValidator(IActivity activity)
        {
            var workFlowTypeValidator = WorkFlowTypeValidators.Select(p => p.CreateWorkFlowTypeValidator(activity)).FirstOrDefault(c => c != null);
            return workFlowTypeValidator ?? new WorkFlowTypeValidator(activity);
        }
    }
}
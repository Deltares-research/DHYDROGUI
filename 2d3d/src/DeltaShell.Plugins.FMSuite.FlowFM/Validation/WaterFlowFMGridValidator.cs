using System.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMGridValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            if (model.Grid == null || model.Grid.IsEmpty)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error, "Grid is empty"));
            }

            return new ValidationReport("Domain", issues);
        }
    }
}
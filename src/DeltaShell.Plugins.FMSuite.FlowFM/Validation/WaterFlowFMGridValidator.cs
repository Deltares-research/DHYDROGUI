using System.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMGridValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();
            var network1dInvalid = model.NetworkDiscretization.Locations.Values.Count == 0;
            if (network1dInvalid && (model.Grid == null || model.Grid.IsEmpty))
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error, Resources.WaterFlowFMGridValidator_Validate_Grid_is_empty));

            return new ValidationReport("Domain", issues);
        }
    }
}
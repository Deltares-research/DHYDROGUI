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
            if (model.Grid == null || model.Grid.IsEmpty)
            {
                if (!ModelNetworkDiscretizationIsValid(model))
                {
                    issues.Add(new ValidationIssue(model, ValidationSeverity.Error, Resources.WaterFlowFMGridValidator_Validate_Grid_is_empty));
                }
            }
            return new ValidationReport("Domain", issues);
        }

        private static bool ModelNetworkDiscretizationIsValid(WaterFlowFMModel model)
        {
            if (model.NetworkDiscretization == null
                || model.NetworkDiscretization.Locations == null
                || model.NetworkDiscretization.Locations.Values.Count == 0)
                return false;
            return true;
        }
    }
}
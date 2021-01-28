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
            if ((model.Grid == null || model.Grid.IsEmpty) && 
                !ModelNetworkDiscretizationIsValid(model))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error, Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_));
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
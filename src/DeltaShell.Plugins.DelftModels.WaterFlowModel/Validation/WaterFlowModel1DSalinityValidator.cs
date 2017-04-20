using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public static class WaterFlowModel1DSalinityValidator
    {
        public static ValidationReport Validate(WaterFlowModel1D model)
        {
            if (!model.UseSalt) return new ValidationReport("Salinity", Enumerable.Empty<ValidationIssue>());

            return new ValidationReport("Salinity", new[]
            {
                ValidateSalinityBoundaryConditions(model),
                ValidateSalinityPathForThatcherHarlemanValid(model)
            });
        }

        private static ValidationReport ValidateSalinityBoundaryConditions(WaterFlowModel1D model)
        {
            var issues = new List<ValidationIssue>();
            
            var invalidSaltBoundaryConditions = model.BoundaryConditions
                .Where(bc => bc.DataType != WaterFlowModel1DBoundaryNodeDataType.None
                    && bc.SaltConditionType == SaltBoundaryConditionType.None);

            foreach (var bc in invalidSaltBoundaryConditions)
            {
                issues.Add(new ValidationIssue(bc, ValidationSeverity.Error, string.Format(
                    Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_The_boundary_condition__0__has_a_salinity_type_of_None__All_open_boundaries_must_specify_salinity_values_, bc.Name)));
            }
            
            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_Boundary_conditions, issues);
        }
       
        private static ValidationReport ValidateSalinityPathForThatcherHarlemanValid(WaterFlowModel1D flowModel1D)
        {
            if (flowModel1D.DispersionFormulationType == DispersionFormulationType.Constant)
                return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateSalinity_Salinity, Enumerable.Empty<ValidationIssue>());

            var issues = new List<ValidationIssue>();
            
            if (flowModel1D.SalinityValidNonConstantFormulation)
            {
                if (string.IsNullOrEmpty(flowModel1D.SalinityPath))
                {
                    issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error,
                        Resources.SalinityValidator_SpatialDataForDispersionF4CoefficientExistsButNoSalinityIniFileHasBeenSpecified));
                }
                else if (!File.Exists(flowModel1D.SalinityPath))
                {
                    var error = string.Format(Resources.SalinityValidator_SpatialDataForDispersionF4CoefficientExistsButSalinityIniFileWasNotFound, flowModel1D.SalinityPath);
                    issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, error));
                }
            }
            else
            {
                if (File.Exists(flowModel1D.SalinityPath))
                {
                    var error = string.Format(Resources.SalinityValidator_SalinityIniFileFoundButNoSpatialDataForDispersionF4CoefficientExists, flowModel1D.SalinityPath);
                    issues.Add(new ValidationIssue(flowModel1D, ValidationSeverity.Error, error));
                }
            }
            
            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateSalinity_Salinity, issues);
        }
    }
}

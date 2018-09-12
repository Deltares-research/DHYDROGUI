using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFmModelValidationExtensions
    {
        public static ValidationReport Validate(this WaterFlowFMModel model)
        {
            var validationReports = new[]
            {
                ValidateCoordinateSystem(model),
                WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model),
                WaterFlowFMModelNetworkValidator.Validate(model.Network),
                WaterFlowFMGridValidator.Validate(model),
                ValidateBathymetry(model),
                ValidatePhysicalProcesses(model),
                WaterFlowFMWindValidator.Validate(model),
                WaterFlowFMModelDefinitionValidator.Validate(model),
                WaterFlowFMBoundaryConditionValidator.Validate(model),
                WaterFlowFMArea2DValidator.Validate(model),
                ValidateRestartInput(model),
                WaterFlowFMEmbankmentValidator.Validate(model),
                WaterFlowFMEnclosureValidator.Validate(model),
            };

            var subReports = model.UseMorSed
                ? validationReports.Plus(WaterFlowFMSedimentMorphologyValidator.ValidateWithMorphologyBetaWarning(model))
                : validationReports;

            var validationReport = new ValidationReport(model.Name + " (Water Flow FM Model)",
                subReports);
            
            return validationReport;
        }

        private static ValidationReport ValidatePhysicalProcesses(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var roughnessValues = model.Roughness.GetValues<double>();
            if (roughnessValues.Contains(model.Roughness.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                    string.Format("Roughness contains unspecified points, the calculation kernel will replace these with default values")));
            }

            var viscosityValues = model.Viscosity.GetValues<double>();
            if (viscosityValues.Contains(model.Viscosity.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                    string.Format("Viscosity contains unspecified points, the calculation kernel will replace these with default values")));
            }

            var diffusivityValues = model.Diffusivity.GetValues<double>();
            if (diffusivityValues.Contains(model.Diffusivity.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                    string.Format("Diffusivity contains unspecified points, the calculation kernel will replace these with default values")));
            }

            return new ValidationReport("Physical Processes", issues);
        }

        private static ValidationReport ValidateBathymetry(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var values = model.Bathymetry.GetValues<double>();
            if (values.Contains(model.Bathymetry.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                    string.Format("Bathymetry contains unspecified points, the calculation kernel will replace these with default values")));
            }
            return new ValidationReport("Bathymetry", issues);
        }

        private static ValidationReport ValidateCoordinateSystem(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            if (model.CoordinateSystem == null)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                                               "No coordinate system specified. The kernel will assume a projected system."));
            }
            else if (model.CoordinateSystem.IsGeographic && model.CoordinateSystem.AuthorityCode != 4326 /*WGS84*/)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    "The geographic coordinate system specified may lead to incorrect results in the calculation. The calculation kernel only supports the WGS84 spherical system."));
            }
            return new ValidationReport("Coordinate System", issues);
        }

        private static ValidationReport ValidateRestartInput(WaterFlowFMModel model)
        {
            if (!model.UseRestart) return new ValidationReport("Input restart state", Enumerable.Empty<ValidationReport>());

            IList<ValidationIssue> issues = new List<ValidationIssue>();

            if (model.RestartInput.IsEmpty)
            {
                issues.Add(new ValidationIssue("Input restart state", ValidationSeverity.Error,
                                               "Input restart state is empty; cannot restart."));
            }
            else
            {
                IEnumerable<string> errors, warnings;
                model.ValidateInputState(out errors, out warnings);

                issues = errors.Select(error => new ValidationIssue("Input restart state", ValidationSeverity.Error, error)).ToList();
                issues.AddRange(warnings.Select(warning => new ValidationIssue("Input restart state", ValidationSeverity.Warning, warning)));
            }
            return new ValidationReport("Input restart state", issues);
        }
    }

}

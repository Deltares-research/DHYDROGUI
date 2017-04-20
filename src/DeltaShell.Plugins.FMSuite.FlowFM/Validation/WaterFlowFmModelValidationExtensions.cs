using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFmModelValidationExtensions
    {
        public static ValidationReport Validate(this WaterFlowFMModel model, WaterFlowFMModel target = null)
        {
            return new ValidationReport(model.Name + " (Water Flow FM Model)",
                                        new[]
                                            {
                                                ValidateCoordinateSystem(model),
                                                WaterFlowFMGridValidator.Validate(model),
                                                ValidateBathymetry(model),
                                                ValidateInitialConditions(model),
                                                ValidatePhysicalProcesses(model),
                                                WaterFlowFMWindValidator.Validate(model),
                                                WaterFlowFMModelDefinitionValidator.Validate(model),
                                                WaterFlowFMBoundaryConditionValidator.Validate(model),
                                                WaterFlowFMArea2DValidator.Validate(model),
                                                ValidateRestartInput(model),
                                                WaterFlowFMEmbankmentValidator.Validate(model)
                                            });
        }

        private static ValidationReport ValidatePhysicalProcesses(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var roughnessValues = model.Roughness.GetValues<double>();
            if (roughnessValues.Contains(model.Roughness.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    string.Format("Roughness contains unspecified points, the calculation kernel will replace these with default values")));
            }

            var viscosityValues = model.Viscosity.GetValues<double>();
            if (viscosityValues.Contains(model.Viscosity.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    string.Format("Viscosity contains unspecified points, the calculation kernel will replace these with default values")));
            }

            var diffusivityValues = model.Diffusivity.GetValues<double>();
            if (diffusivityValues.Contains(model.Diffusivity.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    string.Format("Diffusivity contains unspecified points, the calculation kernel will replace these with default values")));
            }

            return new ValidationReport("Physical Processes", issues);
        }

        private static ValidationReport ValidateInitialConditions(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var waterlevelValues = model.InitialWaterLevel.GetValues<double>();
            if (waterlevelValues.Contains(model.InitialWaterLevel.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    string.Format("Initial Water Level contains unspecified points, the calculation kernel will replace these with default values")));   
            }

            var salinityValues = model.InitialSalinity.Coverages.SelectMany(c => c.GetValues<double>());
            if (model.UseSalinity && model.InitialSalinity.Coverages.Any() && salinityValues.Contains((double)model.InitialSalinity.Coverages[0].Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    string.Format("Initial Salinity contains unspecified points, the calculation kernel will replace these with default values")));
            }

            var tracerValues = model.InitialTracers.SelectMany(c => c.GetValues<double>());
            var firstTracer = model.InitialTracers.FirstOrDefault();
            if (firstTracer != null && tracerValues.Contains((double)firstTracer.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    string.Format("Initial Tracers contain unspecified points, the calculation kernel will replace these with default values")));
            }

            var temperatureValues = model.InitialTemperature.GetValues<double>();
            if (model.HeatFluxModelType != HeatFluxModelType.None && temperatureValues.Contains(model.InitialTemperature.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning, 
                    string.Format("Initial Temperature contains unspecified points, the calculation kernel will replace these with default values")));   
            }


            return new ValidationReport("Initial Conditions", issues);
        }

        private static ValidationReport ValidateBathymetry(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var values = model.Bathymetry.GetValues<double>();
            if (values.Contains(model.Bathymetry.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
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

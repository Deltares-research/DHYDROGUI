using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMProcessesValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            ISpatialData spatialData = model.SpatialData;
            IMultiDimensionalArray<double> roughnessValues = spatialData.Roughness.GetValues<double>();
            if (roughnessValues.Contains(spatialData.Roughness.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                                               "Roughness contains unspecified points, the calculation kernel will replace these with default values"));
            }

            IMultiDimensionalArray<double> viscosityValues = spatialData.Viscosity.GetValues<double>();
            if (viscosityValues.Contains(spatialData.Viscosity.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                                               "Viscosity contains unspecified points, the calculation kernel will replace these with default values"));
            }

            IMultiDimensionalArray<double> diffusivityValues = spatialData.Diffusivity.GetValues<double>();
            if (diffusivityValues.Contains(spatialData.Diffusivity.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                                               "Diffusivity contains unspecified points, the calculation kernel will replace these with default values"));
            }

            HeatFluxModel heatFluxModel = model.ModelDefinition.HeatFluxModel;
            if (model.HeatFluxModelType == HeatFluxModelType.Composite
                && !heatFluxModel.MeteoData.GetValues<double>().Any())
            {
                issues.Add(new ValidationIssue(model,
                                               ValidationSeverity.Error,
                                               Resources.ValidatePhysicalProcesses_HeatFluxModel_has_composite_model_option_selected_for_temperature_but_no_meteo_data_was_specified,
                                               heatFluxModel));
            }

            return new ValidationReport("Physical Processes", issues);
        }
    }
}
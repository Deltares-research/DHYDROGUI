using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveOutputParametersValidator
    {
        /// <summary>
        /// Validates wave model settings that are shown in the output parameters window of wave models.
        /// </summary>
        /// <param name="waveModel"> The wave model that is being validated. </param>
        /// <returns> </returns>
        public static ValidationReport Validate(WaveModel waveModel)
        {
            var validationIssues = new List<ValidationIssue>();
            if (waveModel.WriteTable && !waveModel.FeatureContainer.ObservationPoints.Any())
            {
                validationIssues.Add(new ValidationIssue(waveModel, ValidationSeverity.Warning, 
                                                         Resources.WaveOutputParametersValidator_Validate_Option__Write_Tables__is_selected_but_there_are_no_Observation_Points_in_your_model_));
            }

            // Note this ValidationIssue can be removed once DSF properly supports single-precision netCDF values, see D3DFMIQ-2555.
            if ((bool) waveModel.ModelDefinition.GetModelProperty(ModelDefinition.KnownWaveSections.OutputSection, 
                                                           ModelDefinition.KnownWaveProperties.NetCdfSinglePrecision).Value && 
                (bool) waveModel.ModelDefinition.GetModelProperty(ModelDefinition.KnownWaveSections.OutputSection, 
                                                                  ModelDefinition.KnownWaveProperties.MapWriteNetCDF).Value)
            {
                validationIssues.Add(new ValidationIssue(waveModel, ValidationSeverity.Warning,
                                                         Resources.WaveOutputParametersValidator_Validate_Enabling__Use_NetCDF_single_precision__might_lead_to_unexpected_behavior_when_inspecting_the_NetCDF_model_output_data_));
            }

            return new ValidationReport("Output parameters", validationIssues);
        }
    }
}
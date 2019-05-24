using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveCouplingValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            var issues = new List<ValidationIssue>();

            string comFilePath =
                model.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory,
                                                       KnownWaveProperties.COMFile).GetValueAsString();

            if (model.IsCoupledToFlow)
            {
                if (!model.WriteCOM || string.IsNullOrEmpty(comFilePath) || model.GetFlowComFilePath == null)
                {
                    issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                   Resources
                                                       .WaveCouplingValidator_Validate_Coupled_wave_model_must_use_COM_file,
                                                   model));
                }

                issues.AddRange(model.ValidateModelTimeSettings());
            }
            else
            {
                if (model.WriteCOM || !string.IsNullOrEmpty(comFilePath))
                {
                    issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                   Resources
                                                       .WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use_COM_file,
                                                   model));
                }

                IList<WaveDomainData> waveDomainDataObjects = WaveDomainHelper.GetAllDomains(model.OuterDomain);
                waveDomainDataObjects.ForEach(domain => issues.AddRange(ValidateWaveDomainData(domain)));
            }

            return new ValidationReport("Flow coupling", issues);
        }

        private static IEnumerable<ValidationIssue> ValidateModelTimeSettings(this WaveModel model)
        {
            if (model.StartTime < model.ModelDefinition.ModelReferenceDateTime)
            {
                var waveValidationShortcut = new WaveValidationShortcut
                {
                    WaveModel = model,
                    TabName = "General"
                };
                yield return new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                 Resources
                                                     .WaveTimePointValidator_Validate_Model_start_time_precedes_reference_time,
                                                 waveValidationShortcut);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateWaveDomainData(WaveDomainData waveDomainData)
        {
            if (waveDomainData.HydroFromFlowData.BedLevelUsage != UsageFromFlowType.DoNotUse)
            {
                yield return new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use__0_,
                                                     "flow bed level"), waveDomainData);
            }

            if (waveDomainData.HydroFromFlowData.WaterLevelUsage != UsageFromFlowType.DoNotUse)
            {
                yield return new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use__0_,
                                                     "flow water level"), waveDomainData);
            }

            if (waveDomainData.HydroFromFlowData.VelocityUsage != UsageFromFlowType.DoNotUse)
            {
                yield return new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use__0_,
                                                     "flow velocities"), waveDomainData);
            }

            if (waveDomainData.HydroFromFlowData.WindUsage != UsageFromFlowType.DoNotUse)
            {
                yield return new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use__0_,
                                                     "flow wind"), waveDomainData);
            }
        }
    }
}
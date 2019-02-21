using System.Collections.Generic;
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

            var comFilePath =
                    model.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory,
                        KnownWaveProperties.COMFile).GetValueAsString();

            if (model.IsCoupledToFlow)
            {
                if (!model.WriteCOM || string.IsNullOrEmpty(comFilePath) || model.GetFlowComFilePath == null)
                {
                    issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                        "Coupled wave model must use COM-file", model));
                }

                if (model.ModelDefinition.ModelReferenceDateTime < model.StartTime)
                {
                    issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error, Resources.WaveTimePointValidator_Validate_Model_start_time_precedes_reference_time, model));
                }
            }
            else
            {
                if (model.WriteCOM || !string.IsNullOrEmpty(comFilePath))
                {
                    issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                        "Stand-alone wave model cannot use COM-file", model));
                }

                foreach (var waveDomainData in WaveDomainHelper.GetAllDomains(model.OuterDomain))
                {
                    if (waveDomainData.HydroFromFlowData.BedLevelUsage != UsageFromFlowType.DoNotUse)
                    {
                        issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                            "Stand-alone wave model cannot use flow bed level", waveDomainData));
                    }

                    if (waveDomainData.HydroFromFlowData.WaterLevelUsage != UsageFromFlowType.DoNotUse)
                    {
                        issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                            "Stand-alone wave model cannot use flow water level", waveDomainData));
                    }

                    if (waveDomainData.HydroFromFlowData.VelocityUsage != UsageFromFlowType.DoNotUse)
                    {
                        issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                            "Stand-alone wave model cannot use flow velocities", waveDomainData));
                    }

                    if (waveDomainData.HydroFromFlowData.WindUsage != UsageFromFlowType.DoNotUse)
                    {
                        issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                            "Stand-alone wave model cannot use flow wind", waveDomainData));
                    }
                }
            }

            return new ValidationReport("Flow coupling", issues);
        }
    }
}

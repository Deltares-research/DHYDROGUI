using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    /// <summary>
    /// Validator for model coupling settings of <see cref="WaveModel"/> objects.
    /// </summary>
    public static class WaveCouplingValidator
    {
        /// <summary>
        /// Validates the coupling settings of a <see cref="WaveModel"/>.
        /// </summary>
        /// <param name="model"> The wave model to validate. </param>
        /// <returns> A collection of validation issues encountered. </returns>
        public static ValidationReport Validate(WaveModel model)
        {
            var issues = new List<ValidationIssue>();

            string comFilePath = model.ModelDefinition.CommunicationsFilePath;
            if (model.IsCoupledToFlow)
            {
                ValidateOnlineCoupledWavesModel(model, comFilePath, issues);
            }
            else
            {
                ValidateStandAloneWavesModel(model, comFilePath, issues);
            }

            return new ValidationReport("Flow coupling", issues);
        }

        private static void ValidateOnlineCoupledWavesModel(WaveModel model, string comFilePath, List<ValidationIssue> issues)
        {
            if (!model.WriteCOM || string.IsNullOrEmpty(comFilePath))
            {
                issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                                               Resources
                                                   .WaveCouplingValidator_Validate_Coupled_wave_model_must_use_COM_file,
                                               model));
            }

            issues.AddRange(model.ValidateModelTimeSettings());
        }

        private static void ValidateStandAloneWavesModel(WaveModel model, string comFilePath, List<ValidationIssue> issues)
        {
            if (!string.IsNullOrEmpty(comFilePath))
            {
                ValidateStandAloneWavesModelWithComFile(model, comFilePath, issues);
            }

            if (model.WriteCOM)
            {
                issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                                               Resources
                                                   .WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_write_COM_file,
                                               model));
            }
        }

        private static void ValidateStandAloneWavesModelWithComFile(WaveModel model, string comFilePath,
                                                                    List<ValidationIssue> issues)
        {
            string absoluteComFilePath;
            if (FileUtils.PathIsRelative(comFilePath))
            {
                string modelDirectoryPath = Directory.GetParent(model.MdwFilePath).FullName;
                absoluteComFilePath = Path.GetFullPath(Path.Combine(modelDirectoryPath, comFilePath));
            }
            else
            {
                absoluteComFilePath = comFilePath;
            }

            if (!File.Exists(absoluteComFilePath))
            {
                issues.Add(new ValidationIssue("Coupling", ValidationSeverity.Error,
                                               string.Format(Resources
                                                                 .WaveCouplingValidator_Validate_Communications_file___0___does_not_exist_,
                                                             comFilePath)));
            }
        }

        private static IEnumerable<ValidationIssue> ValidateModelTimeSettings(this WaveModel model)
        {
            var waveValidationShortcut = new WaveValidationShortcut
            {
                WaveModel = model,
                TabName = "General"
            };
            if (model.StartTime < model.ModelDefinition.ModelReferenceDateTime)
            {
                yield return new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                 Resources
                                                     .WaveTimePointValidator_Validate_Model_start_time_precedes_reference_time,
                                                 waveValidationShortcut);
            }

            if (model.TimeStep.TotalSeconds <= 0)
            {
                yield return new ValidationIssue("Time Step", ValidationSeverity.Error,
                                                 Resources.WaveCouplingValidator_ValidateModelTimeSettings_Time_step_cannot_be_set_to_Zero_,
                                                 waveValidationShortcut
                );
            }

            if (model.StopTime <= model.StartTime)
            {
                yield return new ValidationIssue("Coupling", ValidationSeverity.Error,
                                                 Resources.WaveCouplingValidator_ValidateModelTimeSettings_start_time_must_be_smaller_than_stop_time_,
                                                 waveValidationShortcut);
            }
        }
    }
}
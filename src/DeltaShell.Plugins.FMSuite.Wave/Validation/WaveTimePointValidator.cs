using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveTimePointValidator
    {
        private static List<ValidationIssue> issues;
        private static IList<DateTime> timePoints;
        private static WaveModel waveModel;

        /// <summary>
        /// Validation for wave model time point editing.
        /// <param name="model"> A wave model entity </param>
        /// <returns> A validation report regarding wave model time points </returns>
        /// </summary>
        public static ValidationReport Validate(WaveModel model)
        {
            waveModel = model;
            issues = new List<ValidationIssue>();
            timePoints = model.TimeFrameData.TimePoints.ToList();

            ValidateReferenceTime();
            ValidateTimePoints();

            return new ValidationReport("Waves Model Time Points", issues);
        }

        private static void ValidateReferenceTime()
        {
            if (timePoints.Count > 0)
            {
                bool hasInvalidTimePoint = timePoints.Any(tp => tp < waveModel.ModelDefinition.ModelReferenceDateTime);

                if (hasInvalidTimePoint)
                {
                    issues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                   Resources.WaveTimePointValidator_Validate_Model_start_time_precedes_reference_time,
                                                   waveModel.TimeFrameData));
                }
            }
        }

        private static void ValidateTimePoints()
        {
            if (!waveModel.IsCoupledToFlow && timePoints.Count == 0)
            {
                issues.Add(new ValidationIssue(waveModel, ValidationSeverity.Error,
                                               Resources.WaveTimePointValidator_Validate_No_time_points_defined,
                                               waveModel.TimeFrameData));
            }
        }
    }
}
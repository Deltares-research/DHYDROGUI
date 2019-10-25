using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
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
            timePoints = model.TimePointData.TimePoints;

            ValidateReferenceTime();
            ValidateBoundaryConditionTimePoints();
            ValidateTimePoints();
            

            return new ValidationReport("Waves Model Time Points", issues);
        }

        private static void ValidateBoundaryConditionTimePoints()
        {
            List<WaveBoundaryCondition> boundaryConditionWithParameterizedSpectrumTimeSeries =
                waveModel.BoundaryConditions
                         .Where(bc => bc.DataType == BoundaryConditionDataType.ParameterizedSpectrumTimeseries &&
                                      bc.PointData.SelectMany(b => b.Arguments[0].GetValues<DateTime>())
                                        .Any())
                         .ToList();

            if (boundaryConditionWithParameterizedSpectrumTimeSeries.Count == 0)
            {
                return;
            }

            foreach (WaveBoundaryCondition bc in boundaryConditionWithParameterizedSpectrumTimeSeries)
            {
                IEventedList<IFunction> boundaryConditionPointData = bc.PointData;
                List<DateTime> boundaryConditionTimePoints = boundaryConditionPointData
                                                             .SelectMany(b => b.Arguments[0].GetValues<DateTime>())
                                                             .ToList();
                bool allTimePointsPrecedeModelStartTime =
                    boundaryConditionTimePoints.All(b => b.Date < timePoints.FirstOrDefault());

                if (allTimePointsPrecedeModelStartTime)
                {
                    issues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                   string.Join(
                                                       " ",
                                                       $"{Resources.WaveTimePointValidator_BoundaryConditionTimePointsPrecedesModelStartTime_Model_start_time_does_not_precede_any_of_Boundary_Condition_time_points_}",
                                                       $"{bc.Name}"),
                                                   waveModel));
                }
            }
        }

        private static void ValidateReferenceTime()
        {
            if (timePoints.Count > 0)
            {
                bool hasInvalidTimePoint = timePoints.Any(tp => tp < waveModel.ModelDefinition.ModelReferenceDateTime);

                if (hasInvalidTimePoint)
                {
                    issues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                   Resources
                                                       .WaveTimePointValidator_Validate_Model_start_time_precedes_reference_time,
                                                   waveModel.TimePointData));
                }
            }
        }

        private static void ValidateTimePoints()
        {
            if (!waveModel.IsCoupledToFlow && timePoints.Count == 0)
            {
                issues.Add(new ValidationIssue(waveModel, ValidationSeverity.Error,
                                               Resources.WaveTimePointValidator_Validate_No_time_points_defined,
                                               waveModel.TimePointData));
            }
        }
    }
}
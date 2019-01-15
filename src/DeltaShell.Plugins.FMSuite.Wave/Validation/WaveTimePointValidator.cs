using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="model">A wave model entity</param>
        /// <returns>A validation report regarding wave model time points</returns>
        /// </summary>
        public static ValidationReport Validate(WaveModel model)
        {
            waveModel = model;
            issues = new List<ValidationIssue>();
            timePoints = model.TimePointData.TimePoints;

            ValidateBoundaryConditionTimePoints();
            ValidateTimePoints();
            ValidateReferenceTime();

            return new ValidationReport("Waves Model Time Points", issues);
        }

        private static void ValidateBoundaryConditionTimePoints()
        {
            var boundaryConditionWithParameterizedSpectrumTimeSeries = waveModel.BoundaryConditions.Where(bc =>
                bc.DataType == BoundaryConditionDataType.ParameterizedSpectrumTimeseries).ToList();

            if (boundaryConditionWithParameterizedSpectrumTimeSeries.Count == 0) return;

            var boundaryConditionPointData = boundaryConditionWithParameterizedSpectrumTimeSeries.SelectMany(bc => bc.PointData).ToList();
            var boundaryConditionTimePoints = boundaryConditionPointData.SelectMany(b => b.Arguments[0].GetValues<DateTime>().ToList());
            var allTimePointsPrecedeModelStartTime = boundaryConditionTimePoints.All(b => b.Date < timePoints.FirstOrDefault());

            if (allTimePointsPrecedeModelStartTime)
            {
                issues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                    Resources.WaveTimePointValidator_BoundaryConditionTimePointsPrecedesModelStartTime_Model_start_time_does_not_precede_any_of_Boundary_Condition_time_points_,
                    waveModel.TimePointData));
            }
        }

        private static void ValidateReferenceTime()
        {
            if (timePoints.Count > 0)
            {
                var hasInvalidTimePoint = timePoints.Any(tp => tp < waveModel.ModelDefinition.ModelReferenceDateTime);

                if (hasInvalidTimePoint)
                {
                    issues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                               Resources.WaveTimePointValidator_Validate_Model_Start_time_precedes_Reference_Time,
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
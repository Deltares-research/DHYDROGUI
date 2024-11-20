using System;
using System.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.Properties;

namespace DeltaShell.NGHS.Common.Validation
{
    /// <summary>
    /// Validator for validating the write restart settings.
    /// </summary>
    public static class RestartTimeRangeValidator
    {
        /// <summary>
        /// Method for validating the write restart settings.
        /// </summary>
        /// <param name="writeRestart"> Setting if writing restart files should be done during a run. </param>
        /// <param name="restartStartTime"> The restart start time. </param>
        /// <param name="restartStopTime"> The restart stop time. </param>
        /// <param name="restartTimeStep"> The restart time step</param>
        /// <param name="modelStartTime"> The model start time. </param>
        /// <param name="modelTimeStep"> The model time step.</param>
        /// <param name="viewData">
        /// The view data which should be shown if there is a validation error.
        /// </param>
        /// <returns> Validation report of the write restart time settings</returns>
        public static ValidationReport ValidateWriteRestartSettings(bool writeRestart, DateTime restartStartTime, DateTime restartStopTime, TimeSpan restartTimeStep,
                                                                    DateTime modelStartTime, TimeSpan modelTimeStep, object viewData = null)
        {
            var issues = new List<ValidationIssue>();

            if (!writeRestart)
            {
                return new ValidationReport(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_range_settings, issues);
            }

            var modelTimeStepSeconds = (long) modelTimeStep.TotalSeconds;

            ValidateRestartTimeStep(modelTimeStepSeconds, restartTimeStep, issues, viewData);

            ValidateRestartStopTimeIsNotBeforeRestartStartTime(restartStartTime, restartStopTime, issues, viewData);

            ValidateRestartStartTime(restartStartTime, modelStartTime, modelTimeStepSeconds, issues, viewData);

            ValidateRestartStopTime(restartStopTime, modelStartTime, modelTimeStepSeconds, issues, viewData);

            return new ValidationReport(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_range_settings, issues);
        }

        private static void ValidateRestartStopTime(DateTime restartStopTime, DateTime modelStartTime, long modelTimeStepSeconds,
                                                    ICollection<ValidationIssue> issues, object viewData)
        {
            if (restartStopTime < modelStartTime ||
                restartStopTime > modelStartTime && modelTimeStepSeconds > 0 && (long) (restartStopTime - modelStartTime).TotalSeconds % modelTimeStepSeconds != 0)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_stop_time, ValidationSeverity.Error,
                                               Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_stop_time_must_be_expressed_by_model_start_time_plus_a_positive_integer_multiple_of_the_model_time_step_, viewData));
            }
        }

        private static void ValidateRestartStartTime(DateTime restartStartTime, DateTime modelStartTime, long modelTimeStepSeconds,
                                                     ICollection<ValidationIssue> issues, object viewData)
        {
            if (restartStartTime < modelStartTime ||
                restartStartTime > modelStartTime && modelTimeStepSeconds > 0 && (long) (restartStartTime - modelStartTime).TotalSeconds % modelTimeStepSeconds != 0)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_start_time, ValidationSeverity.Error,
                                               Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_start_time_must_be_expressed_by_model_start_time_plus_a_positive_integer_multiple_of_the_model_time_step_, viewData));
            }
        }

        private static void ValidateRestartTimeStep(long modelTimeStepSeconds, TimeSpan restartTimeStep, ICollection<ValidationIssue> issues, object viewData)
        {
            if (restartTimeStep.TotalSeconds <= 0)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_step, ValidationSeverity.Error,
                                               Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_time_step_must_be_positive_value_, viewData));
                return;
            }

            var restartTimeStepSeconds = (long) restartTimeStep.TotalSeconds;

            if (modelTimeStepSeconds > 0 && restartTimeStepSeconds % modelTimeStepSeconds != 0)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_step, ValidationSeverity.Error,
                                               Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_time_step_must_be_an_integer_multiple_of_the_output_time_step_, viewData));
            }
        }

        private static void ValidateRestartStopTimeIsNotBeforeRestartStartTime(DateTime restartStartTime, DateTime restartStopTime, ICollection<ValidationIssue> issues, object viewData)
        {
            if (restartStopTime < restartStartTime)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_interval, ValidationSeverity.Error,
                                               Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_stop_time_cannot_be_before_restart_start_time_, viewData));
            }
        }
    }
}
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
        /// <param name="modelStopTime"> The model stop time. </param>
        /// <param name="modelTimeStep"> The model time step.</param>
        /// <returns></returns>
        public static ValidationReport ValidateWriteRestartSettings(bool writeRestart, DateTime restartStartTime, DateTime restartStopTime, TimeSpan restartTimeStep,
                                                                        DateTime modelStartTime, DateTime modelStopTime, TimeSpan modelTimeStep)
        {
            var issues = new List<ValidationIssue>();
            
            if (!writeRestart) return new ValidationReport(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_range_settings, issues);
            
            if (restartTimeStep.TotalSeconds <= 0)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Save_state_time_step, ValidationSeverity.Error,
                    Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_save_state_time_step_must_be_positive_value_));
            }

            if (restartStopTime < restartStartTime)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Save_state_interval, ValidationSeverity.Error,
                    Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_save_state_interval_stop_time_cannot_be_before_start_time_));
            }

            var modelTimeStepSeconds = (long)modelTimeStep.TotalSeconds;
            var restartTimeStepSeconds = (long)restartTimeStep.TotalSeconds;
            if (modelTimeStepSeconds > 0 && restartTimeStepSeconds % modelTimeStepSeconds != 0)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Save_state_time_step, ValidationSeverity.Error,
                    Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_save_state_time_step_must_be_an_integer_multiple_of_the_output_time_step_));
            }

            if (restartStartTime < modelStartTime||
                restartStartTime > modelStopTime ||
                restartStartTime > modelStartTime && modelTimeStepSeconds > 0 && (long)(restartStartTime - modelStartTime).TotalSeconds % modelTimeStepSeconds != 0)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Save_state_start_time, ValidationSeverity.Error,
                    Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_save_state_start_time_must_be_expressed_by_start_time_plus_an_positive_integer_multiple_of_the_model_time_step_and_before_stop_time_));
            }

            if (restartStopTime > modelStopTime ||
                restartStopTime < modelStartTime ||
                restartStopTime < modelStopTime && restartTimeStepSeconds > 0 && (long)(restartStopTime - restartStartTime).TotalSeconds % modelTimeStepSeconds != 0)
            {
                issues.Add(new ValidationIssue(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Save_state_stop_time, ValidationSeverity.Error,
                    Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_save_state_stop_time_must_be_expressed_by_start_time_plus_an_positive_integer_multiple_of_the_model_time_step_and_not_after_stop_time_));
            }

            return new ValidationReport(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_range_settings, issues);
        }
    }
}
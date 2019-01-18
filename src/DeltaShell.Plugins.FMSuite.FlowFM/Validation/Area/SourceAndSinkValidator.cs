using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    /// <summary>
    /// Class that is responsible for validating sources and sinks in Flow FM models.
    /// </summary>
    public static class SourceAndSinkValidator
    {
        /// <summary>
        /// Validate the source and sinks and return any issues encountered.
        /// </summary>
        /// <param name="model">The model to which the source and sinks belong.</param>
        /// <param name="sourcesAndSinks">The the set of sources and sinks to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(WaterFlowFMModel model, IEnumerable<SourceAndSink> sourcesAndSinks)
        {
            var issues = new List<ValidationIssue>();
            foreach (var sourceAndSink in sourcesAndSinks)
            {
                issues.AddRange(sourceAndSink.SnapsToModelGrid(model));
                issues.AddRange(sourceAndSink.ValidateTimeArgument(model.StartTime, model.StopTime));
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateTimeArgument(this SourceAndSink sourceAndSink, DateTime modelStartTime, DateTime modelStopTime)
        {
            var timeArgument = sourceAndSink.Function.Arguments.OfType<IVariable<DateTime>>().First();
            if (timeArgument.Values.Any())
            {
                foreach (var validationIssue in sourceAndSink.ValidateTimeArgumentValues(timeArgument, modelStartTime, modelStopTime))
                {
                    yield return validationIssue;
                }
            }
            else
            {
                yield return new ValidationIssue(sourceAndSink, ValidationSeverity.Error,
                    string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0____discharge_time_series_does_not_contain_any_values_, sourceAndSink.Name),
                    sourceAndSink);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateTimeArgumentValues(this SourceAndSink sourceAndSink, IVariable<DateTime> timeArgument, DateTime modelStartTime, DateTime modelStopTime)
        {
            var startTime = timeArgument.Values.First();
            var stopTime = timeArgument.Values.Last();

            if (startTime > modelStartTime || stopTime < modelStopTime)
            {
                yield return new ValidationIssue(sourceAndSink, ValidationSeverity.Error,
                    string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0____discharge_time_series_does_not_span_the_model_run_interval_, sourceAndSink.Name),
                    sourceAndSink);
            }
        }

        private static IEnumerable<ValidationIssue> SnapsToModelGrid(this SourceAndSink sourceAndSink, WaterFlowFMModel model)
        {
            if (!model.SnapsToGrid(sourceAndSink.Feature.Geometry))
            {
                yield return new ValidationIssue(sourceAndSink, ValidationSeverity.Warning,
                    string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0___not_within_grid_extent, sourceAndSink.Name), 
                    sourceAndSink);
            }
        }
    }
}

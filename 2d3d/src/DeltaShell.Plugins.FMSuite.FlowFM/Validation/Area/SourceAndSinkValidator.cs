using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;

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
        /// <param name="sourcesAndSinks"> The the set of sources and sinks to be evaluated. </param>
        /// <param name="gridExtent"> The <see cref="Envelope"/> object that describes the extent of the FM model grid. </param>
        /// <param name="modelStartTime"> The model start time. </param>
        /// <param name="modelStopTime"> The model stop time. </param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<SourceAndSink> sourcesAndSinks,
                                                            Envelope gridExtent, DateTime modelStartTime,
                                                            DateTime modelStopTime)
        {
            var issues = new List<ValidationIssue>();
            foreach (SourceAndSink sourceAndSink in sourcesAndSinks)
            {
                issues.AddRange(sourceAndSink.SnapsToModelGrid(gridExtent));
                issues.AddRange(sourceAndSink.ValidateTimeArgument(modelStartTime, modelStopTime));
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> SnapsToModelGrid(this SourceAndSink sourceAndSink,
                                                                     Envelope gridExtent)
        {
            if (!sourceAndSink.Feature.Geometry.SnapsToFlowFmGrid(gridExtent))
            {
                yield return new ValidationIssue(sourceAndSink, ValidationSeverity.Warning,
                                                 string.Format(
                                                     Resources
                                                         .SourceAndSinkValidator_Validate_source_sink___0___not_within_grid_extent,
                                                     sourceAndSink.Name),
                                                 sourceAndSink);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateTimeArgument(
            this SourceAndSink sourceAndSink, DateTime modelStartTime, DateTime modelStopTime)
        {
            IVariable<DateTime> timeArgument = sourceAndSink.Function.Arguments.OfType<IVariable<DateTime>>().First();
            if (timeArgument.Values.Any())
            {
                foreach (ValidationIssue validationIssue in sourceAndSink.ValidateTimeArgumentValues(
                    timeArgument, modelStartTime, modelStopTime))
                {
                    yield return validationIssue;
                }
            }
            else
            {
                yield return new ValidationIssue(sourceAndSink, ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .SourceAndSinkValidator_Validate_source_sink___0____discharge_time_series_does_not_contain_any_values_,
                                                     sourceAndSink.Name),
                                                 sourceAndSink);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateTimeArgumentValues(
            this SourceAndSink sourceAndSink, IVariable<DateTime> timeArgument, DateTime modelStartTime,
            DateTime modelStopTime)
        {
            DateTime startTime = timeArgument.Values.First();
            DateTime stopTime = timeArgument.Values.Last();

            if (startTime > modelStartTime || stopTime < modelStopTime)
            {
                yield return new ValidationIssue(sourceAndSink, ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .SourceAndSinkValidator_Validate_source_sink___0____discharge_time_series_does_not_span_the_model_run_interval_,
                                                     sourceAndSink.Name),
                                                 sourceAndSink);
            }
        }
    }
}
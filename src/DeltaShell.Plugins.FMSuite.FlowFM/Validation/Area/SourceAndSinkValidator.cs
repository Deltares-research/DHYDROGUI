using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class SourceAndSinkValidator
    {
        /// <summary>
        /// Validate the source and sinks and return any issues encountered.
        /// </summary>
        /// <param name="model">The model to which the source and sinks belong.</param>
        /// <param name="sourcesAndSinks">The the set of sources and sinks to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(WaterFlowFMModel model, IEnumerable<FeatureData.SourceAndSink> sourcesAndSinks)
        {
            var issues = new List<ValidationIssue>();
            foreach (var sourceAndSink in sourcesAndSinks)
            {
                if (!model.SnapsToGrid(sourceAndSink.Feature.Geometry))
                {
                    issues.Add(new ValidationIssue(sourceAndSink,
                                                   ValidationSeverity.Warning,
                                                   $"source/sink '{sourceAndSink.Name}' not within grid extent",
                                                   model.Pipes));
                }

                var timeArgument = sourceAndSink
                                   .Function.Arguments.OfType<IVariable<DateTime>>()
                                   .First();
                if (timeArgument.Values.Any())
                {
                    var startTime = timeArgument.Values.First();
                    var stopTime = timeArgument.Values.Last();

                    if (startTime > model.StartTime || stopTime < model.StopTime)
                    {
                        issues.Add(new ValidationIssue(sourceAndSink,
                                                       ValidationSeverity.Error,
                                                       $"source/sink '{sourceAndSink.Name}': discharge time series does not span the model run interval.",
                                                       sourceAndSink));
                    }
                }
                else
                {
                    issues.Add(new ValidationIssue(sourceAndSink,
                                                   ValidationSeverity.Error,
                                                   $"source/sink '{sourceAndSink.Name}': discharge time series does not contain any values.",
                                                   sourceAndSink));
                }
            }

            return issues;
        }
    }
}

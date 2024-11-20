using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using GeoAPI.Extensions.Feature;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Validation
{
    public class ControlGroupValidator : IValidator<IRealTimeControlModel, IControlGroup>
    {
        /// <summary>
        /// Validate the provided control group.
        /// </summary>
        /// <param name="rootObject"> The real-time control model to which the control group belongs. </param>
        /// <param name="target"> The control group to validate. </param>
        /// <returns>
        /// A new <see cref="ValidationResult"/> containing the validation results.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="rootObject"/> or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public ValidationReport Validate(IRealTimeControlModel rootObject, IControlGroup target)
        {
            Ensure.NotNull(rootObject, nameof(rootObject));
            Ensure.NotNull(target, nameof(target));

            return new ValidationReport(target.Name + " (Control Group)", new[]
            {
                ValidateControlGroup(rootObject, target),
                ValidateRules(rootObject, target),
                ValidateConditions(rootObject, target),
                ValidateSignals(target)
            });
        }

        private static ValidationReport ValidateRules(IRealTimeControlModel model, IControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            RtcBaseObjectCheckForUniqueness(controlGroup.Rules, issues, "Rule");

            issues.AddRange(ValidateTimeSeriesInRules(model, controlGroup));

            foreach (RuleBase rule in controlGroup.Rules.Where(r => r.IsLinkedFromSignal()))
            {
                if (!controlGroup.Signals.Any(s => s.RuleBases.Contains(rule)))
                {
                    issues.Add(new ValidationIssue(rule, ValidationSeverity.Error, "Rule is configured to be receiving input from a signal, but none is connected.", controlGroup));
                }
            }

            return new ValidationReport("Rules", issues);
        }

        private static ValidationReport ValidateConditions(IRealTimeControlModel model, IControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            RtcBaseObjectCheckForUniqueness(controlGroup.Conditions, issues, "Condition");

            issues.AddRange(ValidateTimeSeriesInConditions(model, controlGroup));

            return new ValidationReport("Conditions", issues);
        }

        private static ValidationReport ValidateSignals(IControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            RtcBaseObjectCheckForUniqueness(controlGroup.Signals, issues, "Signals");

            foreach (SignalBase signal in controlGroup.Signals)
            {
                foreach (RuleBase ruleBase in signal.RuleBases)
                {
                    if (!ruleBase.IsLinkedFromSignal())
                    {
                        issues.Add(new ValidationIssue(ruleBase, ValidationSeverity.Warning, "Signal connected to rule, but its values will not be used according to the rule settings.", controlGroup));
                    }
                }
            }

            return new ValidationReport("Signals", issues);
        }

        private static ValidationReport ValidateControlGroup(IRealTimeControlModel realTimeControlModel, IControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            if (!controlGroup.Rules.Any())
            {
                issues.Add(new ValidationIssue(controlGroup, ValidationSeverity.Error, "Control Group requires at least 1 rule"));
            }

            if (!controlGroup.Outputs.Any())
            {
                issues.Add(new ValidationIssue(controlGroup, ValidationSeverity.Error, "Control Group requires at least 1 output"));
            }

            // PostSharp validation:
            ValidationResult result = controlGroup.Validate();
            if (!result.IsValid)
            {
                issues.AddRange(result.Messages.Select(m => new ValidationIssue(controlGroup, ValidationSeverity.Error, m)));
            }

            ValidateConnectionPointsWithControlledModels(realTimeControlModel, controlGroup, issues);

            return new ValidationReport("Control group configuration", issues);
        }

        private static void ValidateConnectionPointsWithControlledModels(IRealTimeControlModel realTimeControlModel, IControlGroup controlGroup, List<ValidationIssue> issues)
        {
            ValidateUncontrollableInputs(realTimeControlModel, controlGroup, issues);
            ValidateUncontrollableOutputs(realTimeControlModel, controlGroup, issues);
        }

        private static void ValidateUncontrollableOutputs(IRealTimeControlModel realTimeControlModel, IControlGroup controlGroup, List<ValidationIssue> issues)
        {
            foreach (IFeature uncontrollableOutput in GetUncontrollableFeatures(realTimeControlModel, controlGroup.Outputs, DataItemRole.Input))
            {
                string message = string.Format(Resources.Feature_0_cannot_be_used_as_output_for_control_group_1_, uncontrollableOutput, controlGroup.Name);
                issues.Add(new ValidationIssue(controlGroup, ValidationSeverity.Error, message));
            }
        }

        private static void ValidateUncontrollableInputs(IRealTimeControlModel realTimeControlModel, IControlGroup controlGroup, List<ValidationIssue> issues)
        {
            foreach (IFeature uncontrollableInput in GetUncontrollableFeatures(realTimeControlModel, controlGroup.Inputs, DataItemRole.Output))
            {
                string message = string.Format(Resources.Feature_0_cannot_be_used_as_input_for_control_group_1_, uncontrollableInput, controlGroup.Name);
                issues.Add(new ValidationIssue(controlGroup, ValidationSeverity.Error, message));
            }
        }

        private static IEnumerable<IFeature> GetUncontrollableFeatures(IRealTimeControlModel realTimeControlModel, IEnumerable<ConnectionPoint> connectionPoints, DataItemRole dataItemRole)
        {
            if (!connectionPoints.Any())
            {
                yield break;
            }

            HashSet<IFeature> controllableFeatures = realTimeControlModel.ControlledModels
                                                                         .SelectMany(model => model.GetChildDataItemLocations(dataItemRole))
                                                                         .ToHashSet();
            
            foreach (IFeature feature in connectionPoints.Select(p=> p.Feature))
            {
                if (!controllableFeatures.Contains(feature))
                {
                    yield return feature;
                }
            }
        }

        private static void RtcBaseObjectCheckForUniqueness(IEnumerable<INameable> nameables,
                                                            IList<ValidationIssue> issueList, string typeObject)
        {
            var ruleNames = new HashSet<string>();
            foreach (INameable nameable in nameables)
            {
                if (!ruleNames.Add(nameable.Name))
                {
                    issueList.Add(new ValidationIssue(nameable,
                                                      ValidationSeverity.Error,
                                                      $"The name '{nameable.Name}' is used by {nameables.Count(bo => bo.Name == nameable.Name)} {typeObject}s."));
                }
            }
        }

        private static IEnumerable<ValidationIssue> ValidateTimeSeriesInConditions(IRealTimeControlModel rootObject, IControlGroup controlGroup)
        {
            IEnumerable<TimeCondition> invalidTimeConditions = controlGroup.Conditions
                                                                           .OfType<TimeCondition>()
                                                                           .Where(tc => !IsTimeSeriesMultipleOfModelTimeSteps(tc.TimeSeries, rootObject.StartTime, rootObject.TimeStep));

            return invalidTimeConditions.Select(tc => new ValidationIssue(tc, ValidationSeverity.Error, Resources.ControlGroupValidator_TimeSeriesNotAMultipleOfModelTimeStep, tc.TimeSeries));
        }

        private static IEnumerable<ValidationIssue> ValidateTimeSeriesInRules(IRealTimeControlModel rootObject, IControlGroup controlGroup)
        {
            List<ITimeDependentRtcObject> timeDependentRules = controlGroup.Rules
                                                                           .OfType<ITimeDependentRtcObject>()
                                                                           .ToList();

            IEnumerable<ITimeDependentRtcObject> invalidTimeStepRules = timeDependentRules.Where(r => !IsTimeSeriesMultipleOfModelTimeSteps(r.TimeSeries, rootObject.StartTime, rootObject.TimeStep));
            IEnumerable<ValidationIssue> timeStepIssues = invalidTimeStepRules.Select(r => new ValidationIssue(r, ValidationSeverity.Error, Resources.ControlGroupValidator_TimeSeriesNotAMultipleOfModelTimeStep, r.TimeSeries));

            IEnumerable<ITimeDependentRtcObject> invalidTimeSpanRules = timeDependentRules.Where(r => !IsTimeSeriesSpanningModelRunInterval(r.TimeSeries, rootObject.StartTime, rootObject.StopTime));
            IEnumerable<ValidationIssue> timeSpanIssues = invalidTimeSpanRules.Select(r => new ValidationIssue(r, ValidationSeverity.Error, Resources.ControlGroupValidator_TimeSeriesDoesNotSpanModelRunInterval, r.TimeSeries));

            return timeStepIssues.Concat(timeSpanIssues);
        }

        private static bool IsTimeSeriesMultipleOfModelTimeSteps(ITimeSeries timeSeries, DateTime startTime, TimeSpan timeStep)
        {
            if (timeStep.Equals(TimeSpan.Zero))
            {
                return false;
            }

            if (timeSeries.Time.Values.Count == 0)
            {
                return true;
            }

            return timeSeries.Time.Values.All(t => (t - startTime).Ticks % timeStep.Ticks == 0);
        }

        private static bool IsTimeSeriesSpanningModelRunInterval(ITimeSeries timeSeries, DateTime startTime, DateTime stopTime)
        {
            IMultiDimensionalArray<DateTime> timeValues = timeSeries.Time.Values;

            if (timeValues.Count == 0)
            {
                return true;
            }

            return timeValues[0] <= startTime && timeValues[timeValues.Count-1] >= stopTime;
        }
    }
}
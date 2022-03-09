using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Validation
{
    public class ControlGroupValidator : IValidator<RealTimeControlModel, ControlGroup>
    {
        public ValidationReport Validate(RealTimeControlModel rootObject, ControlGroup target)
        {
            return new ValidationReport(target.Name + " (Control Group)", new[]
            {
                ValidateControlGroup(target),
                ValidateRules(rootObject, target),
                ValidateConditions(rootObject, target),
                ValidateSignals(target)
            });
        }

        private static ValidationReport ValidateRules(RealTimeControlModel model, ControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            RtcBaseObjectCheckForUniqueness(controlGroup.Rules.Cast<INameable>(), issues, "Rule");
            if (model != null)
            {
                issues.AddRange(ValidateTimeSeriesInRules(model, controlGroup));
            }

            foreach (RuleBase rule in controlGroup.Rules.Where(r => r.IsLinkedFromSignal()))
            {
                if (!controlGroup.Signals.Any(s => s.RuleBases.Contains(rule)))
                {
                    issues.Add(new ValidationIssue(rule, ValidationSeverity.Error, "Rule is configured to be receiving input from a signal, but none is connected.", controlGroup));
                }
            }

            return new ValidationReport("Rules", issues);
        }

        private static ValidationReport ValidateConditions(RealTimeControlModel model, ControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            RtcBaseObjectCheckForUniqueness(controlGroup.Conditions.Cast<INameable>(), issues, "Condition");
            if (model != null)
            {
                issues.AddRange(ValidateTimeSeriesInConditions(model, controlGroup));
            }

            return new ValidationReport("Conditions", issues);
        }

        private static ValidationReport ValidateSignals(ControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            RtcBaseObjectCheckForUniqueness(controlGroup.Signals.Cast<INameable>(), issues, "Signals");

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

        private static ValidationReport ValidateControlGroup(ControlGroup controlGroup)
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

            return new ValidationReport("Control group configuration", issues);
        }

        private static void RtcBaseObjectCheckForUniqueness(IEnumerable<INameable> nameables,
                                                            IList<ValidationIssue> issueList, string typeObject)
        {
            var ruleNames = new HashSet<string>();
            foreach (INameable nameable in nameables)
            {
                if (ruleNames.Contains(nameable.Name))
                {
                    issueList.Add(new ValidationIssue(nameable,
                                                      ValidationSeverity.Error,
                                                      string.Format("The name '{0}' is used by {1} {2}s.",
                                                                    nameable.Name,
                                                                    nameables.Count(bo => bo.Name == nameable.Name),
                                                                    typeObject)));
                }
                else
                {
                    ruleNames.Add(nameable.Name);
                }
            }
        }

        private static IEnumerable<ValidationIssue> ValidateTimeSeriesInConditions(RealTimeControlModel rootObject, IControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            DateTime startTime = rootObject.StartTime;
            TimeSpan timeStep = rootObject.TimeStep;

            IEnumerable<TimeCondition> invalidTimeConditions = controlGroup.Conditions.OfType<TimeCondition>().Where(tc => !ValidateTimeSeries(tc.TimeSeries, startTime, timeStep));
            invalidTimeConditions.ForEach(tc => issues.Add(new ValidationIssue(tc, ValidationSeverity.Error,
                                                                               string.Format(Resources.RealTimeControlControlGroupValidator_SeriesTimesShouldMatchModelTimeStep,
                                                                                             tc.TimeSeries.Name, controlGroup.Name,
                                                                                             timeStep), tc.TimeSeries)));

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateTimeSeriesInRules(RealTimeControlModel rootObject, IControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            DateTime startTime = rootObject.StartTime;
            DateTime stopTime = rootObject.StopTime;
            TimeSpan timeStep = rootObject.TimeStep;

            List<PIDRule> pidRules = controlGroup.Rules.OfType<PIDRule>().ToList();
            List<TimeRule> timeRules = controlGroup.Rules.OfType<TimeRule>().ToList();
            List<IntervalRule> intervalRules = controlGroup.Rules.OfType<IntervalRule>().ToList();

            // Check if time steps of rule match with the model time step
            IEnumerable<PIDRule> invalidPidRules = pidRules.Where(r => r.PidRuleSetpointType == PIDRule.PIDRuleSetpointTypes.TimeSeries && !ValidateTimeSeries(r.TimeSeries, startTime, timeStep));
            IEnumerable<TimeRule> invalidTimeRules = timeRules.Where(r => !ValidateTimeSeries(r.TimeSeries, startTime, timeStep));
            IEnumerable<IntervalRule> invalidIntervalRules = intervalRules.Where(r => !ValidateTimeSeries(r.TimeSeries, startTime, timeStep));

            string validationString = Resources.RealTimeControlControlGroupValidator_SeriesTimesShouldMatchModelTimeStep;
            issues.AddRange(
                GetInvalidRulesIssues(controlGroup.Name,
                                      invalidPidRules, invalidTimeRules, invalidIntervalRules,
                                      ValidationSeverity.Error,
                                      validationString,
                                      timeStep.ToString()));

            // check start times
            invalidPidRules = pidRules.Where(r => r.PidRuleSetpointType == PIDRule.PIDRuleSetpointTypes.TimeSeries && TimeSeriesEntriesPrecedeModelStartTime(r.TimeSeries, startTime));
            invalidTimeRules = timeRules.Where(r => TimeSeriesEntriesPrecedeModelStartTime(r.TimeSeries, startTime));
            invalidIntervalRules = intervalRules.Where(r => TimeSeriesEntriesPrecedeModelStartTime(r.TimeSeries, startTime));

            issues.AddRange(
                GetInvalidRulesIssues(controlGroup.Name,
                                      invalidPidRules, invalidTimeRules, invalidIntervalRules,
                                      ValidationSeverity.Warning,
                                      Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime,
                                      startTime.ToString(CultureInfo.InvariantCulture)));

            // check end times
            invalidPidRules = pidRules.Where(r => r.PidRuleSetpointType == PIDRule.PIDRuleSetpointTypes.TimeSeries && TimeSeriesEntriesExceedModelStopTime(r.TimeSeries, stopTime));
            invalidTimeRules = timeRules.Where(r => TimeSeriesEntriesExceedModelStopTime(r.TimeSeries, stopTime));
            invalidIntervalRules = intervalRules.Where(r => TimeSeriesEntriesExceedModelStopTime(r.TimeSeries, stopTime));

            issues.AddRange(
                GetInvalidRulesIssues(controlGroup.Name,
                                      invalidPidRules, invalidTimeRules, invalidIntervalRules,
                                      ValidationSeverity.Warning,
                                      Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime,
                                      stopTime.ToString(CultureInfo.InvariantCulture)));

            return issues;
        }

        private static IEnumerable<ValidationIssue> GetInvalidRulesIssues(string controlGroupName, IEnumerable<PIDRule> pidRules, IEnumerable<TimeRule> timeRules, IEnumerable<IntervalRule> intervalRules, ValidationSeverity severity, string message, string messageArgument)
        {
            var issues = new List<ValidationIssue>();

            pidRules.ForEach(r => issues.Add(new ValidationIssue(r, severity, string.Format(message, r.TimeSeries.Name, controlGroupName, messageArgument), r.TimeSeries)));
            timeRules.ForEach(r => issues.Add(new ValidationIssue(r, severity, string.Format(message, r.TimeSeries.Name, controlGroupName, messageArgument), r.TimeSeries)));
            intervalRules.ForEach(r => issues.Add(new ValidationIssue(r, severity, string.Format(message, r.TimeSeries.Name, controlGroupName, messageArgument), r.TimeSeries)));

            return issues;
        }

        private static bool ValidateTimeSeries(ITimeSeries timeSeries, DateTime startTime, TimeSpan timeStep)
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

        private static bool TimeSeriesEntriesPrecedeModelStartTime(ITimeSeries timeSeries, DateTime startTime)
        {
            if (timeSeries.Time.Values.Count == 0)
            {
                return false;
            }

            return startTime > timeSeries.Time.Values.First();
        }

        private static bool TimeSeriesEntriesExceedModelStopTime(ITimeSeries timeSeries, DateTime stopTime)
        {
            if (timeSeries.Time.Values.Count == 0)
            {
                return false;
            }

            return stopTime < timeSeries.Time.Values.Last();
        }
    }
}
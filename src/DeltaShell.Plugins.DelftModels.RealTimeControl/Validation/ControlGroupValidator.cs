using System;
using System.Collections.Generic;
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
            return new ValidationReport(target.Name + " (Control Group)", new []
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

            foreach (var rule in controlGroup.Rules.Where(r => r.IsLinkedFromSignal()))
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

            foreach (var signal in controlGroup.Signals)
            {
                foreach (var ruleBase in signal.RuleBases)
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
            var result = controlGroup.Validate();
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
            foreach (var nameable in nameables)
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

            var startTime = rootObject.StartTime;
            var timeStep = rootObject.TimeStep;
            const string validationString = "Series '{0}' time steps not multiple of model time step {1}.";
            
            var invalidTimeConditions = controlGroup.Conditions.OfType<TimeCondition>().Where(tc => !ValidateTimeSeries(tc.TimeSeries, startTime, timeStep));
            invalidTimeConditions.ForEach(tc => issues.Add(new ValidationIssue(tc, ValidationSeverity.Error,
                                                                               String.Format(validationString,
                                                                                             tc.TimeSeries.Name,
                                                                                             timeStep), tc.TimeSeries)));

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateTimeSeriesInRules(RealTimeControlModel rootObject, IControlGroup controlGroup)
        {
            var issues = new List<ValidationIssue>();

            var startTime = rootObject.StartTime;
            var stopTime = rootObject.StopTime;
            var timeStep = rootObject.TimeStep;
            string validationString = "Series '{0}' time steps not multiple of model time step {1}.";

            var invalidPidRules = controlGroup.Rules.OfType<PIDRule>().Where(r => r.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.TimeSeries &&
                                                                            !ValidateTimeSeries(r.TimeSeries, startTime, timeStep));
            invalidPidRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Error,
                                               String.Format(validationString, r.TimeSeries.Name, timeStep), r.TimeSeries)));

            var invalidTimeRules = controlGroup.Rules.OfType<TimeRule>().Where(r => !ValidateTimeSeries(r.TimeSeries, startTime, timeStep));
            invalidTimeRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Error,
                                                                        String.Format(validationString, r.TimeSeries.Name, timeStep), r.TimeSeries)));

            var invalidIntervalRules = controlGroup.Rules.OfType<IntervalRule>().Where(r => r.IntervalType == IntervalRule.IntervalRuleIntervalType.Variable && 
                                                                                      !ValidateTimeSeries(r.TimeSeries, startTime, timeStep));
            invalidIntervalRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Error,
                                                                        String.Format(validationString, r.TimeSeries.Name, timeStep), r.TimeSeries)));

            // check start times
            var pidRules = controlGroup.Rules.OfType<PIDRule>().Where(r => r.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.TimeSeries &&
                                                                                  TimeSeriesEntriesPrecedeModelStartTime(r.TimeSeries, startTime));
            pidRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Warning,
                String.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, r.TimeSeries.Name, startTime), r.TimeSeries)));

            var timeRules = controlGroup.Rules.OfType<TimeRule>().Where(r => TimeSeriesEntriesPrecedeModelStartTime(r.TimeSeries, startTime));
            timeRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Warning,
                String.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, r.TimeSeries.Name, startTime), r.TimeSeries)));

            var intervalRules = controlGroup.Rules.OfType<IntervalRule>().Where(r => r.TimeSeries.Time.Values.Any() &&
                                                                           TimeSeriesEntriesPrecedeModelStartTime(r.TimeSeries, startTime));
            intervalRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Warning,
                String.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, r.TimeSeries.Name, startTime), r.TimeSeries)));

            // check end times
            pidRules = controlGroup.Rules.OfType<PIDRule>().Where(r => r.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.TimeSeries &&
                                                                           TimeSeriesEntriesExceedModelStopTime(r.TimeSeries, stopTime));
            pidRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Warning,
                String.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, r.TimeSeries.Name, stopTime), r.TimeSeries)));

            timeRules = controlGroup.Rules.OfType<TimeRule>().Where(r => TimeSeriesEntriesExceedModelStopTime(r.TimeSeries, stopTime));
            timeRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Warning,
                String.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, r.TimeSeries.Name, stopTime), r.TimeSeries)));

            intervalRules = controlGroup.Rules.OfType<IntervalRule>().Where(r => r.IntervalType == IntervalRule.IntervalRuleIntervalType.Variable &&
                                                                                 TimeSeriesEntriesExceedModelStopTime(r.TimeSeries, stopTime));
            intervalRules.ForEach(r => issues.Add(new ValidationIssue(r, ValidationSeverity.Warning,
                String.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, r.TimeSeries.Name, stopTime), r.TimeSeries)));

            return issues;
        }

        private static bool ValidateTimeSeries(ITimeSeries timeSeries, DateTime startTime, TimeSpan timeStep)
        {
            if (timeSeries.Time.Values.Count == 0) return true;
            return timeSeries.Time.Values.All(t => (t - startTime).Ticks%timeStep.Ticks == 0);
        }

        private static bool TimeSeriesEntriesPrecedeModelStartTime(ITimeSeries timeSeries, DateTime startTime)
        {
            if (timeSeries.Time.Values.Count == 0) return false;
            return startTime > timeSeries.Time.Values.First();
        }

        private static bool TimeSeriesEntriesExceedModelStopTime(ITimeSeries timeSeries, DateTime stopTime)
        {
            if (timeSeries.Time.Values.Count == 0) return false;
            return stopTime < timeSeries.Time.Values.Last();
        }
    }
}
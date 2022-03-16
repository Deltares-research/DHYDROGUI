using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Validation
{
    [TestFixture]
    public class ControlGroupValidatorTest
    {
        [Test]
        public void ValidControlGroup()
        {
            ControlGroup controlGroup = CreateValidControlGroup();

            ValidationReport validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void RulesmustHaveUniqueNames()
        {
            ControlGroup controlGroup = CreateValidControlGroup();
            controlGroup.Rules.Add(CreateValidHydraulicRule());

            ValidationReport validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("The name 'Rule 1' is used by 2 Rules.", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ControlGroupMustHaveAtLeastOneRule()
        {
            ControlGroup controlGroup = CreateValidControlGroup();
            controlGroup.Rules.Clear();

            ValidationReport validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Control Group requires at least 1 rule", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ControlGroupMustHaveAtLeastOneOutput()
        {
            ControlGroup controlGroup = CreateValidControlGroup();
            controlGroup.Outputs.Clear();

            ValidationReport validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Control Group requires at least 1 output", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ConditionsMustHaveUniqueNames()
        {
            ControlGroup controlGroup = CreateValidControlGroup();
            var timecondition = new TimeCondition {Name = "Test"};
            timecondition.TrueOutputs.Add(controlGroup.Rules.First());
            controlGroup.Conditions.Add(timecondition);
            controlGroup.Conditions.Add(timecondition);

            ValidationReport validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("The name 'Test' is used by 2 Conditions.", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidationFailsForSetPointTimeStepSmallerThanModelTimeStep()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            timeSeries.Clear();
            timeSeries[model.StartTime] = 3.0;
            timeSeries[model.StartTime.AddSeconds(1)] = 3.5;

            var pidRule = new PIDRule()
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };

            ControlGroup controlGroup = model.ControlGroups.First();
            controlGroup.Rules.Add(pidRule);
            controlGroup.Outputs.Add(new Output());

            var validator = new ControlGroupValidator();
            List<ValidationIssue> allPidIssues = validator.Validate(model, controlGroup).GetAllIssuesRecursive()
                                                          .Where(i => ReferenceEquals(i.Subject, pidRule)).ToList();
            Assert.AreEqual(1, allPidIssues.Count,
                            "The number of validation issues for the PID rule itself (i.e. not in the context of a control group)");
            Assert.AreEqual($"Series '{pidRule.TimeSeries.Name}' in RTC control group '{controlGroup.Name}': the amount of time steps is not a multiple of the model time step {model.TimeStep}.",
                            allPidIssues.First().Message, "");
        }

        [Test]
        public void ValidationPassesForSetPointTimeStepEqualToModelTimeStep()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            var timeStep = new TimeSpan(0, 1, 0, 0);

            timeSeries.Clear();
            timeSeries[model.StartTime] = 3.0;
            timeSeries[model.StartTime + timeStep] = 3.5;

            var PIDrule = new PIDRule()
            {
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries,
                TimeSeries = timeSeries
            };

            ControlGroup controlGroup = model.ControlGroups.First();
            controlGroup.Rules.Add(PIDrule);
            controlGroup.Outputs.Add(new Output());

            var validator = new ControlGroupValidator();
            Assert.AreEqual(0, validator.Validate(model, controlGroup).GetAllIssuesRecursive().Count(
                                i => ReferenceEquals(i.Subject, PIDrule)), "The number of validation issues for the PID rule");
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepPidControllerTest()
        {
            DateTime startTime;
            TimeSpan timeStep;
            ControlGroup controlGroup;

            TimeSeries timeSeries =
                SetRealTimeControllerControlGroup(out startTime, out timeStep, out controlGroup);
            var model = new RealTimeControlModel
            {
                TimeStep = timeStep,
                StartTime = startTime
            };
            var pidRule = new PIDRule
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };
            controlGroup.Rules.Add(pidRule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, pidRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the PID rule");

            var errorExpected =
                $"Series '{pidRule.TimeSeries.Name}' in RTC control group '{controlGroup.Name}': the amount of time steps is not a multiple of the model time step {model.TimeStep}.";
            Assert.AreEqual(errorExpected, foundIssues[0].Message);
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepTimeRuleControllerTest()
        {
            DateTime startTime;
            TimeSpan timeStep;
            ControlGroup controlGroup;
            TimeSeries timeSeries =
                SetRealTimeControllerControlGroup(out startTime, out timeStep, out controlGroup);

            var model = new RealTimeControlModel()
            {
                TimeStep = timeStep,
                StartTime = startTime
            };

            var timeRule = new TimeRule() {TimeSeries = timeSeries};
            controlGroup.Rules.Add(timeRule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the time rule");

            var errorExpected =
                $"Series '{timeRule.TimeSeries.Name}' in RTC control group '{controlGroup.Name}': the amount of time steps is not a multiple of the model time step {model.TimeStep}.";
            Assert.AreEqual(errorExpected, foundIssues[0].Message);
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepIntervalRuleControllerTest()
        {
            DateTime startTime;
            TimeSpan timeStep;
            ControlGroup controlGroup;
            TimeSeries timeSeries =
                SetRealTimeControllerControlGroup(out startTime, out timeStep, out controlGroup);

            var model = new RealTimeControlModel()
            {
                TimeStep = timeStep,
                StartTime = startTime
            };

            var intervalRule = new IntervalRule()
            {
                TimeSeries = timeSeries,
                IntervalType = IntervalRule.IntervalRuleIntervalType.Variable
            };

            controlGroup.Rules.Add(intervalRule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the interval rule");

            var errorExpected =
                $"Series '{intervalRule.TimeSeries.Name}' in RTC control group '{controlGroup.Name}': the amount of time steps is not a multiple of the model time step {model.TimeStep}.";
            Assert.AreEqual(errorExpected, foundIssues[0].Message);
        }

        [Test]
        public void ValidationHasWarningsIfTimeSeriesBoundsExceedModelTimes()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            timeSeries[model.StartTime.AddDays(-1)] = 1.0;
            timeSeries[model.StopTime.AddDays(1)] = 31.0;

            var validator = new ControlGroupValidator();
            ControlGroup controlGroup = model.ControlGroups.First();

            // check PIDrule
            var pidRule = new PIDRule
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };
            controlGroup.Rules.Add(pidRule);

            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, pidRule)).ToList();
            Assert.AreEqual(2, foundIssues.Count, "The number of validation issues for the PID rule");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[0].Severity, "Time series bound checking should raise warnings, not errors.");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[1].Severity, "Time series bound checking should raise warnings, not errors.");

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, pidRule.TimeSeries.Name, controlGroup.Name, model.StartTime.ToString(CultureInfo.InvariantCulture)),
                            foundIssues[0].Message);

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, pidRule.TimeSeries.Name, controlGroup.Name, model.StopTime.ToString(CultureInfo.InvariantCulture)),
                            foundIssues[1].Message);

            // check time rule
            var timeRule = new TimeRule {TimeSeries = timeSeries};
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(timeRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();
            Assert.AreEqual(2, foundIssues.Count, "The number of validation issues for the time rule");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[0].Severity, "Time series bound checking should raise warnings, not errors.");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[1].Severity, "Time series bound checking should raise warnings, not errors.");

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, timeRule.TimeSeries.Name, controlGroup.Name, model.StartTime.ToString(CultureInfo.InvariantCulture)),
                            foundIssues[0].Message);

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, timeRule.TimeSeries.Name, controlGroup.Name, model.StopTime.ToString(CultureInfo.InvariantCulture)),
                            foundIssues[1].Message);

            // check interval rule
            var intervalRule = new IntervalRule {TimeSeries = timeSeries};
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(intervalRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();
            Assert.AreEqual(2, foundIssues.Count, "The number of validation issues for the interval rule");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[0].Severity, "Time series bound checking should raise warnings, not errors.");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[1].Severity, "Time series bound checking should raise warnings, not errors.");

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, intervalRule.TimeSeries.Name, controlGroup.Name, model.StartTime.ToString(CultureInfo.InvariantCulture)),
                            foundIssues[0].Message);

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, intervalRule.TimeSeries.Name, controlGroup.Name, model.StopTime.ToString(CultureInfo.InvariantCulture)),
                            foundIssues[1].Message);

            // check values at start and stop time of model
            Assert.AreEqual(2.0, timeSeries.Evaluate<double>(model.StartTime), 1e-5);
            Assert.AreEqual(30.0, timeSeries.Evaluate<double>(model.StopTime), 1e-5);
        }

        [Test]
        public void ValidationDoesNotHaveWarningsIfTimeSeriesBoundsDoNotExceedModelTimes()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            // For the purpose of this test we need clean time series
            timeSeries.Clear();
            timeSeries[model.StartTime.AddDays(1)] = 1.0;
            timeSeries[model.StopTime.AddDays(-1)] = 31.0;

            ControlGroup controlGroup = model.ControlGroups.First();

            var validator = new ControlGroupValidator();

            // check PIDrule
            var PIDrule = new PIDRule()
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };
            controlGroup.Rules.Add(PIDrule);

            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, PIDrule)).ToList();
            Assert.AreEqual(0, foundIssues.Count, "The number of validation issues for the PIDrule");

            // check time rule
            var timeRule = new TimeRule() {TimeSeries = timeSeries};
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(timeRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();
            Assert.AreEqual(0, foundIssues.Count, "The number of validation issues for the time rule");

            // check interval rule
            var intervalRule = new IntervalRule() {TimeSeries = timeSeries};
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(intervalRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();
            Assert.AreEqual(0, foundIssues.Count, "The number of validation issues for the interval rule");
        }

        [Test]
        public void ValidationDoesNotThrowDivideByZeroExceptionIfModelTimestepIsZero()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel rtcModel = setup.Item1;
            rtcModel.TimeStep = new TimeSpan(0, 0, 0, 0);
            ControlGroup controlGroup = rtcModel.ControlGroups.First();

            TimeSeries timeSeries = setup.Item2;

            var pidRule = new PIDRule {PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries};
            pidRule.TimeSeries = timeSeries;
            controlGroup.Rules.Add(pidRule);

            var controlGroupValidator = new ControlGroupValidator();
            ValidationReport result = controlGroupValidator.Validate(rtcModel, controlGroup);

            // If we reach this statement validation did not throw errors
            Assert.GreaterOrEqual(result.ErrorCount, 0);
        }

        public static ControlGroup CreateValidControlGroup()
        {
            var validControlGroup = new ControlGroup();

            HydraulicRule validHydraulicRule = CreateValidHydraulicRule();
            validControlGroup.Rules.Add(validHydraulicRule);

            validControlGroup.Outputs.Add(validHydraulicRule.Outputs.First());

            return validControlGroup;
        }

        private static HydraulicRule CreateValidHydraulicRule()
        {
            Function tableFunction = HydraulicRule.DefineFunction();
            tableFunction[0.0] = 123.6;

            var input = new Input
            {
                ParameterName = "In",
                Feature = new RtcTestFeature {Name = "InFeat"}
            };

            var output = new Output
            {
                ParameterName = "Out",
                Feature = new RtcTestFeature {Name = "OutFeat"}
            };

            var validHydraulicRule = new HydraulicRule
            {
                Name = "Rule 1",
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output},
                Function = tableFunction,
                Interpolation = InterpolationType.Linear
            };
            return validHydraulicRule;
        }

        private Tuple<RealTimeControlModel, TimeSeries> SetUpRtcModelWithTimeSeriesAndEmptyControlGroup()
        {
            var timeSeriesStartTime = new DateTime(2012, 1, 1);
            var timeSeriesStopTime = new DateTime(2012, 1, 31);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            var timeSeries = new TimeSeries
            {
                Components = {new Variable<double>("SetPoint")},
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            timeSeries[timeSeriesStartTime] = 1.0;
            timeSeries[timeSeriesStopTime] = 31.0;

            DateTime modelStartTime = timeSeriesStartTime.AddDays(1);
            DateTime modelStopTime = timeSeriesStopTime.AddDays(-1);
            var rtcModel = new RealTimeControlModel
            {
                TimeStep = timeStep,
                StartTime = modelStartTime,
                StopTime = modelStopTime
            };
            rtcModel.ControlGroups.Add(new ControlGroup());

            return new Tuple<RealTimeControlModel, TimeSeries>(rtcModel, timeSeries);
        }

        private static TimeSeries SetRealTimeControllerControlGroup(out DateTime startTime, out TimeSpan timeStep, out ControlGroup controlGroup)
        {
            var timeSeries = new TimeSeries()
            {
                Components = {new Variable<double>("SetPoint")},
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            startTime = new DateTime(2012, 1, 1);
            timeStep = new TimeSpan(0, 1, 0, 0);

            timeSeries[startTime] = 3.0;
            timeSeries[startTime.AddSeconds(timeStep.TotalSeconds)] = 3.5;
            DateTime irregularTimeStep = startTime.AddSeconds(timeStep.TotalSeconds + 1);
            timeSeries[irregularTimeStep] = 3.5; //Irregular time series, timestep not multiple.
            controlGroup = new ControlGroup();
            controlGroup.Outputs.Add(new Output());
            return timeSeries;
        }
    }
}
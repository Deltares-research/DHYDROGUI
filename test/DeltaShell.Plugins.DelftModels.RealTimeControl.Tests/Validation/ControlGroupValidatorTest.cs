using System;
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
        public static ControlGroup CreateValidControlGroup()
        {
            var validControlGroup = new ControlGroup();

            var validHydraulicRule = CreateValidHydraulicRule();
            validControlGroup.Rules.Add(validHydraulicRule);

            validControlGroup.Outputs.Add(validHydraulicRule.Outputs.First());

            return validControlGroup;
        }
        private static HydraulicRule CreateValidHydraulicRule()
        {
            var tableFunction = HydraulicRule.DefineFunction();
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
                Inputs = new EventedList<Input> {input},
                Outputs = new EventedList<Output> {output},
                Function = tableFunction,
                Interpolation = InterpolationType.Linear
            };
            return validHydraulicRule;
        }

        [Test]
        public void ValidControlGroup()
        {
            var controlGroup = CreateValidControlGroup();

            var validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void RulesmustHaveUniqueNames()
        {
            var controlGroup = CreateValidControlGroup();
            controlGroup.Rules.Add(CreateValidHydraulicRule());

            var validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("The name 'Rule 1' is used by 2 Rules.", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ControlGroupMustHaveAtLeastOneRule()
        {
            var controlGroup = CreateValidControlGroup();
            controlGroup.Rules.Clear();

            var validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Control Group requires at least 1 rule", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ControlGroupMustHaveAtLeastOneOutput()
        {
            var controlGroup = CreateValidControlGroup();
            controlGroup.Outputs.Clear();

            var validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Control Group requires at least 1 output", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ConditionsMustHaveUniqueNames()
        {
            var controlGroup = CreateValidControlGroup();
            var timecondition = new TimeCondition {Name = "Test"};
            timecondition.TrueOutputs.Add(controlGroup.Rules.First());
            controlGroup.Conditions.Add(timecondition);
            controlGroup.Conditions.Add(timecondition);

            var validationResult = new ControlGroupValidator().Validate(null, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("The name 'Test' is used by 2 Conditions.", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidationFailsForSetPointTimeStepSmallerThanModelTimeStep()
        {
            var timeSeries = new TimeSeries()
            {
                Components = {new Variable<double>("SetPoint")},
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            var startTime = new DateTime(2012, 1, 1);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            timeSeries[startTime] = 3.0;
            timeSeries[startTime.AddSeconds(1)] = 3.5;

            var model = new RealTimeControlModel() {TimeStep = timeStep, StartTime = startTime};

            var PIDrule = new PIDRule()
                {TimeSeries = timeSeries, PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries};


            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(PIDrule);
            controlGroup.Outputs.Add(new Output());
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            var allPidIssues = validator.Validate(model, controlGroup).GetAllIssuesRecursive()
                .Where(i => ReferenceEquals(i.Subject, PIDrule)).ToList();
            Assert.AreEqual(1, allPidIssues.Count,
                "The number of validation issues for the PID rule itself (i.e. not in the context of a control group)");
            Assert.AreEqual(@"Series 'SetPoint' time steps not multiple of model time step 01:00:00.",
                allPidIssues.First().Message, "");
        }

        [Test]
        public void ValidationPassesForSetPointTimeStepEqualToThanModelTimeStep()
        {
            var timeSeries = new TimeSeries()
            {
                Components = {new Variable<double>("SetPoint")},
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            var startTime = new DateTime(2012, 1, 1);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            timeSeries[startTime] = 3.0;
            timeSeries[startTime + timeStep] = 3.5;

            var model = new RealTimeControlModel() {TimeStep = timeStep, StartTime = startTime};

            var PIDrule = new PIDRule()
            {
                PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries,
                TimeSeries = timeSeries
            };


            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(PIDrule);
            controlGroup.Outputs.Add(new Output());
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            Assert.AreEqual(0, validator.Validate(model, controlGroup).GetAllIssuesRecursive().Count(
                i => ReferenceEquals(i.Subject, PIDrule)), "The number of validation issues for the PID rule");
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepPidControllerTest()
        {
            DateTime startTime;
            TimeSpan timeStep;
            DateTime irregularTimeStep;
            ControlGroup controlGroup;

            var timeSeries =
                SetRealTimeControllerControlGroup(out startTime, out timeStep, out irregularTimeStep, out controlGroup);
            var model = new RealTimeControlModel() {TimeStep = timeStep, StartTime = startTime};
            var PIDrule = new PIDRule()
                {TimeSeries = timeSeries, PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries};
            controlGroup.Rules.Add(PIDrule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            var report = validator.Validate(model, controlGroup);
            var validationIssues = report.GetAllIssuesRecursive();
            var foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, PIDrule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the PID rule");

            var errorExpected =
                $"Series '{PIDrule.TimeSeries.Name}' time steps not multiple of model time step {model.TimeStep}.";
            Assert.AreEqual(errorExpected, foundIssues[0].Message);
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepTimeRuleControllerTest()
        {
            DateTime startTime;
            TimeSpan timeStep;
            DateTime irregularTimeStep;
            ControlGroup controlGroup;
            var timeSeries =
                SetRealTimeControllerControlGroup(out startTime, out timeStep, out irregularTimeStep, out controlGroup);

            var model = new RealTimeControlModel() {TimeStep = timeStep, StartTime = startTime};

            var timeRule = new TimeRule()
                {TimeSeries = timeSeries};
            controlGroup.Rules.Add(timeRule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            var report = validator.Validate(model, controlGroup);
            var validationIssues = report.GetAllIssuesRecursive();
            var foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();
           Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the time rule");

            var errorExpected =
                $"Series '{timeRule.TimeSeries.Name}' time steps not multiple of model time step {model.TimeStep}.";
            Assert.AreEqual(errorExpected, foundIssues[0].Message);
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepIntervalRuleControllerTest()
        {
            DateTime startTime;
            TimeSpan timeStep;
            DateTime irregularTimeStep;
            ControlGroup controlGroup;
            var timeSeries =
                SetRealTimeControllerControlGroup(out startTime, out timeStep, out irregularTimeStep, out controlGroup);

            var model = new RealTimeControlModel() {TimeStep = timeStep, StartTime = startTime};

            var intervalRule = new IntervalRule()
                {TimeSeries = timeSeries, IntervalType = IntervalRule.IntervalRuleIntervalType.Variable};

            controlGroup.Rules.Add(intervalRule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            var report = validator.Validate(model, controlGroup);
            var validationIssues = report.GetAllIssuesRecursive();
            var foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the interval rule");

            var errorExpected =
                $"Series '{intervalRule.TimeSeries.Name}' time steps not multiple of model time step {model.TimeStep}.";
            Assert.AreEqual(errorExpected, foundIssues[0].Message);
        }

        private static TimeSeries SetRealTimeControllerControlGroup(out DateTime startTime, out TimeSpan timeStep,
            out DateTime irregularTimeStep, out ControlGroup controlGroup)
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
            irregularTimeStep = startTime.AddSeconds(timeStep.TotalSeconds + 1);
            timeSeries[irregularTimeStep] = 3.5; //Irregular time series, timestep not multiple.
            controlGroup = new ControlGroup();
            controlGroup.Outputs.Add(new Output());
            return timeSeries;
        }

        [Test]
        public void ValidationHasWarningsIfTimeSeriesBoundsExceedModelTimes()
        {
            var startTime = new DateTime(2012, 1, 1);
            var stopTime = new DateTime(2012, 1, 31);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            var timeSeries = new TimeSeries()
            {
                Components = { new Variable<double>("SetPoint") },
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            timeSeries[startTime] = 1.0;
            timeSeries[stopTime] = 31.0;

            var modelStartTime = startTime.AddDays(1);
            var modelStopTime = stopTime.AddDays(-1);
            var model = new RealTimeControlModel() { TimeStep = timeStep, StartTime = modelStartTime, StopTime = modelStopTime };
            var validator = new ControlGroupValidator();
            var controlGroup = new ControlGroup();
            model.ControlGroups.Add(controlGroup);

            // check PIDrule
            var PIDrule = new PIDRule()
                { TimeSeries = timeSeries, PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries };
            controlGroup.Rules.Add(PIDrule);

            var report = validator.Validate(model, controlGroup);
            var validationIssues = report.GetAllIssuesRecursive();
            var foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, PIDrule)).ToList();
            Assert.AreEqual(2, foundIssues.Count, "The number of validation issues for the PID rule");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[0].Severity, "Time series bound checking should raise warnings, not errors.");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[1].Severity, "Time series bound checking should raise warnings, not errors.");

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, PIDrule.TimeSeries.Name, model.StartTime.ToString(CultureInfo.InvariantCulture)), 
                            foundIssues[0].Message);

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, PIDrule.TimeSeries.Name, model.StopTime.ToString(CultureInfo.InvariantCulture)),
                            foundIssues[1].Message);

            // check time rule
            var timeRule = new TimeRule()
                { TimeSeries = timeSeries };
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(timeRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();
            Assert.AreEqual(2, foundIssues.Count, "The number of validation issues for the time rule");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[0].Severity, "Time series bound checking should raise warnings, not errors.");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[1].Severity, "Time series bound checking should raise warnings, not errors.");

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, timeRule.TimeSeries.Name, model.StartTime.ToString(CultureInfo.InvariantCulture)),
                foundIssues[0].Message);

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, timeRule.TimeSeries.Name, model.StopTime.ToString(CultureInfo.InvariantCulture)),
                foundIssues[1].Message);

            // check interval rule
            var intervalRule = new IntervalRule()
                { TimeSeries = timeSeries };
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(intervalRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();
            Assert.AreEqual(2, foundIssues.Count, "The number of validation issues for the interval rule");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[0].Severity, "Time series bound checking should raise warnings, not errors.");
            Assert.AreEqual(ValidationSeverity.Warning, foundIssues[1].Severity, "Time series bound checking should raise warnings, not errors.");

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatPrecedeModelStartTime, intervalRule.TimeSeries.Name, model.StartTime.ToString(CultureInfo.InvariantCulture)),
                foundIssues[0].Message);

            Assert.AreEqual(string.Format(Resources.RealTimeControlControlGroupValidator_SeriesHasTimestepsThatExceedModelStopTime, intervalRule.TimeSeries.Name, model.StopTime.ToString(CultureInfo.InvariantCulture)),
                foundIssues[1].Message);

            // check values at start and stop time of model
            Assert.AreEqual(2.0, timeSeries.Evaluate<double>(modelStartTime), 1e-5);
            Assert.AreEqual(30.0, timeSeries.Evaluate<double>(modelStopTime), 1e-5);
        }

        [Test]
        public void ValidationDoesNotHaveWarningsIfTimeSeriesBoundsDoNotExceedModelTimes()
        {
            // setup model and controlgroup
            var startTime = new DateTime(2012, 1, 2);
            var stopTime = new DateTime(2012, 1, 30);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            var timeSeries = new TimeSeries()
            {
                Components = { new Variable<double>("SetPoint") },
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            timeSeries[startTime] = 1.0;
            timeSeries[stopTime] = 31.0;

            var modelStartTime = startTime.AddDays(-1);
            var modelStopTime = stopTime.AddDays(1);
            var model = new RealTimeControlModel() { TimeStep = timeStep, StartTime = modelStartTime, StopTime = modelStopTime };

            var controlGroup = new ControlGroup();
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();

            // check PIDrule
            var PIDrule = new PIDRule()
                { TimeSeries = timeSeries, PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries };
            controlGroup.Rules.Add(PIDrule);

            var report = validator.Validate(model, controlGroup);
            var validationIssues = report.GetAllIssuesRecursive();
            var foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, PIDrule)).ToList();
            Assert.AreEqual(0, foundIssues.Count, "The number of validation issues for the PIDrule");

            // check time rule
            var timeRule = new TimeRule()
            { TimeSeries = timeSeries };
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(timeRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();
            Assert.AreEqual(0, foundIssues.Count, "The number of validation issues for the time rule");


            // check interval rule
            var intervalRule = new IntervalRule()
                { TimeSeries = timeSeries };
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(intervalRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();
            Assert.AreEqual(0, foundIssues.Count, "The number of validation issues for the interval rule");

        }
    }
}
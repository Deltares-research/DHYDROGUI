using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain;
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
                Feature = new RtcTestFeature { Name = "InFeat" }
            };

            var output = new Output
            {
                ParameterName = "Out",
                Feature = new RtcTestFeature { Name = "OutFeat" }
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
            var timecondition = new TimeCondition { Name = "Test" };
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
                Components = { new Variable<double>("SetPoint") },
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
            
            var startTime = new DateTime(2012, 1, 1);
            var timeStep = new TimeSpan(0, 1, 0, 0);
            
            timeSeries[startTime] = 3.0;
            timeSeries[startTime.AddSeconds(1)] = 3.5;

            var model = new RealTimeControlModel() { TimeStep = timeStep, StartTime = startTime };

            var PIDrule = new PIDRule()
                              {TimeSeries = timeSeries, PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries};
            
            
            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(PIDrule);
            controlGroup.Outputs.Add(new Output());
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            Assert.AreEqual(1, validator.Validate(model, controlGroup).GetAllIssuesRecursive().Count(
                                i => ReferenceEquals(i.Subject, PIDrule)), "The number of validation issues for the PID rule");
        }

        [Test]
        public void ValidationPassesForSetPointTimeStepEqualToThanModelTimeStep()
        {
            var timeSeries = new TimeSeries()
            {
                Components = { new Variable<double>("SetPoint") },
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            var startTime = new DateTime(2012, 1, 1);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            timeSeries[startTime] = 3.0;
            timeSeries[startTime + timeStep] = 3.5;

            var model = new RealTimeControlModel() { TimeStep = timeStep, StartTime = startTime };

            var PIDrule = new PIDRule() { PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries, TimeSeries = timeSeries };


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

            var timeSeries = SetRealTimeControllerControlGroup(out startTime, out timeStep, out irregularTimeStep, out controlGroup);
            var model = new RealTimeControlModel() { TimeStep = timeStep, StartTime = startTime };
            var PIDrule = new PIDRule()
                { TimeSeries = timeSeries, PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries };
            controlGroup.Rules.Add(PIDrule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            var report = validator.Validate(model, controlGroup);
            var validationIssues = report.GetAllIssuesRecursive();
            var foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, PIDrule)).ToList();
            Assert.AreEqual(1, foundIssues.Count,"The number of validation issues for the PID rule" );

            var errorExpected = String.Format("Series '{0}' time steps not multiple of model time step {1}.", PIDrule.TimeSeries.Name, model.TimeStep,
                irregularTimeStep);
            Assert.AreEqual( errorExpected, foundIssues[0].Message );
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepTimeRuleControllerTest()
        {
            DateTime startTime;
            TimeSpan timeStep;
            DateTime irregularTimeStep;
            ControlGroup controlGroup;
            var timeSeries = SetRealTimeControllerControlGroup(out startTime, out timeStep, out irregularTimeStep, out controlGroup);

            var model = new RealTimeControlModel() { TimeStep = timeStep, StartTime = startTime };

            var timeRule = new TimeRule()
                { TimeSeries = timeSeries};
            controlGroup.Rules.Add(timeRule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            var report = validator.Validate(model, controlGroup);
            var validationIssues = report.GetAllIssuesRecursive();
            var foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the PID rule");

            var errorExpected = String.Format("Series '{0}' time steps not multiple of model time step {1}.", timeRule.TimeSeries.Name, model.TimeStep,
                irregularTimeStep);
            Assert.AreEqual(errorExpected, foundIssues[0].Message);
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepIntervalRuleControllerTest()
        {
            DateTime startTime;
            TimeSpan timeStep;
            DateTime irregularTimeStep;
            ControlGroup controlGroup;
            var timeSeries = SetRealTimeControllerControlGroup(out startTime, out timeStep, out irregularTimeStep, out controlGroup);

            var model = new RealTimeControlModel() { TimeStep = timeStep, StartTime = startTime };

            var intervalRule = new IntervalRule()
                { TimeSeries = timeSeries, IntervalType = IntervalRule.IntervalRuleIntervalType.Variable};

            controlGroup.Rules.Add(intervalRule);
            model.ControlGroups.Add(controlGroup);

            var validator = new ControlGroupValidator();
            var report = validator.Validate(model, controlGroup);
            var validationIssues = report.GetAllIssuesRecursive();
            var foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the PID rule");

            var errorExpected = String.Format("Series '{0}' time steps not multiple of model time step {1}.", intervalRule.TimeSeries.Name, model.TimeStep,
                irregularTimeStep);
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Validation;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Validation
{
    [TestFixture]
    public class ControlGroupValidatorTest
    {
        private IRealTimeControlModel realTimeControlModel;
        private IModel controlledModel;
        private ControlGroupValidator validator;

        [SetUp]
        public void SetUp()
        {
            realTimeControlModel = Substitute.For<IRealTimeControlModel>();
            controlledModel = Substitute.For<IModel>();
            realTimeControlModel.ControlledModels.Returns(new[] { controlledModel });
            validator = new ControlGroupValidator();
        }

        [Test]
        public void Constructor_RootObjectIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => validator.Validate(null, Substitute.For<IControlGroup>()), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_TargetIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => validator.Validate(realTimeControlModel, null), Throws.ArgumentNullException);
        }
        
        [Test]
        public void ValidControlGroup()
        {
            IControlGroup controlGroup = CreateValidControlGroup(controlledModel);

            ValidationReport validationResult = validator.Validate(realTimeControlModel, controlGroup);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void RulesMustHaveUniqueNames()
        {
            IControlGroup controlGroup = CreateValidControlGroup(controlledModel);

            string existingRuleName = controlGroup.Rules.First().Name;
            RuleBase rule = CreateRule(existingRuleName);
            controlGroup.Rules.Add(rule);

            ValidationReport validationResult = validator.Validate(realTimeControlModel, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual($"The name '{existingRuleName}' is used by 2 Rules.", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ControlGroupMustHaveAtLeastOneRule()
        {
            IControlGroup controlGroup = CreateValidControlGroup(controlledModel);
            
            controlGroup.Inputs.Clear();
            controlGroup.Rules.Clear();

            ValidationReport validationResult = validator.Validate(realTimeControlModel, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Control Group requires at least 1 rule", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ControlGroupMustHaveAtLeastOneOutput()
        {
            IControlGroup controlGroup = CreateValidControlGroup(controlledModel);
            
            controlGroup.Outputs.Clear();

            ValidationReport validationResult = validator.Validate(realTimeControlModel, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Control Group requires at least 1 output", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ConditionsMustHaveUniqueNames()
        {
            IControlGroup controlGroup = CreateValidControlGroup(controlledModel);

            const string duplicateName = "Test";
            controlGroup.Conditions.Add(CreateCondition(name: duplicateName));
            controlGroup.Conditions.Add(CreateCondition(name: duplicateName));

            ValidationReport validationResult = validator.Validate(realTimeControlModel, controlGroup);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual($"The name '{duplicateName}' is used by 2 Conditions.", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidationFailsForSetPointTimeStepSmallerThanModelTimeStep()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            timeSeries[model.StartTime.AddSeconds(1)] = 3.5;

            var pidRule = new PIDRule()
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };

            ControlGroup controlGroup = model.ControlGroups.First();
            controlGroup.Rules.Add(pidRule);
            controlGroup.Outputs.Add(new Output());

            List<ValidationIssue> allPidIssues = validator.Validate(model, controlGroup).GetAllIssuesRecursive()
                                                          .Where(i => ReferenceEquals(i.Subject, pidRule)).ToList();
            Assert.AreEqual(1, allPidIssues.Count,
                            "The number of validation issues for the PID rule itself (i.e. not in the context of a control group)");
            Assert.AreEqual($"Time series time step is not a multiple of the model time step.", allPidIssues.First().Message);
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
            timeSeries[model.StopTime] = 12.0;
            
            var pidRule = new PIDRule()
            {
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries,
                TimeSeries = timeSeries
            };

            ControlGroup controlGroup = model.ControlGroups.First();
            controlGroup.Rules.Add(pidRule);
            controlGroup.Outputs.Add(new Output());

            Assert.AreEqual(0, validator.Validate(model, controlGroup).GetAllIssuesRecursive().Count(
                                i => ReferenceEquals(i.Subject, pidRule)), "The number of validation issues for the PID rule");
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepPidControllerTest()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithIrregularTimeSeriesAndEmptyControlGroup();
            
            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;
            
            var pidRule = new PIDRule
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };
            
            ControlGroup controlGroup = model.ControlGroups.First();
            controlGroup.Rules.Add(pidRule);
            model.ControlGroups.Add(controlGroup);

            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, pidRule)).ToList();

            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the PID rule");
            Assert.AreEqual("Time series time step is not a multiple of the model time step.", foundIssues[0].Message);
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepTimeRuleControllerTest()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithIrregularTimeSeriesAndEmptyControlGroup();
            
            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            var timeRule = new TimeRule() {TimeSeries = timeSeries};
            
            ControlGroup controlGroup = model.ControlGroups.First();
            controlGroup.Rules.Add(timeRule);
            model.ControlGroups.Add(controlGroup);

            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();

            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the time rule");
            Assert.AreEqual("Time series time step is not a multiple of the model time step.", foundIssues[0].Message);
        }

        [Test]
        public void ValidationFailsForIrregularNotMultipleTimeStepIntervalRuleControllerTest()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithIrregularTimeSeriesAndEmptyControlGroup();
            
            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            var intervalRule = new IntervalRule()
            {
                TimeSeries = timeSeries,
                IntervalType = IntervalRule.IntervalRuleIntervalType.Variable
            };

            ControlGroup controlGroup = model.ControlGroups.First();
            controlGroup.Rules.Add(intervalRule);
            model.ControlGroups.Add(controlGroup);

            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();

            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the interval rule");
            Assert.AreEqual("Time series time step is not a multiple of the model time step.", foundIssues[0].Message);
        }

        [Test]
        public void ValidationHasWarningsIfTimeSeriesDoesNotSpanModelRunInterval()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            timeSeries.Clear();
            timeSeries[model.StartTime.AddDays(1)] = 1.0;
            timeSeries[model.StopTime.AddDays(-1)] = 31.0;

            ControlGroup controlGroup = model.ControlGroups.First();

            // check PID rule
            var pidRule = new PIDRule
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };
            controlGroup.Rules.Add(pidRule);

            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, pidRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the PID rule");
            Assert.AreEqual(ValidationSeverity.Error, foundIssues[0].Severity, "Time series bound checking should raise errors.");
            Assert.AreEqual(Resources.ControlGroupValidator_TimeSeriesDoesNotSpanModelRunInterval, foundIssues[0].Message);

            // check time rule
            var timeRule = new TimeRule {TimeSeries = timeSeries};
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(timeRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, timeRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the time rule");
            Assert.AreEqual(ValidationSeverity.Error, foundIssues[0].Severity, "Time series bound checking should raise errors.");
            Assert.AreEqual(Resources.ControlGroupValidator_TimeSeriesDoesNotSpanModelRunInterval, foundIssues[0].Message);

            // check interval rule
            var intervalRule = new IntervalRule {TimeSeries = timeSeries};
            controlGroup.Rules.Clear();
            controlGroup.Rules.Add(intervalRule);

            report = validator.Validate(model, controlGroup);
            validationIssues = report.GetAllIssuesRecursive();
            foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, intervalRule)).ToList();
            Assert.AreEqual(1, foundIssues.Count, "The number of validation issues for the interval rule");
            Assert.AreEqual(ValidationSeverity.Error, foundIssues[0].Severity, "Time series bound checking should raise errors.");
            Assert.AreEqual(Resources.ControlGroupValidator_TimeSeriesDoesNotSpanModelRunInterval, foundIssues[0].Message);

            // check values at start and stop time of model
            Assert.AreEqual(1.0, timeSeries.Evaluate<double>(model.StartTime), 1e-5);
            Assert.AreEqual(31.0, timeSeries.Evaluate<double>(model.StopTime), 1e-5);
        }

        [Test]
        public void ValidationDoesNotHaveWarningsIfTimeSeriesSpansModelRunInterval()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            // For the purpose of this test we need clean time series
            timeSeries.Clear();
            timeSeries[model.StartTime.AddDays(-1)] = 1.0;
            timeSeries[model.StopTime.AddDays(1)] = 31.0;

            ControlGroup controlGroup = model.ControlGroups.First();

            // check PID rule
            var pidRule = new PIDRule()
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };
            controlGroup.Rules.Add(pidRule);

            ValidationReport report = validator.Validate(model, controlGroup);
            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            List<ValidationIssue> foundIssues = validationIssues.Where(i => ReferenceEquals(i.Subject, pidRule)).ToList();
            Assert.AreEqual(0, foundIssues.Count, "The number of validation issues for the PID rule");

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
        public void ValidationDoesNotThrowDivideByZeroExceptionIfModelTimeStepIsZero()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();

            RealTimeControlModel rtcModel = setup.Item1;
            rtcModel.TimeStep = new TimeSpan(0, 0, 0, 0);
            ControlGroup controlGroup = rtcModel.ControlGroups.First();

            TimeSeries timeSeries = setup.Item2;

            var pidRule = new PIDRule
            {
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries,
                TimeSeries = timeSeries
            };
            controlGroup.Rules.Add(pidRule);

            ValidationReport result = validator.Validate(rtcModel, controlGroup);

            // If we reach this statement validation did not throw errors
            Assert.GreaterOrEqual(result.ErrorCount, 0);
        }
        
        [Test]
        public void Validate_WhenInputHasInvalidCoupling_ReportContainsError()
        {
            // Arrange
            IControlGroup controlGroup = CreateValidControlGroup(controlledModel);

            var invalidFeature = Substitute.For<IFeature>();
            controlGroup.Inputs.First().Feature = invalidFeature;

            // Act
            ValidationReport report = validator.Validate(realTimeControlModel, controlGroup);

            // Assert
            Assert.That(report.IsEmpty, Is.False);
            Assert.That(report.ErrorCount, Is.EqualTo(1));
            Assert.That(report.AllErrors.ElementAt(0).Message, Is.EqualTo(string.Format(Resources.Feature_0_cannot_be_used_as_input_for_control_group_1_, invalidFeature, controlGroup.Name)));
        }
        
        [Test]
        public void Validate_WhenOutputHasInvalidCoupling_ReportContainsError()
        {
            // Arrange
            IControlGroup controlGroup = CreateValidControlGroup(controlledModel);

            var invalidFeature = Substitute.For<IFeature>();
            controlGroup.Outputs.First().Feature = invalidFeature;

            // Act
            ValidationReport report = validator.Validate(realTimeControlModel, controlGroup);

            // Assert
            Assert.That(report.IsEmpty, Is.False);
            Assert.That(report.ErrorCount, Is.EqualTo(1));
            Assert.That(report.AllErrors.ElementAt(0).Message, Is.EqualTo(string.Format(Resources.Feature_0_cannot_be_used_as_output_for_control_group_1_, invalidFeature, controlGroup.Name)));
        }

        private Tuple<RealTimeControlModel, TimeSeries> SetUpRtcModelWithTimeSeriesAndEmptyControlGroup()
        {
            var timeSeriesStartTime = new DateTime(2012, 1, 1);
            var timeSeriesStopTime = new DateTime(2012, 1, 31);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            var timeSeries = new TimeSeries
            {
                Components = { new Variable<double>("SetPoint") },
                Name = "SetPoint",
                Time =
                {
                    DefaultValue = new DateTime(2000, 1, 1),
                    InterpolationType = InterpolationType.Linear,
                    ExtrapolationType = ExtrapolationType.Constant
                },
                [timeSeriesStartTime] = 1.0,
                [timeSeriesStopTime] = 31.0
            };

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

        private Tuple<RealTimeControlModel, TimeSeries> SetUpRtcModelWithIrregularTimeSeriesAndEmptyControlGroup()
        {
            Tuple<RealTimeControlModel, TimeSeries> setup = SetUpRtcModelWithTimeSeriesAndEmptyControlGroup();
            
            RealTimeControlModel model = setup.Item1;
            TimeSeries timeSeries = setup.Item2;

            DateTime irregularTimeStep = model.StartTime.AddSeconds(model.TimeStep.TotalSeconds + 1);
            timeSeries[irregularTimeStep] = 3.5; //Irregular time series, time step not multiple.
            
            return setup;
        }

        private static void ConfigureControlledModel(IModel controlledModel, IControlGroup controlGroup)
        {
            IEnumerable<IFeature> controlledInputs = controlGroup.Inputs.Select(i => i.Feature).ToArray();
            controlledModel.GetChildDataItemLocations(DataItemRole.Output).Returns(controlledInputs);

            IEnumerable<IFeature> controlledOutputs = controlGroup.Outputs.Select(i => i.Feature).ToArray();
            controlledModel.GetChildDataItemLocations(DataItemRole.Input).Returns(controlledOutputs);
        }

        private static IControlGroup CreateValidControlGroup(IModel controlledModel)
        {
            Input input = CreateInput();
            Output output = CreateOutput();

            RuleBase rule = CreateRule(output: output);
            rule.Inputs.Add(input);

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(rule);
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            ConfigureControlledModel(controlledModel, controlGroup);

            return controlGroup;
        }

        private static Input CreateInput()
        {
            var feature = Substitute.For<IFeature>();
            return new Input
            {
                Feature = feature,
                ParameterName = "some_input"
            };
        }

        private static Output CreateOutput()
        {
            var feature = Substitute.For<IFeature>();
            return new Output
            {
                Feature = feature,
                ParameterName = "some_output"
            };
        }

        private static RuleBase CreateRule(string name = "some_rule_name", Output output = null)
        {
            if (output == null)
            {
                output = CreateOutput();
            }

            var rule = Substitute.For<RuleBase>();
            rule.Name = name;
            rule.Inputs = new EventedList<IInput>();
            rule.Outputs = new EventedList<Output> { output };

            return rule;
        }

        private static ConditionBase CreateCondition(string name = "some_condition_name", RtcBaseObject trueOutput = null)
        {
            if (trueOutput == null)
            {
                trueOutput = Substitute.For<RtcBaseObject>();
            }

            var condition = Substitute.For<ConditionBase>();
            condition.Name = name;
            condition.TrueOutputs.Add(trueOutput);

            return condition;
        }
    }
}
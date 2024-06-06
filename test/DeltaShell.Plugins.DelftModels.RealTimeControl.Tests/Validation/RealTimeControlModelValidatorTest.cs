using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Validation;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Validation
{
    [TestFixture]
    public class RealTimeControlModelValidatorTest
    {
        [Test]
        public void ValidRealTimeControlModel()
        {
            RealTimeControlModel model = CreateValidRealTimeControlModel();
            ConfigureControlledModel(model);

            ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ModelMustHaveAtLeastOneControlGroup()
        {
            RealTimeControlModel model = CreateValidRealTimeControlModel();
            model.ControlGroups.Clear();

            ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("There must be at least 1 control group defined", validationResult.AllErrors.First().Message);

            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidateModelWith2RulesWithSameName()
        {
            var realTimeControlModel = new RealTimeControlModel();

            ControlGroup controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            controlGroup1.Name = "controlGroup1";
            var hydraulicRule1A = (HydraulicRule) controlGroup1.Rules[0];
            var hydraulicCondition1A = (StandardCondition) controlGroup1.Conditions[0];
            hydraulicRule1A.Function[0.0] = 1.0;

            var hydraulicRule1B = new HydraulicRule();
            var hydraulicCondition1B = new StandardCondition();

            hydraulicCondition1A.FalseOutputs.Add(hydraulicCondition1B);
            hydraulicCondition1B.TrueOutputs.Add(hydraulicRule1B);
            hydraulicCondition1B.Input = controlGroup1.Inputs[0];
            hydraulicRule1B.Outputs.Add(controlGroup1.Outputs[0]);
            hydraulicRule1B.Inputs.Add(controlGroup1.Inputs[0]);
            hydraulicRule1B.Function[0.0] = 1.0;

            controlGroup1.Rules.Add(hydraulicRule1B);
            controlGroup1.Conditions.Add(hydraulicCondition1B);

            realTimeControlModel.ControlGroups.Add(controlGroup1);
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup1);

            ControlGroup controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            controlGroup2.Name = "controlGroup2";
            var hydraulicRule2A = (HydraulicRule) controlGroup2.Rules[0];
            var hydraulicCondition2A = (StandardCondition) controlGroup2.Conditions[0];
            hydraulicRule2A.Function[0.0] = 1.0;
            realTimeControlModel.ControlGroups.Add(controlGroup2);
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup2);

            hydraulicRule1A.Name = "rule1";
            hydraulicCondition1A.Name = "condition1";
            hydraulicRule2A.Name = "rule12";
            hydraulicCondition2A.Name = "condition2";

            ConfigureControlledModel(realTimeControlModel);

            ValidationReport result = realTimeControlModel.Validate();
            Assert.AreEqual(0, result.ErrorCount);
            Assert.AreEqual(0, result.WarningCount);
            Assert.AreEqual(0, result.InfoCount);

            // duplicate names are allowed within 1 model; not within 1 controlgroup
            hydraulicRule1A.Name = "It is I";
            hydraulicRule2A.Name = "It is I";
            result = realTimeControlModel.Validate();
            Assert.AreEqual(0, result.ErrorCount);
            Assert.AreEqual(0, result.WarningCount);
            Assert.AreEqual(0, result.InfoCount);

            hydraulicCondition1A.Name = "It is I";
            hydraulicCondition2A.Name = "It is I";
            result = realTimeControlModel.Validate();
            Assert.AreEqual(0, result.ErrorCount);
            Assert.AreEqual(0, result.WarningCount);
            Assert.AreEqual(0, result.InfoCount);

            // 2 rules in 1 group with same name
            hydraulicRule1B.Name = "It is I";
            result = realTimeControlModel.Validate();
            Assert.AreEqual(1, result.ErrorCount);
            Assert.AreEqual(ValidationSeverity.Error, result.Severity());
            Assert.AreEqual(0, result.WarningCount);
            Assert.AreEqual(0, result.InfoCount);

            // 2 rules in 1 group with same name
            hydraulicRule1B.Name = "Something completely different.";
            result = realTimeControlModel.Validate();
            Assert.AreEqual(0, result.ErrorCount);
            Assert.AreEqual(0, result.WarningCount);
            Assert.AreEqual(0, result.InfoCount);
            hydraulicCondition1B.Name = "It is I";
            result = realTimeControlModel.Validate();
            Assert.AreEqual(1, result.ErrorCount);
            Assert.AreEqual(ValidationSeverity.Error, result.Severity());
            Assert.AreEqual(0, result.WarningCount);
            Assert.AreEqual(0, result.InfoCount);
        }

        [Test]
        public void ValidateRealTimeControlModelRestartStateWithIntermediateRestartFiles()
        {
            RealTimeControlModel model = CreateValidFilledRealTimeControlModel();
            ConfigureControlledModel(model);

            model.WriteRestart = true;
            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.TimeStep;

            ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Validate_WhenSaveStateTimeStepIsNotCorrect_ShouldGiveWriteRestartSubReportWithError()
        {
            var rtcModel = new RealTimeControlModel
            {
                WriteRestart = true,
                TimeStep = new TimeSpan(0, 2, 0, 0),
                SaveStateTimeStep = new TimeSpan(0, 3, 0, 0)
            };

            rtcModel.SaveStateStartTime = rtcModel.StartTime;
            rtcModel.SaveStateStopTime = rtcModel.StopTime;

            ValidationReport validationResult = new RealTimeControlModelValidator().Validate(rtcModel);
            ValidationReport writeRestartSubValidationReport = validationResult.SubReports.FirstOrDefault(sr =>
                                                                                                              sr.Category == Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_range_settings);

            Assert.IsNotNull(writeRestartSubValidationReport);

            ValidationIssue restartValidationIssue = writeRestartSubValidationReport.GetAllIssuesRecursive().FirstOrDefault(i =>
                                                                                                                                i.Message == Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_time_step_must_be_an_integer_multiple_of_the_output_time_step_);
            Assert.IsNotNull(restartValidationIssue);
            Assert.AreEqual(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_step,
                            restartValidationIssue.ViewData);
        }

        private RealTimeControlModel CreateValidRealTimeControlModel()
        {
            var validRealTimeControlModel = new RealTimeControlModel();
            validRealTimeControlModel.ControlGroups.Add(CreateValidControlGroup());
            return validRealTimeControlModel;
        }

        private static RealTimeControlModel CreateValidFilledRealTimeControlModel()
        {
            var model = new RealTimeControlModel();

            ControlGroup controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            controlGroup1.Name = "controlGroup1";
            var hydraulicRule1A = (HydraulicRule) controlGroup1.Rules[0];
            var hydraulicCondition1A = (StandardCondition) controlGroup1.Conditions[0];
            var hydraulicRule1B = new HydraulicRule();
            var hydraulicCondition1B = new StandardCondition();
            hydraulicCondition1A.FalseOutputs.Add(hydraulicCondition1B);
            hydraulicCondition1B.TrueOutputs.Add(hydraulicRule1B);
            hydraulicCondition1B.Input = controlGroup1.Inputs[0];
            hydraulicRule1B.Outputs.Add(controlGroup1.Outputs[0]);
            hydraulicRule1B.Inputs.Add(controlGroup1.Inputs[0]);
            hydraulicRule1A.Function[0.0] = 1.0;
            hydraulicRule1B.Function[0.0] = 1.0;
            controlGroup1.Rules.Add(hydraulicRule1B);
            controlGroup1.Conditions.Add(hydraulicCondition1B);
            model.ControlGroups.Add(controlGroup1);
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup1);

            ControlGroup controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            controlGroup2.Name = "controlGroup2";
            var hydraulicRule2A = (HydraulicRule) controlGroup2.Rules[0];
            var hydraulicCondition2A = (StandardCondition) controlGroup2.Conditions[0];
            hydraulicRule2A.Function[0.0] = 1.0;
            model.ControlGroups.Add(controlGroup2);
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup2);

            hydraulicRule1A.Name = "rule1";
            hydraulicCondition1A.Name = "condition1";
            hydraulicRule2A.Name = "rule12";
            hydraulicCondition2A.Name = "condition2";
            return model;
        }

        private static void ConfigureControlledModel(IRealTimeControlModel realTimeControlModel)
        {
            var controlledModel = Substitute.For<ITimeDependentModel>();
            controlledModel.StartTime = realTimeControlModel.StartTime;
            controlledModel.StopTime = realTimeControlModel.StopTime;
            controlledModel.TimeStep = realTimeControlModel.TimeStep;

            IEnumerable<IFeature> controlledInputs = realTimeControlModel.ControlGroups.SelectMany(c => c.Inputs).Select(i => i.Feature);
            controlledModel.GetChildDataItemLocations(DataItemRole.Output).Returns(controlledInputs);

            IEnumerable<IFeature> controlledOutputs = realTimeControlModel.ControlGroups.SelectMany(c => c.Outputs).Select(i => i.Feature);
            controlledModel.GetChildDataItemLocations(DataItemRole.Input).Returns(controlledOutputs);

            var integratedModel = Substitute.For<ICompositeActivity>();
            integratedModel.Activities.Returns(new EventedList<IActivity> { controlledModel });

            realTimeControlModel.Owner = integratedModel;
        }
        
        private static ControlGroup CreateValidControlGroup()
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
    }
}
using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Validation
{
    [TestFixture]
    public class RealTimeControlModelValidatorTest
    {
        private RealTimeControlModel CreateValidRealTimeControlModel()
        {
            var validRealTimeControlModel = new RealTimeControlModel();
            validRealTimeControlModel.ControlGroups.Add(ControlGroupValidatorTest.CreateValidControlGroup());
            return validRealTimeControlModel;
        }

        [Test]
        public void ValidRealTimeControlModel()
        {
            var model = CreateValidRealTimeControlModel();

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }
        
        [Test]
        public void ModelMustHaveAtLeastOneControlGroup()
        {
            var model = CreateValidRealTimeControlModel();
            model.ControlGroups.Clear();

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("There must be at least 1 control group defined", validationResult.AllErrors.First().Message);

            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidateModelWith2RulesWithSameName()
        {
            var realTimeControlModel = new RealTimeControlModel();

            var controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            controlGroup1.Name = "controlGroup1";
            var hydraulicRule1A = (HydraulicRule)controlGroup1.Rules[0];
            var hydraulicCondition1A = (StandardCondition)controlGroup1.Conditions[0];
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
            RealTimeControlTestHelper.AddDummyLinksToGroup(null, controlGroup1);

            var controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            controlGroup2.Name = "controlGroup2";
            var hydraulicRule2A = (HydraulicRule)controlGroup2.Rules[0];
            var hydraulicCondition2A = (StandardCondition)controlGroup2.Conditions[0];
            hydraulicRule2A.Function[0.0] = 1.0;
            realTimeControlModel.ControlGroups.Add(controlGroup2);
            RealTimeControlTestHelper.AddDummyLinksToGroup(null, controlGroup2);

            hydraulicRule1A.Name = "rule1";
            hydraulicCondition1A.Name = "condition1";
            hydraulicRule2A.Name = "rule12";
            hydraulicCondition2A.Name = "condition2";
            var result = realTimeControlModel.Validate();
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
        public void ValidRealTimeControlModelWithConsistentRestartInputState()
        {
            var validRestartFilePath = TestHelper.GetTestFilePath("valid_state_RTC.zip");

            var model = CreateValidFilledRealTimeControlModel();

            model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
            model.UseRestart = true;

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidRealTimeControlModelWithConsistentRestartInputStateWithoutMetadata()
        {
            var validRestartFilePath =
                TestHelper.GetTestFilePath("valid_state_without_metadata_RTC.zip");

            var model = CreateValidFilledRealTimeControlModel();

            model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
            model.UseRestart = true;

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidRealTimeControlModelWithInconsistentRestartInputState()
        {
            var validRestartFilePath =
                TestHelper.GetTestFilePath("invalid_state_RTC.zip");

            var model = CreateValidFilledRealTimeControlModel();

            model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
            model.UseRestart = true;

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(4, validationResult.ErrorCount);
            var validationIssues = validationResult.AllErrors;
            Assert.IsTrue(validationIssues.All(vi => vi.Subject == "Input restart state"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfControlGroups: Value of '4' in restart state not matching expected value of '2' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfRulesPerControlGroups: Value of '2,7,' in restart state not matching expected value of '2,1,' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfConditionsPerControlGroups: Value of '2,9,' in restart state not matching expected value of '2,1,' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "ConditionTypesPerControlGroup: Missing"));
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidRealTimeControlModelWithInvalidModelTypeRestartInputState()
        {
            var validRestartFilePath =
                TestHelper.GetTestFilePath("invalid_ModelType_state_RTC.zip");

            var model = CreateValidFilledRealTimeControlModel();

            model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
            model.UseRestart = true;

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Model type of 'test' is not compatible.", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidRealTimeControlModelWithInvalidVersionRestartInputState()
        {
            var validRestartFilePath =
                TestHelper.GetTestFilePath("invalid_Version_state_RTC.zip");

            var model = CreateValidFilledRealTimeControlModel();

            model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
            model.UseRestart = true;

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Version 2 is not supported.", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidateRealTimeControlModelInputRestartStatePathIncorect()
        {
            var model = CreateValidFilledRealTimeControlModel();

            const string invalidPath = "invalidPath";
            var fileBasedRestartState = new FileBasedRestartState("test", invalidPath);
            ((IFileBased)fileBasedRestartState).Path = invalidPath;
            model.RestartInput = fileBasedRestartState;
            model.UseRestart = true;

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Model state file does not exist: " + invalidPath, validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidateRealTimeControlModelInputRestartStatePathToNonZip()
        {
            var filePathToNonZipFile =
                TestHelper.GetTestFilePath("NotAZipFile.txt");
            var model = CreateValidFilledRealTimeControlModel();

            var fileBasedRestartState = new FileBasedRestartState("test", filePathToNonZipFile);
            ((IFileBased)fileBasedRestartState).Path = filePathToNonZipFile;
            model.RestartInput = fileBasedRestartState;
            model.UseRestart = true;

            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.AreEqual("Model state file should be zip file and have the extension .zip", validationResult.AllErrors.First().Message);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        [Test]
        public void ValidateRealTimeControlModelRestartStateWithIntermediateRestartFiles()
        {
            var model = CreateValidFilledRealTimeControlModel();

            
            model.WriteRestart = true;
            model.UseSaveStateTimeRange = true;
            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime =  model.StopTime;
            model.SaveStateTimeStep =  model.TimeStep;


            var validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(1, validationResult.ErrorCount);
            Assert.That(validationResult.AllErrors.First().Message, Is.StringContaining("Currently, RTC models cannot create intermediate restart files. At the moment, a single restart file may only be written for the final time-step after a complete run."));
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        private static RealTimeControlModel CreateValidFilledRealTimeControlModel()
        {
            var model = new RealTimeControlModel();

            var controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
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
            RealTimeControlTestHelper.AddDummyLinksToGroup(null, controlGroup1);

            var controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            controlGroup2.Name = "controlGroup2";
            var hydraulicRule2A = (HydraulicRule) controlGroup2.Rules[0];
            var hydraulicCondition2A = (StandardCondition) controlGroup2.Conditions[0];
            hydraulicRule2A.Function[0.0] = 1.0;
            model.ControlGroups.Add(controlGroup2);
            RealTimeControlTestHelper.AddDummyLinksToGroup(null, controlGroup2);

            hydraulicRule1A.Name = "rule1";
            hydraulicCondition1A.Name = "condition1";
            hydraulicRule2A.Name = "rule12";
            hydraulicCondition2A.Name = "condition2";
            return model;
        }
    }
}
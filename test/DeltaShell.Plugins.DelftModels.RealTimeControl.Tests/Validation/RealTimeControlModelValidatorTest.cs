using System.Collections.Generic;
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
        [Test]
        public void ValidRealTimeControlModel()
        {
            RealTimeControlModel model = CreateValidRealTimeControlModel();

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
            RealTimeControlTestHelper.AddDummyLinksToGroup(null, controlGroup1);

            ControlGroup controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            controlGroup2.Name = "controlGroup2";
            var hydraulicRule2A = (HydraulicRule) controlGroup2.Rules[0];
            var hydraulicCondition2A = (StandardCondition) controlGroup2.Conditions[0];
            hydraulicRule2A.Function[0.0] = 1.0;
            realTimeControlModel.ControlGroups.Add(controlGroup2);
            RealTimeControlTestHelper.AddDummyLinksToGroup(null, controlGroup2);

            hydraulicRule1A.Name = "rule1";
            hydraulicCondition1A.Name = "condition1";
            hydraulicRule2A.Name = "rule12";
            hydraulicCondition2A.Name = "condition2";
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

        // TODO D3DFMIQ-2077
        //[Test]
        //public void ValidRealTimeControlModelWithConsistentRestartInputState()
        //{
        //    string validRestartFilePath = TestHelper.GetTestFilePath("valid_state_RTC.zip");

        //    RealTimeControlModel model = CreateValidFilledRealTimeControlModel();

        //    model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
        //    model.UseRestart = true;

        //    ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
        //    Assert.AreEqual(0, validationResult.ErrorCount);
        //    Assert.AreEqual(0, validationResult.WarningCount);
        //    Assert.AreEqual(0, validationResult.InfoCount);
        //}

        // TODO D3DFMIQ-2077
        //[Test]
        //public void ValidRealTimeControlModelWithConsistentRestartInputStateWithoutMetadata()
        //{
        //    string validRestartFilePath =
        //        TestHelper.GetTestFilePath("valid_state_without_metadata_RTC.zip");

        //    RealTimeControlModel model = CreateValidFilledRealTimeControlModel();

        //    model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
        //    model.UseRestart = true;

        //    ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
        //    Assert.AreEqual(0, validationResult.ErrorCount);
        //    Assert.AreEqual(0, validationResult.WarningCount);
        //    Assert.AreEqual(0, validationResult.InfoCount);
        //}

        // TODO D3DFMIQ-2077
        //[Test]
        //public void ValidRealTimeControlModelWithInconsistentRestartInputState()
        //{
        //    string validRestartFilePath =
        //        TestHelper.GetTestFilePath("invalid_state_RTC.zip");

        //    RealTimeControlModel model = CreateValidFilledRealTimeControlModel();

        //    model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
        //    model.UseRestart = true;

        //    ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
        //    Assert.AreEqual(4, validationResult.ErrorCount);
        //    IEnumerable<ValidationIssue> validationIssues = validationResult.AllErrors;
        //    Assert.IsTrue(validationIssues.All(vi => vi.Subject == "Input restart state"));
        //    Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfControlGroups: Value of '4' in restart state not matching expected value of '2' of current situation"));
        //    Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfRulesPerControlGroups: Value of '2,7,' in restart state not matching expected value of '2,1,' of current situation"));
        //    Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfConditionsPerControlGroups: Value of '2,9,' in restart state not matching expected value of '2,1,' of current situation"));
        //    Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "ConditionTypesPerControlGroup: Missing"));
        //    Assert.AreEqual(0, validationResult.WarningCount);
        //    Assert.AreEqual(0, validationResult.InfoCount);
        //}

        // TODO D3DFMIQ-2077
        //[Test]
        //public void ValidRealTimeControlModelWithInvalidModelTypeRestartInputState()
        //{
        //    string validRestartFilePath =
        //        TestHelper.GetTestFilePath("invalid_ModelType_state_RTC.zip");

        //    RealTimeControlModel model = CreateValidFilledRealTimeControlModel();

        //    model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
        //    model.UseRestart = true;

        //    ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
        //    Assert.AreEqual(1, validationResult.ErrorCount);
        //    Assert.AreEqual("Model type of 'test' is not compatible.", validationResult.AllErrors.First().Message);
        //    Assert.AreEqual(0, validationResult.WarningCount);
        //    Assert.AreEqual(0, validationResult.InfoCount);
        //}

        // TODO D3DFMIQ-2077
        //[Test]
        //public void ValidRealTimeControlModelWithInvalidVersionRestartInputState()
        //{
        //    string validRestartFilePath =
        //        TestHelper.GetTestFilePath("invalid_Version_state_RTC.zip");

        //    RealTimeControlModel model = CreateValidFilledRealTimeControlModel();

        //    model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
        //    model.UseRestart = true;

        //    ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
        //    Assert.AreEqual(1, validationResult.ErrorCount);
        //    Assert.AreEqual("Version 2 is not supported.", validationResult.AllErrors.First().Message);
        //    Assert.AreEqual(0, validationResult.WarningCount);
        //    Assert.AreEqual(0, validationResult.InfoCount);
        //}

        // TODO D3DFMIQ-2077
        //[Test]
        //public void ValidateRealTimeControlModelInputRestartStatePathIncorect()
        //{
        //    RealTimeControlModel model = CreateValidFilledRealTimeControlModel();

        //    const string invalidPath = "invalidPath";
        //    var fileBasedRestartState = new FileBasedRestartState("test", invalidPath);
        //    ((IFileBased) fileBasedRestartState).Path = invalidPath;
        //    model.RestartInput = fileBasedRestartState;
        //    model.UseRestart = true;

        //    ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
        //    Assert.AreEqual(1, validationResult.ErrorCount);
        //    Assert.AreEqual("Model state file does not exist: " + invalidPath, validationResult.AllErrors.First().Message);
        //    Assert.AreEqual(0, validationResult.WarningCount);
        //    Assert.AreEqual(0, validationResult.InfoCount);
        //}

        // TODO D3DFMIQ-2077
        //[Test]
        //public void ValidateRealTimeControlModelInputRestartStatePathToNonZip()
        //{
        //    string filePathToNonZipFile =
        //        TestHelper.GetTestFilePath("NotAZipFile.txt");
        //    RealTimeControlModel model = CreateValidFilledRealTimeControlModel();

        //    var fileBasedRestartState = new FileBasedRestartState("test", filePathToNonZipFile);
        //    ((IFileBased) fileBasedRestartState).Path = filePathToNonZipFile;
        //    model.RestartInput = fileBasedRestartState;
        //    model.UseRestart = true;

        //    ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
        //    Assert.AreEqual(1, validationResult.ErrorCount);
        //    Assert.AreEqual("Model state file should be zip file and have the extension .zip", validationResult.AllErrors.First().Message);
        //    Assert.AreEqual(0, validationResult.WarningCount);
        //    Assert.AreEqual(0, validationResult.InfoCount);
        //}

        [Test]
        public void ValidateRealTimeControlModelRestartStateWithIntermediateRestartFiles()
        {
            RealTimeControlModel model = CreateValidFilledRealTimeControlModel();

            model.WriteRestart = true;
            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.TimeStep;

            ValidationReport validationResult = new RealTimeControlModelValidator().Validate(model);
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);
        }

        private RealTimeControlModel CreateValidRealTimeControlModel()
        {
            var validRealTimeControlModel = new RealTimeControlModel();
            validRealTimeControlModel.ControlGroups.Add(ControlGroupValidatorTest.CreateValidControlGroup());
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
            RealTimeControlTestHelper.AddDummyLinksToGroup(null, controlGroup1);

            ControlGroup controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
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
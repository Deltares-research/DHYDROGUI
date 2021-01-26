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
    }
}
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlTestHelperTest
    {
        [Test]
        public void CompareMultipleInputs()
        {
            var left = new ControlGroup();
            left.Inputs.Add(new Input());
            left.Inputs.Add(new Input {SetPoint = "Input"});
            var right = new ControlGroup();
            right.Inputs.Add(new Input());
            right.Inputs.Add(new Input {SetPoint = "Input"});
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfControlGroups(left, right));
        }

        [Test]
        public void Compare2Identical()
        {
            ControlGroup left = RealTimeControlModelHelper.CreateGroupPidRule(true);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfControlGroups(left, left));
        }

        [Test]
        public void CreateGroup2RulesTest()
        {
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            Assert.AreEqual(3, controlGroup.Inputs.Count());
            Assert.AreEqual(1, controlGroup.Conditions.Count);
            Assert.AreEqual(2, controlGroup.Rules.Count);
            Assert.AreNotEqual(controlGroup.Conditions[0].FalseOutputs, controlGroup.Conditions[0].TrueOutputs);
        }

        [Test]
        public void CreateControlGroupWithTwoRulesOnOneOutput_ThenCorrectControlGroupIsCreated()
        {
            // Call
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateControlGroupWithTwoRulesOnOneOutput();

            // Assert
            Output output = controlGroup.Outputs.Single();

            IEventedList<RuleBase> rules = controlGroup.Rules;
            Assert.That(rules.Count, Is.EqualTo(2));
            Assert.That(rules.All(rule => rule.Outputs.Single().Equals(output)));

            IEventedList<ConditionBase> conditions = controlGroup.Conditions;
            Assert.That(conditions.Count, Is.EqualTo(2));
            Assert.That(conditions[0].TrueOutputs.Single().Equals(rules[0]));
            Assert.That(conditions[1].TrueOutputs.Single().Equals(rules[1]));
        }
    }
}
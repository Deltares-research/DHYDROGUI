using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class ControlGroupHelperTest
    {
        /// <summary>
        /// Tests InputItemsForOutput which gives the input items that determine an output item
        /// </summary>
        [Test]
        public void OutputForInputTest()
        {
            var rule = new PIDRule("testRule");
            var input = new Input { ParameterName = "InParam", Feature = new RtcTestFeature() };
            var output = new Output { ParameterName = "OutParam", Feature = new RtcTestFeature() };
            var controlGroup = new ControlGroup { Name = "testControlGroup" };
            var condition = new StandardCondition { Name = "testCondition", Input = input };
            rule.Outputs.Add(output);
            condition.Input = input;
            condition.FalseOutputs.Add(rule);
            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
            var inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, output);
            Assert.AreEqual(1, inputs.Count());
            Assert.AreEqual(input, inputs.FirstOrDefault());
        }

        /// <summary>
        /// Tests InputItemsForOutput which gives the input items that determine an output item
        /// </summary>
        [Test]
        public void OutputForInputTestDemo()
        {
            var controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            controlGroup.Inputs[0].ParameterName = "parameter0";
            controlGroup.Inputs[0].Feature = new RtcTestFeature { Name = "location0" };

            controlGroup.Inputs[1].ParameterName = "parameter1";
            controlGroup.Inputs[1].Feature = new RtcTestFeature { Name = "location1" };

            controlGroup.Inputs[2].ParameterName = "parameter2";
            controlGroup.Inputs[2].Feature = new RtcTestFeature { Name = "location2" };
            
            var outputIfTrue = controlGroup.Outputs[0];
            var inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, outputIfTrue);
            Assert.AreEqual(2, inputs.Count());
            Assert.IsNotNull(inputs.Where(i => i.Name.Contains("location0")).FirstOrDefault());
            Assert.IsNotNull(inputs.Where(i => i.Name.Contains("location1")).FirstOrDefault());

            var outputIfFalse = controlGroup.Outputs[1];
            inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, outputIfFalse);
            Assert.AreEqual(2, inputs.Count());
            Assert.IsNotNull(inputs.Where(i => i.Name.Contains("location1")).FirstOrDefault());
            Assert.IsNotNull(inputs.Where(i => i.Name.Contains("location2")).FirstOrDefault());
        }

        /// <summary>
        /// Test of InputItemsForOutput for multiple conditions set in series
        /// </summary>
        [Test]
        public void OutputForInputTestMultipleConditions()
        {
            var rule = new HydraulicRule();
            var input = new Input { ParameterName = "InParam", Feature = new RtcTestFeature { Name = "In" } };
            var output = new Output { ParameterName = "OutParam", Feature = new RtcTestFeature { Name = "Out" } };
            var controlGroup = new ControlGroup { Name = "testControlGroup" };
            var condition1 = new StandardCondition { Name = "Condition1", Input = input };
            var condition2 = new StandardCondition { Name = "Condition1" };
            var condition3 = new StandardCondition { Name = "Condition1" };

            rule.Outputs.Add(output);
            condition1.Input = input;
            condition1.TrueOutputs.Add(condition2);
            condition2.TrueOutputs.Add(condition3);
            condition3.TrueOutputs.Add(rule);
            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition1);
            controlGroup.Conditions.Add(condition2);
            controlGroup.Conditions.Add(condition3);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
            var inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, output);
            Assert.AreEqual(1, inputs.Count());
            Assert.AreEqual(input, inputs.FirstOrDefault());
        }

        [Test]
        public void InputItemsForOutputWithConditionWithoutInput()
        {
            var controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            controlGroup.Inputs[0].ParameterName = "parameter0";
            controlGroup.Inputs[0].Feature = new RtcTestFeature { Name = "location0" };

            controlGroup.Inputs[2].ParameterName = "parameter2";
            controlGroup.Inputs[2].Feature = new RtcTestFeature { Name = "location2" };

            controlGroup.Conditions[0].Input = null; //time condition has no input...

            var outputIfTrue = controlGroup.Outputs[0];
            var inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, outputIfTrue);

            Assert.AreEqual(1, inputs.Count());
        }

        [Test]
        public void InputItemsForOutputWithTwoConditionsToOneRelation()
        {
            var rule = new HydraulicRule();
            var input = new Input { ParameterName = "InParam", Feature = new RtcTestFeature() };
            var output = new Output { ParameterName = "OutParam", Feature = new RtcTestFeature() };
            var controlGroup = new ControlGroup { Name = "testControlGroup" };
            var condition1 = new StandardCondition { Name = "Condition1", Input = input };
            var condition2 = new StandardCondition { Name = "Condition2", Input = input };
            var condition3 = new StandardCondition { Name = "Condition3", Input = input };

            rule.Outputs.Add(output);
            condition1.Input = input;

            condition1.TrueOutputs.Add(condition2);
            condition1.FalseOutputs.Add(condition3);

            condition2.TrueOutputs.Add(rule);
            condition2.FalseOutputs.Add(condition3);

            condition3.TrueOutputs.Add(rule);

            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition1);
            controlGroup.Conditions.Add(condition2);
            controlGroup.Conditions.Add(condition3);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
            var inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, output);
            Assert.AreEqual(1, inputs.Count());
            Assert.AreEqual(input, inputs.FirstOrDefault());
        }

        [Test]
        public void GetConditionsOfARule()
        {
            var rule = new HydraulicRule();
            var controlGroup = new ControlGroup { Name = "testControlGroup" };

            var input = new Input { ParameterName = "InParam", Feature = new RtcTestFeature() };
            var output = new Output { ParameterName = "OutParam", Feature = new RtcTestFeature() };

            rule.Outputs.Add(output);

            var condition1 = new StandardCondition { Name = "Condition1", Input = input };
            var condition2 = new StandardCondition { Name = "Condition2", Input = input };
            var condition3 = new StandardCondition { Name = "Condition3", Input = input };

            condition1.Input = input;

            condition1.TrueOutputs.Add(condition2);
            condition1.FalseOutputs.Add(condition3);

            condition2.TrueOutputs.Add(rule);
            condition2.FalseOutputs.Add(condition3);

            condition3.TrueOutputs.Add(rule);

            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition1);
            controlGroup.Conditions.Add(condition2);
            controlGroup.Conditions.Add(condition3);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            var conditionsOfRule = ControlGroupHelper.ConditionsOfRule(controlGroup, rule);

            Assert.AreEqual(3,conditionsOfRule.Count());

            Assert.IsTrue(conditionsOfRule.Contains(condition1));
            Assert.IsTrue(conditionsOfRule.Contains(condition2));
            Assert.IsTrue(conditionsOfRule.Contains(condition3));

        }

        [Test]
        public void GetStartConditionOfARule()
        {
            var rule = new HydraulicRule();
            var controlGroup = new ControlGroup { Name = "testControlGroup" };
            var input = new Input { ParameterName = "InParam", Feature = new RtcTestFeature() };
            var output = new Output { ParameterName = "OutParam", Feature = new RtcTestFeature() };
            rule.Outputs.Add(output);

            var condition1 = new StandardCondition { Name = "Condition1", Input = input };
            var condition2 = new StandardCondition { Name = "Condition2", Input = input };
            var condition3 = new StandardCondition { Name = "Condition3", Input = input };

            condition1.Input = input;

            condition1.TrueOutputs.Add(condition2);
            condition1.FalseOutputs.Add(condition3);

            condition2.TrueOutputs.Add(rule);
            condition2.FalseOutputs.Add(condition3);

            condition3.TrueOutputs.Add(rule);

            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition1);
            controlGroup.Conditions.Add(condition2);
            controlGroup.Conditions.Add(condition3);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            var startObjectOfARule = ControlGroupHelper.StartObjectOfARule(controlGroup, rule);

            Assert.AreSame(condition1, startObjectOfARule);

        }

        [Test]
        public void SimpleStartConditionsOrRulesForOutputTest()
        {
            var controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            var sources = ControlGroupHelper.StartObjectsForOutput(controlGroup, controlGroup.Outputs[0]);
            Assert.AreEqual(1, sources.Count());
        }

        [Test]
        public void StartConditionsOrRulesForOutputExtraConditionTest()
        {
            var controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            var extraCondition = new StandardCondition();
            extraCondition.FalseOutputs.Add(controlGroup.Rules[0]);
            controlGroup.Conditions.Add(extraCondition);

            var sources = ControlGroupHelper.StartObjectsForOutput(controlGroup, controlGroup.Outputs[0]);
            Assert.AreEqual(2, sources.Count());
        }

        /// <summary>
        /// testcase based on 6.4.2 Hollandsche IJssel
        /// this scheme is invalid; multiple active paths
        /// 
        ///        C1 ------ C2 ------- R1
        ///     /                          \ 
        ///    /                            \
        /// level                              OpenH
        ///   \                            /
        ///    \                          / +
        ///       C-1 ----- C-2 -----  R2
        /// 
        /// </summary>
        [Test]
        public void ActivePathForHollandscheIJssel()
        {
            var controlGroup = new ControlGroup();

            var c1 = new StandardCondition { Name = "C1" };
            var c2 = new StandardCondition { Name = "C2" };
            var c_1 = new StandardCondition { Name = "C-1" };
            var c_2 = new StandardCondition { Name = "C-2" };
            var r1 = new HydraulicRule { Name = "R1" };
            var r2 = new HydraulicRule { Name = "R1" };
            var output = new Output();

            controlGroup.Rules.Add(r1);
            controlGroup.Rules.Add(r2);
            controlGroup.Conditions.Add(c1);
            controlGroup.Conditions.Add(c2);
            controlGroup.Conditions.Add(c_1);
            controlGroup.Conditions.Add(c_2);
            controlGroup.Outputs.Add(output);

            c1.TrueOutputs.Add(c2);
            c2.TrueOutputs.Add(r1);

            c_1.TrueOutputs.Add(c_2);
            c_2.TrueOutputs.Add(r2);

            r1.Outputs.Add(output);
            r2.Outputs.Add(output);

            Assert.IsTrue(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c1, r1));
            Assert.IsFalse(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c2, r1));
            Assert.IsFalse(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c_1, r1));
            Assert.IsFalse(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c_2, r1));

            Assert.IsFalse(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c1, r2));
            Assert.IsFalse(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c2, r2));
            Assert.IsTrue(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c_1, r2));
            Assert.IsFalse(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c_2, r2));

            Assert.AreEqual(2, ControlGroupHelper.StartObjectsForOutput(controlGroup, output).Count);

        }

        /// <summary>
        /// 
        /// 
        ///        C1 ------ R1------------
        ///     /   |                       \ 
        ///    /    |                        \
        /// level   |                           OpenH
        ///   \     |  False                /
        ///    \    V                      / +
        ///       C-1 -----  R2------------
        /// 
        /// </summary>
        [Test]
        public void ActivePathForTwoRules()
        {
            var controlGroup = new ControlGroup();

            var c1 = new StandardCondition { Name = "C1" };
            var c_1 = new StandardCondition { Name = "C-1" };
            var r1 = new HydraulicRule { Name = "R1" };
            var r2 = new HydraulicRule { Name = "R1" };
            var output = new Output();

            controlGroup.Rules.Add(r1);
            controlGroup.Rules.Add(r2);
            controlGroup.Conditions.Add(c1);
            controlGroup.Conditions.Add(c_1);
            controlGroup.Outputs.Add(output);

            c1.TrueOutputs.Add(r1);
            c1.FalseOutputs.Add(c_1);

            c_1.TrueOutputs.Add(r2);

            r1.Outputs.Add(output);
            r2.Outputs.Add(output);

            Assert.IsTrue(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c1, r1));
            Assert.IsFalse(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c_1, r2));
            Assert.IsTrue(ControlGroupHelper.IsActiveConditionForRule(controlGroup, c1, r2));

            Assert.AreEqual(1, ControlGroupHelper.StartObjectsForOutput(controlGroup, output).Count);

        }

        /// <summary>
        /// 
        /// 
        ///       C1 ------ R1----O1
        ///          
        /// 
        ///       C2 -----  R2----O2
        /// 
        /// </summary>
        [Test]
        public void ActivePathForTwoOutputs()
        {
            var controlGroup = new ControlGroup();

            var c1 = new StandardCondition { Name = "C1" };
            var c2 = new StandardCondition { Name = "C2" };
            var r1 = new HydraulicRule { Name = "R1" };
            var r2 = new HydraulicRule { Name = "R1" };
            var o1 = new Output();
            var o2 = new Output();

            controlGroup.Rules.Add(r1);
            controlGroup.Rules.Add(r2);
            controlGroup.Conditions.Add(c1);
            controlGroup.Conditions.Add(c2);
            controlGroup.Outputs.Add(o1);
            controlGroup.Outputs.Add(o2);

            c1.TrueOutputs.Add(r1);
            r1.Outputs.Add(o1);

            c2.TrueOutputs.Add(r2);
            r2.Outputs.Add(o2);

            Assert.AreEqual(1, ControlGroupHelper.StartObjectsForOutput(controlGroup, o1).Count);
            Assert.AreEqual(1, ControlGroupHelper.StartObjectsForOutput(controlGroup, o2).Count);

        }

    }
}

using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NSubstitute;
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
            var input = new Input
            {
                ParameterName = "InParam",
                Feature = new RtcTestFeature()
            };
            var output = new Output
            {
                ParameterName = "OutParam",
                Feature = new RtcTestFeature()
            };
            var controlGroup = new ControlGroup {Name = "testControlGroup"};
            var condition = new StandardCondition
            {
                Name = "testCondition",
                Input = input
            };
            rule.Outputs.Add(output);
            condition.Input = input;
            condition.FalseOutputs.Add(rule);
            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
            IEnumerable<Input> inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, output);
            Assert.AreEqual(1, inputs.Count());
            Assert.AreEqual(input, inputs.FirstOrDefault());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void InputItemsForOutput_ReturnsExpectedResult()
        {
            // Setup
            var input1 = new Input();
            var input2 = new Input();
            var input3 = new Input();
            var input4 = new Input();
            var input5 = new Input();
            var expression1 = new MathematicalExpression();
            var expression2 = new MathematicalExpression();
            var expression3 = new MathematicalExpression();
            var condition1 = Substitute.For<ConditionBase>();
            var condition2 = Substitute.For<ConditionBase>();
            var rule1 = Substitute.For<RuleBase>();
            var rule2 = Substitute.For<RuleBase>();
            var output = new Output();

            // Assemble
            expression1.Inputs.Add(input1);
            expression2.Inputs.Add(expression1);
            expression2.Inputs.Add(input2);
            condition1.Input = expression2;
            condition1.TrueOutputs.Add(condition2);
            condition1.FalseOutputs.Add(rule2);
            rule2.Inputs.Add(input5);
            condition2.Input = input3;
            condition2.FalseOutputs.Add(rule1);
            rule1.Inputs.Add(expression3);
            expression3.Inputs.Add(input4);
            rule1.Outputs.Add(output);

            ControlGroup controlGroup = CreateControlGroupWith(
                input1, input2, input3, input4, input5,
                expression1, expression2, expression3,
                condition1, condition2,
                rule1, rule2,
                output);

            // Call
            IEnumerable<Input> result = ControlGroupHelper.InputItemsForOutput(controlGroup, output);

            // Assert
            Input[] expectedResult =
            {
                input1,
                input2,
                input3,
                input4
            };
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        /// <summary>
        /// Tests InputItemsForOutput which gives the input items that determine an output item
        /// </summary>
        [Test]
        public void OutputForInputTestDemo()
        {
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            controlGroup.Inputs[0].ParameterName = "parameter0";
            controlGroup.Inputs[0].Feature = new RtcTestFeature {Name = "location0"};

            controlGroup.Inputs[1].ParameterName = "parameter1";
            controlGroup.Inputs[1].Feature = new RtcTestFeature {Name = "location1"};

            controlGroup.Inputs[2].ParameterName = "parameter2";
            controlGroup.Inputs[2].Feature = new RtcTestFeature {Name = "location2"};

            Output outputIfTrue = controlGroup.Outputs[0];
            IEnumerable<Input> inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, outputIfTrue);
            Assert.AreEqual(2, inputs.Count());
            Assert.IsNotNull(inputs.Where(i => i.Name.Contains("location0")).FirstOrDefault());
            Assert.IsNotNull(inputs.Where(i => i.Name.Contains("location1")).FirstOrDefault());

            Output outputIfFalse = controlGroup.Outputs[1];
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
            var input = new Input
            {
                ParameterName = "InParam",
                Feature = new RtcTestFeature {Name = "In"}
            };
            var output = new Output
            {
                ParameterName = "OutParam",
                Feature = new RtcTestFeature {Name = "Out"}
            };
            var controlGroup = new ControlGroup {Name = "testControlGroup"};
            var condition1 = new StandardCondition
            {
                Name = "Condition1",
                Input = input
            };
            var condition2 = new StandardCondition {Name = "Condition1"};
            var condition3 = new StandardCondition {Name = "Condition1"};

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
            IEnumerable<Input> inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, output);
            Assert.AreEqual(1, inputs.Count());
            Assert.AreEqual(input, inputs.FirstOrDefault());
        }

        [Test]
        public void InputItemsForOutputWithConditionWithoutInput()
        {
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            controlGroup.Inputs[0].ParameterName = "parameter0";
            controlGroup.Inputs[0].Feature = new RtcTestFeature {Name = "location0"};

            controlGroup.Inputs[2].ParameterName = "parameter2";
            controlGroup.Inputs[2].Feature = new RtcTestFeature {Name = "location2"};

            controlGroup.Conditions[0].Input = null; //time condition has no input...

            Output outputIfTrue = controlGroup.Outputs[0];
            IEnumerable<Input> inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, outputIfTrue);

            Assert.AreEqual(1, inputs.Count());
        }

        [Test]
        public void InputItemsForOutputWithTwoConditionsToOneRelation()
        {
            var rule = new HydraulicRule();
            var input = new Input
            {
                ParameterName = "InParam",
                Feature = new RtcTestFeature()
            };
            var output = new Output
            {
                ParameterName = "OutParam",
                Feature = new RtcTestFeature()
            };
            var controlGroup = new ControlGroup {Name = "testControlGroup"};
            var condition1 = new StandardCondition
            {
                Name = "Condition1",
                Input = input
            };
            var condition2 = new StandardCondition
            {
                Name = "Condition2",
                Input = input
            };
            var condition3 = new StandardCondition
            {
                Name = "Condition3",
                Input = input
            };

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
            IEnumerable<Input> inputs = ControlGroupHelper.InputItemsForOutput(controlGroup, output);
            Assert.AreEqual(1, inputs.Count());
            Assert.AreEqual(input, inputs.FirstOrDefault());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RetrieveTriggerObjects_1ConditionWithRule()
        {
            ControlGroup group = RealTimeControlModelHelper.CreateGroupPidRule(true);
            IList<RtcBaseObject> startTriggers = ControlGroupHelper.RetrieveTriggerObjects(group).ToList();

            Assert.AreSame(group.Conditions.Single(), startTriggers.Single());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RetrieveTriggerObjects_ConditionInsideConditionWithRule()
        {
            var controlGroup = new ControlGroup {Name = "Control group"};
            var ruleInput = new Input();
            var ruleOutput = new Output();
            var ruleOutput2 = new Output();
            var ruleOutput3 = new Output();

            controlGroup.Inputs.Add(ruleInput);
            controlGroup.Outputs.Add(ruleOutput);
            controlGroup.Outputs.Add(ruleOutput2);
            controlGroup.Outputs.Add(ruleOutput3);

            var pidRule = new PIDRule();
            var pidRule2 = new PIDRule();
            var pidRule3 = new PIDRule();
            controlGroup.Rules.Add(pidRule);
            controlGroup.Rules.Add(pidRule2);
            controlGroup.Rules.Add(pidRule3);

            pidRule.Inputs.Add(ruleInput);
            pidRule.Outputs.Add(ruleOutput);
            pidRule.Inputs.Add(ruleInput);
            pidRule.Outputs.Add(ruleOutput2);
            pidRule.Inputs.Add(ruleInput);
            pidRule.Outputs.Add(ruleOutput3);

            var conditionInput = new Input();
            var condition1 = new StandardCondition {Input = conditionInput};
            var condition2 = new StandardCondition {Input = conditionInput};
            controlGroup.Inputs.Add(conditionInput);
            condition1.TrueOutputs.Add(condition2);
            condition1.FalseOutputs.Add(pidRule);
            condition2.TrueOutputs.Add(pidRule2);
            condition2.FalseOutputs.Add(pidRule3);
            controlGroup.Conditions.Add(condition1);
            controlGroup.Conditions.Add(condition2);

            IList<RtcBaseObject> startTriggers = ControlGroupHelper.RetrieveTriggerObjects(controlGroup).ToList();

            Assert.AreSame(condition1, startTriggers.Single());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RetrieveTriggerObjects_TwoSeparatePaths()
        {
            var controlGroup = new ControlGroup();

            var c1 = new StandardCondition {Name = "C1"};
            var c2 = new StandardCondition {Name = "C2"};
            var r1 = new HydraulicRule {Name = "R1"};
            var r2 = new HydraulicRule {Name = "R1"};
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

            IList<RtcBaseObject> startTriggers = ControlGroupHelper.RetrieveTriggerObjects(controlGroup).ToList();
            Assert.AreEqual(2, startTriggers.Count);
            Assert.IsTrue(startTriggers.Contains(c1));
            Assert.IsTrue(startTriggers.Contains(c2));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RetrieveTriggerObjects_For1MathematicalExpressionBeforeRule()
        {
            var controlGroup = new ControlGroup {Name = "Control group"};

            var inputMathematicalExpression = new Input();
            var mathematicalExpression = new MathematicalExpression();
            mathematicalExpression.Inputs.Add(inputMathematicalExpression);
            controlGroup.MathematicalExpressions.Add(mathematicalExpression);

            var ruleOutput = new Output();
            controlGroup.Inputs.Add(inputMathematicalExpression);
            controlGroup.Outputs.Add(ruleOutput);

            var pidRule = new PIDRule();
            controlGroup.Rules.Add(pidRule);
            pidRule.Inputs.Add(mathematicalExpression);
            pidRule.Outputs.Add(ruleOutput);

            IList<RtcBaseObject> startTriggers = ControlGroupHelper.RetrieveTriggerObjects(controlGroup).ToList();

            Assert.AreSame(mathematicalExpression, startTriggers.Single());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RetrieveTriggerObjects_For2DifferentMathematicalExpressionsBefore2Rules()
        {
            var controlGroup = new ControlGroup {Name = "Control group"};

            var inputMathematicalExpression = new Input();
            var mathematicalExpression = new MathematicalExpression();
            mathematicalExpression.Inputs.Add(inputMathematicalExpression);
            controlGroup.MathematicalExpressions.Add(mathematicalExpression);

            var ruleOutput = new Output();
            controlGroup.Inputs.Add(inputMathematicalExpression);
            controlGroup.Outputs.Add(ruleOutput);

            var pidRule = new PIDRule();
            controlGroup.Rules.Add(pidRule);
            pidRule.Inputs.Add(mathematicalExpression);
            pidRule.Outputs.Add(ruleOutput);

            var mathematicalExpression2 = new MathematicalExpression();
            mathematicalExpression2.Inputs.Add(inputMathematicalExpression);
            controlGroup.MathematicalExpressions.Add(mathematicalExpression2);

            var ruleOutput2 = new Output();
            controlGroup.Outputs.Add(ruleOutput2);

            var pidRule2 = new PIDRule();
            controlGroup.Rules.Add(pidRule2);
            pidRule2.Inputs.Add(mathematicalExpression2);
            pidRule2.Outputs.Add(ruleOutput2);

            IList<RtcBaseObject> startTriggers = ControlGroupHelper.RetrieveTriggerObjects(controlGroup).ToList();

            Assert.AreEqual(2, startTriggers.Count);
            Assert.IsTrue(startTriggers.Contains(mathematicalExpression));
            Assert.IsTrue(startTriggers.Contains(mathematicalExpression2));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RetrieveTriggerObjects_For2MathematicalExpressionsBeforeRule()
        {
            var controlGroup = new ControlGroup {Name = "Control group"};

            var inputMathematicalExpression = new Input();
            var mathematicalExpression = new MathematicalExpression();
            mathematicalExpression.Inputs.Add(inputMathematicalExpression);
            controlGroup.MathematicalExpressions.Add(mathematicalExpression);

            var mathematicalExpression2 = new MathematicalExpression();
            mathematicalExpression2.Inputs.Add(mathematicalExpression);
            controlGroup.MathematicalExpressions.Add(mathematicalExpression2);

            var ruleOutput = new Output();
            controlGroup.Inputs.Add(inputMathematicalExpression);
            controlGroup.Outputs.Add(ruleOutput);

            var pidRule = new PIDRule();
            controlGroup.Rules.Add(pidRule);
            pidRule.Inputs.Add(mathematicalExpression2);
            pidRule.Outputs.Add(ruleOutput);

            IList<RtcBaseObject> startTriggers = ControlGroupHelper.RetrieveTriggerObjects(controlGroup).ToList();

            Assert.AreEqual(2, startTriggers.Count);
            Assert.IsTrue(startTriggers.Contains(mathematicalExpression));
            Assert.IsTrue(startTriggers.Contains(mathematicalExpression2));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RetrieveTriggerObjects_1ConditionWithMathematicalExpressionAsInput()
        {
            ControlGroup group = RealTimeControlModelHelper.CreateGroupPidRule(true);
            var mathematicalExpression = new MathematicalExpression();
            group.MathematicalExpressions.Add(mathematicalExpression);
            group.Conditions.First().Input = mathematicalExpression;

            IList<RtcBaseObject> startTriggers = ControlGroupHelper.RetrieveTriggerObjects(group).ToList();

            Assert.AreEqual(2, startTriggers.Count);
            Assert.IsTrue(startTriggers.Contains(mathematicalExpression));
            Assert.IsTrue(startTriggers.Contains(group.Conditions.Single()));
        }

        private static ControlGroup CreateControlGroupWith(params RtcBaseObject[] objects)
        {
            var controlGroup = new ControlGroup();

            foreach (RtcBaseObject obj in objects)
            {
                switch (obj)
                {
                    case ConditionBase condition:
                        controlGroup.Conditions.Add(condition);
                        break;
                    case Output output:
                        controlGroup.Outputs.Add(output);
                        break;
                    case Input input:
                        controlGroup.Inputs.Add(input);
                        break;
                    case MathematicalExpression expression:
                        controlGroup.MathematicalExpressions.Add(expression);
                        break;
                    case RuleBase rule:
                        controlGroup.Rules.Add(rule);
                        break;
                    case SignalBase signalBase:
                        controlGroup.Signals.Add(signalBase);
                        break;
                }
            }

            return controlGroup;
        }
    }
}
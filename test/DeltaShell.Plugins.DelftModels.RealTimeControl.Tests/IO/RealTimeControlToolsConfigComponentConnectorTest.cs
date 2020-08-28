using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils.AssertConstraints;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlToolsConfigComponentConnectorTest
    {
        private const string controlGroupName = "control_group_name";
        private const string outputName = "output_name";
        private const string componentName = "component_name";
        private readonly Random random = new Random();

        private RealTimeControlToolsConfigComponentConnector toolsConfigComponentConnector;
        private static readonly string inputName = $"{RtcXmlTag.Input}input_name";

        [SetUp]
        public void SetUp()
        {
            toolsConfigComponentConnector = new RealTimeControlToolsConfigComponentConnector(controlGroupName);
        }

        [TearDown]
        public void TearDown()
        {
            toolsConfigComponentConnector = null;
        }

        [Test]
        public void AssembleControlGroup_WithSignal_ReturnsCorrectControlGroup()
        {
            // Setup
            var signal = Substitute.For<SignalBase>();
            var sigalDAObj = new SignalDataAccessObject("id", signal);
            sigalDAObj.InputReferences.Add(inputName);

            // Call
            IControlGroup controlGroup = toolsConfigComponentConnector.AssembleControlGroup(new IRtcDataAccessObject<RtcBaseObject>[]
            {
                sigalDAObj
            });

            // Assert
            IEventedList<Input> inputs = signal.Inputs;
            Assert.That(inputs, Has.Count.EqualTo(1));
            Input input = inputs[0];
            Assert.That(input.Name, Is.EqualTo(inputName));

            Assert.That(controlGroup.Name, Is.EqualTo(controlGroupName));
            Assert.That(controlGroup.Inputs, Collection.OnlyContains(input));
            Assert.That(controlGroup.MathematicalExpressions, Is.Empty);
            Assert.That(controlGroup.Conditions, Is.Empty);
            Assert.That(controlGroup.Signals, Collection.OnlyContains(signal));
            Assert.That(controlGroup.Rules, Is.Empty);
            Assert.That(controlGroup.Outputs, Is.Empty);
        }

        [Test]
        public void AssembleControlGroup_WithRule_ReturnsCorrectControlGroup()
        {
            // Setup
            const string signalId = "signal_id";
            var signal = Substitute.For<SignalBase>();
            var signalDAObj = new SignalDataAccessObject(signalId, signal);

            var rule = Substitute.For<RuleBase>();
            var ruleDAObj = new RuleDataAccessObject("id", rule);
            ruleDAObj.InputReferences.Add(inputName);
            ruleDAObj.SignalReferences.Add(signalId);
            ruleDAObj.OutputReferences.Add(outputName);

            // Call
            IControlGroup controlGroup = toolsConfigComponentConnector.AssembleControlGroup(new IRtcDataAccessObject<RtcBaseObject>[]
            {
                ruleDAObj,
                signalDAObj
            });

            // Assert
            IEventedList<Output> outputs = rule.Outputs;
            Assert.That(outputs, Has.Count.EqualTo(1));
            Output output = outputs[0];
            Assert.That(output.Name, Is.EqualTo(outputName));

            IEventedList<IInput> inputs = rule.Inputs;
            Assert.That(inputs, Has.Count.EqualTo(1));
            var input = (Input) inputs[0];
            Assert.That(input.Name, Is.EqualTo(inputName));

            Assert.That(signal.RuleBases, Collection.OnlyContains(rule));

            Assert.That(controlGroup.Name, Is.EqualTo(controlGroupName));
            Assert.That(controlGroup.Inputs, Collection.OnlyContains(input));
            Assert.That(controlGroup.MathematicalExpressions, Is.Empty);
            Assert.That(controlGroup.Conditions, Is.Empty);
            Assert.That(controlGroup.Signals, Collection.OnlyContains(signal));
            Assert.That(controlGroup.Rules, Collection.OnlyContains(rule));
            Assert.That(controlGroup.Outputs, Collection.OnlyContains(output));
        }

        [Test]
        public void AssembleControlGroup_WithRule_ExpressionAsInput_ReturnsCorrectControlGroup()
        {
            // Setup
            const string expressionName = "expression_name";
            var mathematicalExpression = new MathematicalExpression {Name = expressionName};
            var expressionTree = new ExpressionTree(Substitute.For<IBranchNode>(), "", "",
                                                    mathematicalExpression);

            var rule = Substitute.For<RuleBase>();
            var ruleDAObj = new RuleDataAccessObject("id", rule);
            ruleDAObj.InputReferences.Add(expressionName);
            ruleDAObj.OutputReferences.Add(outputName);

            // Call
            IControlGroup controlGroup = toolsConfigComponentConnector.AssembleControlGroup(new IRtcDataAccessObject<RtcBaseObject>[]
            {
                ruleDAObj,
                expressionTree
            });

            // Assert
            IEventedList<Output> outputs = rule.Outputs;
            Assert.That(outputs, Has.Count.EqualTo(1));
            Output output = outputs[0];
            Assert.That(output.Name, Is.EqualTo(outputName));

            Assert.That(rule.Inputs, Collection.OnlyContains(mathematicalExpression));

            Assert.That(controlGroup.Name, Is.EqualTo(controlGroupName));
            Assert.That(controlGroup.Inputs, Is.Empty);
            Assert.That(controlGroup.MathematicalExpressions, Collection.OnlyContains(mathematicalExpression));
            Assert.That(controlGroup.Conditions, Is.Empty);
            Assert.That(controlGroup.Signals, Is.Empty);
            Assert.That(controlGroup.Rules, Collection.OnlyContains(rule));
            Assert.That(controlGroup.Outputs, Collection.OnlyContains(output));
        }

        [Test]
        public void AssembleControlGroup_WithCondition_ReturnsCorrectControlGroup()
        {
            // Setup
            ConditionDataAccessObject conditionDaObject = GetConditionWithTrueAndFalseOutput(
                out IRtcDataAccessObject<RtcBaseObject> trueOutputDaObject,
                out IRtcDataAccessObject<RtcBaseObject> falseOutputDaObject);

            conditionDaObject.InputReferences.Add(inputName);

            // Call
            IControlGroup controlGroup = toolsConfigComponentConnector.AssembleControlGroup(new[]
            {
                trueOutputDaObject,
                falseOutputDaObject,
                conditionDaObject
            });

            // Assert
            ConditionBase condition = conditionDaObject.Object;
            var input = (Input) condition.Input;
            Assert.That(input.Name, Is.EqualTo(inputName));

            Assert.That(condition.TrueOutputs, Collection.OnlyContains(trueOutputDaObject.Object));
            Assert.That(condition.FalseOutputs, Collection.OnlyContains(falseOutputDaObject.Object));

            Assert.That(controlGroup.Name, Is.EqualTo(controlGroupName));
            Assert.That(controlGroup.Inputs, Collection.OnlyContains(input));
            Assert.That(controlGroup.MathematicalExpressions, Is.Empty);
            Assert.That(controlGroup.Conditions, Collection.OnlyContains(condition));
            Assert.That(controlGroup.Signals, Is.Empty);
            Assert.That(controlGroup.Rules, Is.Empty);
            Assert.That(controlGroup.Outputs, Is.Empty);
        }

        [Test]
        public void AssembleControlGroup_WithCondition_ExpressionAsInput_ReturnsCorrectControlGroup()
        {
            // Setup
            const string expressionName = "expression_name";
            var mathematicalExpression = new MathematicalExpression {Name = expressionName};
            var expressionTree = new ExpressionTree(Substitute.For<IBranchNode>(), "", "",
                                                    mathematicalExpression);

            var condition = Substitute.For<ConditionBase>();
            var conditionDAObj = new ConditionDataAccessObject("id", condition);
            conditionDAObj.InputReferences.Add(expressionName);

            // Call
            IControlGroup controlGroup = toolsConfigComponentConnector.AssembleControlGroup(
                new IRtcDataAccessObject<RtcBaseObject>[]
                {
                    conditionDAObj,
                    expressionTree
                });

            // Assert
            Assert.That(condition.Input, Is.SameAs(mathematicalExpression));
            Assert.That(condition.TrueOutputs, Is.Empty);
            Assert.That(condition.FalseOutputs, Is.Empty);

            Assert.That(controlGroup.Name, Is.EqualTo(controlGroupName));
            Assert.That(controlGroup.Inputs, Is.Empty);
            Assert.That(controlGroup.MathematicalExpressions, Collection.OnlyContains(mathematicalExpression));
            Assert.That(controlGroup.Conditions, Collection.OnlyContains(condition));
            Assert.That(controlGroup.Signals, Is.Empty);
            Assert.That(controlGroup.Rules, Is.Empty);
            Assert.That(controlGroup.Outputs, Is.Empty);
        }

        [Test]
        public void AssembleControlGroup_WithMathematicalExpression_ReturnsCorrectControlGroup()
        {
            // Setup
            const string referenceExpressionName = "expression_ref";
            var referenceExpression = new MathematicalExpression {Name = referenceExpressionName};
            var referenceExpressionTree = new ExpressionTree(Substitute.For<IBranchNode>(), "", "",
                                                             referenceExpression);

            var rootNode = Substitute.For<IBranchNode>();
            rootNode.GetChildNodes().Returns(new List<IExpressionNode>
            {
                new ParameterLeafNode(referenceExpressionName),
                new ParameterLeafNode(inputName),
                new ConstantValueLeafNode(random.Next().ToString())
            });
            const string expression = "expression_string";
            rootNode.GetExpression().Returns(expression);

            var mathematicalExpression = new MathematicalExpression();
            var expressionTree = new ExpressionTree(rootNode, "", "",
                                                    mathematicalExpression);

            // Call
            IControlGroup controlGroup = toolsConfigComponentConnector.AssembleControlGroup(new IRtcDataAccessObject<RtcBaseObject>[]
            {
                referenceExpressionTree,
                expressionTree
            });

            // Assert
            IEventedList<IInput> inputs = mathematicalExpression.Inputs;
            Assert.That(inputs, Has.Count.EqualTo(2));

            IInput expressionInput = inputs.FirstOrDefault(i => i is MathematicalExpression);
            Assert.That(expressionInput, Is.Not.Null);
            IInput inputInput = inputs.FirstOrDefault(i => i is Input);
            Assert.That(inputInput, Is.Not.Null);

            Assert.That(controlGroup.Name, Is.EqualTo(controlGroupName));
            Assert.That(controlGroup.Inputs, Collection.OnlyContains(inputInput));
            Assert.That(controlGroup.MathematicalExpressions, Is.EquivalentTo(new[]
            {
                mathematicalExpression,
                expressionInput
            }));
            Assert.That(controlGroup.Conditions, Is.Empty);
            Assert.That(controlGroup.Signals, Is.Empty);
            Assert.That(controlGroup.Rules, Is.Empty);
            Assert.That(controlGroup.Outputs, Is.Empty);

            Assert.That(mathematicalExpression.Expression, Is.EqualTo(expression));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AssembleControlGroup_WithSeveralRtcObjects_ReturnsCorrectControlGroup()
        {
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = CreateExampleSet(
                out MathematicalExpression expression,
                out ConditionBase condition,
                out SignalBase signal,
                out RuleBase rule1,
                out RuleBase rule2);

            // Call
            IControlGroup controlGroup = toolsConfigComponentConnector.AssembleControlGroup(dataAccessObjects);

            // Assert
            IInput input = condition.Input;
            Output output = rule1.Outputs[0];

            Assert.That(input.Name, Is.EqualTo(inputName));
            Assert.That(condition.TrueOutputs, Collection.OnlyContains(rule2));
            Assert.That(condition.FalseOutputs, Collection.OnlyContains(expression));
            Assert.That(expression.Inputs, Collection.OnlyContains(input));
            Assert.That(rule1.Inputs, Collection.OnlyContains(expression));
            Assert.That(rule1.Outputs, Collection.OnlyContains(output));
            Assert.That(signal.Inputs, Collection.OnlyContains(input));
            Assert.That(signal.RuleBases, Collection.OnlyContains(rule1));
            Assert.That(rule2.Inputs, Collection.OnlyContains(expression));
            Assert.That(rule2.Outputs, Collection.OnlyContains(output));

            Assert.That(controlGroup.Inputs, Collection.OnlyContains(input));
            Assert.That(controlGroup.MathematicalExpressions, Collection.OnlyContains(expression));
            Assert.That(controlGroup.Conditions, Collection.OnlyContains(condition));
            Assert.That(controlGroup.Signals, Collection.OnlyContains(signal));
            Assert.That(controlGroup.Rules, Is.EquivalentTo(new[]
            {
                rule1,
                rule2
            }));
            Assert.That(controlGroup.Outputs, Collection.OnlyContains(output));
        }

        private static IRtcDataAccessObject<RtcBaseObject>[] CreateExampleSet(out MathematicalExpression expression,
                                                                              out ConditionBase condition,
                                                                              out SignalBase signal,
                                                                              out RuleBase rule1,
                                                                              out RuleBase rule2)
        {
            const string expressionName = "expression_name";
            const string expressionId = "expression_id";
            const string conditionId = "condition_id";
            const string signalId = "signal_id";
            const string ruleId1 = "rule_id1";
            const string ruleId2 = "rule_id2";

            expression = new MathematicalExpression {Name = expressionName};
            condition = Substitute.For<ConditionBase>();
            signal = Substitute.For<SignalBase>();
            rule1 = Substitute.For<RuleBase>();
            rule2 = Substitute.For<RuleBase>();

            var rootNode = Substitute.For<IBranchNode>();
            rootNode.GetChildNodes().Returns(new List<IExpressionNode>
            {
                new ParameterLeafNode(inputName)
            });

            var expressionTree = new ExpressionTree(rootNode, "", expressionId, expression);

            var conditionDAObj = new ConditionDataAccessObject(conditionId, condition);
            conditionDAObj.InputReferences.Add(inputName);
            conditionDAObj.TrueOutputReferences.Add(ruleId2);
            conditionDAObj.FalseOutputReferences.Add(expressionId);

            var signalDAObj = new SignalDataAccessObject(signalId, signal);
            signalDAObj.InputReferences.Add(inputName);

            var rule1DAObj = new RuleDataAccessObject(ruleId1, rule1);
            rule1DAObj.InputReferences.Add(expressionName);
            rule1DAObj.SignalReferences.Add(signalId);
            rule1DAObj.OutputReferences.Add(outputName);

            var rule2DAObj = new RuleDataAccessObject(ruleId2, rule2);
            rule2DAObj.InputReferences.Add(expressionName);
            rule2DAObj.OutputReferences.Add(outputName);

            return new IRtcDataAccessObject<RtcBaseObject>[]
            {
                expressionTree,
                conditionDAObj,
                signalDAObj,
                rule1DAObj,
                rule2DAObj
            };
        }

        private static ConditionDataAccessObject GetConditionWithTrueAndFalseOutput(
            out IRtcDataAccessObject<RtcBaseObject> trueOutputDaObject,
            out IRtcDataAccessObject<RtcBaseObject> falseOutputDAObj
        )
        {
            const string trueOutputId = "true_output_id";
            trueOutputDaObject = CreateRtcDataAccessObject(trueOutputId);

            const string falseOutputId = "false_output_id";
            falseOutputDAObj = CreateRtcDataAccessObject(falseOutputId);

            var condition = new StandardCondition {Name = componentName};
            var conditionDAObj = new ConditionDataAccessObject("id", condition);
            conditionDAObj.TrueOutputReferences.Add(trueOutputId);
            conditionDAObj.FalseOutputReferences.Add(falseOutputId);

            return conditionDAObj;
        }

        private static IRtcDataAccessObject<RtcBaseObject> CreateRtcDataAccessObject(string id)
        {
            var dataAccessObject = Substitute.For<IRtcDataAccessObject<RtcBaseObject>>();
            dataAccessObject.Id.Returns(id);
            dataAccessObject.Object.Returns(Substitute.For<RtcBaseObject>());

            return dataAccessObject;
        }
    }
}
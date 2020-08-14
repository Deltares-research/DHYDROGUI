using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Graph;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using GeoAPI.Extensions.Feature;
using log4net;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlModelCopyPasteHelperShadowTest
    {
        [SetUp]
        public void Setup()
        {
            // As the helper is a singleton, reset its state before every test begins
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
            helper.ClearData();

            // Precondition
            Assert.That(helper.CopiedShapes, Is.Empty);
            Assert.That(helper.IsDataSet, Is.False);
        }

        [Test]
        public void Instance_Always_ReturnsSameInstance()
        {
            // Call
            RealTimeControlModelCopyPasteHelperShadow firstInstance = RealTimeControlModelCopyPasteHelperShadow.Instance;
            RealTimeControlModelCopyPasteHelperShadow secondInstance = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Assert
            Assert.That(firstInstance, Is.SameAs(secondInstance));
        }

        [Test]
        public void Instance_ExpectedProperties()
        {
            // Call
            RealTimeControlModelCopyPasteHelperShadow instance = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Assert
            Assert.That(instance.CopiedShapes, Is.Empty);
            Assert.That(instance.IsDataSet, Is.False);
        }

        [Test]
        public void SetCopiedData_ShapesNull_ThrowsArgumentNullException()
        {
            // Setup
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Call
            TestDelegate call = () => helper.SetCopiedData(null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("shapes"));
        }

        [Test]
        public void SetCopiedData_CollectionEmpty_SetsCopiedShapesAndIsDataSetFalse()
        {
            // Setup
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Call
            helper.SetCopiedData(Enumerable.Empty<ShapeBase>());

            // Assert
            Assert.That(helper.IsDataSet, Is.False);
            Assert.That(helper.CopiedShapes, Is.Empty);
        }

        [Test]
        public void SetCopiedData_CollectionNotEmpty_SetsCopiedShapesAndIsDataSetTrue()
        {
            // Setup
            var shapes = new[] {new TestShape(), new TestShape(), new TestShape()};
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Call
            helper.SetCopiedData(shapes);

            // Assert
            CollectionAssert.AreEqual(shapes, helper.CopiedShapes);
            Assert.That(helper.IsDataSet, Is.True);
        }

        [Test]
        public void GivenHelperWithSetData_WhenClearingCopiedData_ThenDataIsClearedAndDataSetFalse()
        {
            // Given
            var shapes = new[] {new TestShape(), new TestShape(), new TestShape()};
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            helper.SetCopiedData(shapes);

            // Precondition
            Assert.That(helper.CopiedShapes, Is.Not.Empty);

            // When
            helper.ClearData();

            // Then
            Assert.That(helper.IsDataSet, Is.False);
            Assert.That(helper.CopiedShapes, Is.Empty);
        }

        [Test]
        public void CopyShapesToController_ControllerNull_ThrowsArgumentNullException()
        {
            // Setup
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Call
            TestDelegate call = () => helper.CopyShapesToController(null, Point.Empty);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("controller"));
        }

        [Test]
        public void GivenHelperWithoutData_WhenCopyShapesToController_ThenNoChange()
        {
            // Given
            using (var controlGroupEditor = new ControlGroupEditor {Data = new ControlGroup()})
            {
                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

                // Precondition
                Assert.That(helper.CopiedShapes, Is.Empty);
                Assert.That(helper.IsDataSet, Is.False);

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes, Is.Empty);
            }
        }

        [Test]
        public void GivenHelperWithOutputData_WhenCopyShapesToController_ThenMessageLoggedAndCopiedOutputReset()
        {
            // Given
            IFeature outputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var output = new Output
            {
                Name = "Output",
                Feature = outputFeature
            };

            var controlGroup = new ControlGroup();
            controlGroup.Outputs.Add(output);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(1));

                // When
                Action call = () => helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                string expectedMessage = $"It is not possible to copy and paste internal data for control group outputs, the connection to {output.Name} will be reset.";
                TestHelper.AssertLogMessageIsGenerated(call, expectedMessage);

                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(2));

                IEnumerable<OutputItemShape> outputShapes = actualShapes.OfType<OutputItemShape>();
                Assert.That(outputShapes.Count(), Is.EqualTo(2));
                IEnumerable<Output> actualOutputs = outputShapes.Select(s => s.Tag).Cast<Output>();

                Output originalOutput = actualOutputs.Single(o => string.Equals(o.Name, output.Name));
                Assert.That(originalOutput.Feature, Is.SameAs(outputFeature));

                Output copiedOutput = actualOutputs.Single(o => string.Equals(o.Name, "[Not Set]"));
                Assert.That(copiedOutput.Feature, Is.Null);
            }
        }

        [Test]
        public void GivenHelperWithRuleBasedData_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Given
            IFeature inputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var input = new Input
            {
                Name = "Input",
                Feature = inputFeature
            };

            IFeature outputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var output = new Output
            {
                Name = "Output",
                Feature = outputFeature
            };

            const string ruleName = "Rule";
            var clonedRule = Substitute.For<RuleBase>();
            clonedRule.Name = ruleName;
            clonedRule.Inputs = new EventedList<IInput>();
            clonedRule.Outputs = new EventedList<Output>();

            var rule = Substitute.For<RuleBase>();
            rule.Name = ruleName;
            rule.Inputs = new EventedList<IInput>(new[] {input});
            rule.Outputs = new EventedList<Output>(new[] {output});
            rule.Clone().Returns(clonedRule);

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(rule);
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(3));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(6));

                const int expectedNrOfInputs = 2;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                AssertInputs(inputs, input, expectedNrOfInputs);

                IEnumerable<OutputItemShape> outputShapes = actualShapes.OfType<OutputItemShape>();
                Assert.That(outputShapes.Count(), Is.EqualTo(2));
                IEnumerable<Output> actualOutputs = outputShapes.Select(s => s.Tag).Cast<Output>();

                Output originalOutput = actualOutputs.Single(o => string.Equals(o.Name, output.Name));
                Assert.That(originalOutput, Is.SameAs(output));
                Assert.That(originalOutput.Feature, Is.SameAs(outputFeature));

                Output copiedOutput = actualOutputs.Single(o => string.Equals(o.Name, "[Not Set]"));
                Assert.That(copiedOutput, Is.Not.SameAs(output));
                Assert.That(copiedOutput.Feature, Is.Null);

                IEnumerable<RuleShape> ruleShapes = actualShapes.OfType<RuleShape>();
                Assert.That(ruleShapes.Count(), Is.EqualTo(2));

                IEnumerable<RuleBase> rules = ruleShapes.Select(r => r.Tag).Cast<RuleBase>();
                RuleBase originalRule = rules.Single(r => string.Equals(r.Name, rule.Name));
                Assert.That(originalRule, Is.SameAs(rule));
                CollectionAssert.AreEqual(rule.Inputs, originalRule.Inputs);
                CollectionAssert.AreEqual(rule.Outputs, originalRule.Outputs);

                RuleBase copiedRule = rules.Single(r => string.Equals(r.Name, "Rule - Copy 1"));
                Assert.That(copiedRule, Is.Not.SameAs(rule));
                Assert.That(copiedRule.Inputs.Single(), Is.Not.SameAs(input)); // There are only two inputs present, therefore the new rule should not match the original input
                Assert.That(copiedRule.Outputs.Single(), Is.SameAs(copiedOutput));
            }
        }

        [Test]
        public void GivenHelperWithSignalBasedData_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Given
            IFeature inputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var input = new Input
            {
                Name = "Input",
                Feature = inputFeature
            };

            const string ruleName = "Rule";
            var clonedRule = Substitute.For<RuleBase>();
            clonedRule.Name = ruleName;

            var rule = Substitute.For<RuleBase>();
            rule.Name = ruleName;
            rule.Clone().Returns(clonedRule);

            const string signalName = "Signal";
            var clonedSignal = Substitute.For<SignalBase>();
            clonedSignal.Name = signalName;

            var signal = Substitute.For<SignalBase>();
            signal.Name = signalName;
            signal.Inputs = new EventedList<Input>(new[] {input});
            signal.RuleBases = new EventedList<RuleBase>(new[] {rule});
            signal.Clone().Returns(clonedSignal);

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);
            controlGroup.Signals.Add(signal);
            controlGroup.Rules.Add(rule);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(3));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(6));

                const int expectedNrOfInputs = 2;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                AssertInputs(inputs, input, expectedNrOfInputs);

                IEnumerable<RuleShape> ruleShapes = actualShapes.OfType<RuleShape>();
                Assert.That(ruleShapes.Count(), Is.EqualTo(2));
                IEnumerable<RuleBase> rules = ruleShapes.Select(r => r.Tag).Cast<RuleBase>();
                AssertRuleWithoutInputsAndOutputs(rules, ruleName, rule, true);
                AssertRuleWithoutInputsAndOutputs(rules, "Rule - Copy 1", rule);

                IEnumerable<SignalShape> signalShapes = actualShapes.OfType<SignalShape>();
                Assert.That(signalShapes.Count(), Is.EqualTo(2));
                IEnumerable<SignalBase> actualSignals = signalShapes.Select(r => r.Tag).Cast<SignalBase>();

                SignalBase originalSignal = actualSignals.Single(s => string.Equals(s.Name, signal.Name));
                Assert.That(originalSignal, Is.SameAs(signal));
                CollectionAssert.AreEqual(signal.Inputs, originalSignal.Inputs);
                CollectionAssert.AreEqual(signal.RuleBases, originalSignal.RuleBases);

                SignalBase copiedSignal = actualSignals.Single(s => string.Equals(s.Name, "Signal - Copy 1"));
                Assert.That(copiedSignal, Is.Not.SameAs(signal));
                Assert.That(copiedSignal.Inputs.Single(), Is.Not.SameAs(input));   // There are only two inputs present, therefore the new rule should not match the original input
                Assert.That(copiedSignal.RuleBases.Single(), Is.Not.SameAs(rule)); // Similar for the rules
            }
        }

        [Test]
        public void GivenHelperWithMathematicalExpressionData_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Given
            IFeature inputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var input = new Input
            {
                Name = "Input",
                Feature = inputFeature
            };

            var expression = new MathematicalExpression
            {
                Name = "Expression",
                Expression = "Potato"
            };
            expression.Inputs.Add(input);

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);
            controlGroup.MathematicalExpressions.Add(expression);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(2));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(4));

                const int expectedNrOfInputs = 2;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                AssertInputs(inputs, input, expectedNrOfInputs);

                IEnumerable<MathematicalExpressionShape> mathematicalExpressionShapes = actualShapes.OfType<MathematicalExpressionShape>();
                Assert.That(mathematicalExpressionShapes.Count(), Is.EqualTo(2));

                IEnumerable<MathematicalExpression> actualMathematicalExpressions = mathematicalExpressionShapes.Select(s => s.Tag).Cast<MathematicalExpression>();
                Assert.That(actualMathematicalExpressions.All(e => string.Equals(e.Expression, expression.Expression)), Is.True);

                MathematicalExpression originalExpression = actualMathematicalExpressions.Single(o => string.Equals(o.Name, expression.Name));
                Assert.That(originalExpression, Is.SameAs(expression));
                CollectionAssert.AreEqual(expression.Inputs, originalExpression.Inputs);

                MathematicalExpression copiedExpression = actualMathematicalExpressions.Single(s => string.Equals(s.Name, "Expression - Copy 1"));
                Assert.That(copiedExpression, Is.Not.SameAs(expression));
                Assert.That(copiedExpression.Inputs.Single(), Is.Not.SameAs(input)); // There are only two inputs present, therefore the new rule should not match the original input
            }
        }

        [Test]
        public void GivenHelperWithConditionBasedNothingConnected_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Given
            const string conditionName = "Condition";
            var clonedCondition = Substitute.For<ConditionBase>();
            clonedCondition.Name = conditionName;

            var conditionBase = Substitute.For<ConditionBase>();
            conditionBase.Name = conditionName;
            conditionBase.Clone().Returns(clonedCondition);

            var controlGroup = new ControlGroup();
            controlGroup.Conditions.Add(conditionBase);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(1));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(2));

                IEnumerable<ConditionShape> conditionShapes = actualShapes.OfType<ConditionShape>();
                Assert.That(conditionShapes.Count(), Is.EqualTo(2));

                IEnumerable<ConditionBase> actualConditions = conditionShapes.Select(r => r.Tag).Cast<ConditionBase>();
                Assert.That(actualConditions.All(c => c.Input == null), Is.True);
                Assert.That(actualConditions.SelectMany(c => c.TrueOutputs), Is.Empty); // No outputs are present and should remain empty
                Assert.That(actualConditions.SelectMany(c => c.FalseOutputs), Is.Empty);

                ConditionBase originalCondition = actualConditions.Single(c => string.Equals(c.Name, conditionBase.Name));
                Assert.That(originalCondition, Is.SameAs(conditionBase));

                ConditionBase copiedCondition = actualConditions.Single(c => string.Equals(c.Name, "Condition - Copy 1"));
                Assert.That(copiedCondition, Is.Not.SameAs(conditionBase));
            }
        }

        [Test]
        public void GivenHelperWithConditionBaseConnectedWithRules_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Note the ConditionBase only accepts:
            // - RuleBase
            // - MathematicalExpression
            // - ConditionBase
            // as valid objects for the TrueOutputs and FalseOutputs collection.
            // 
            // As such, the RTC objects Input and Output are not considered as valid items and are ignored for the tests.

            // Given
            IFeature inputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var input = new Input
            {
                Name = "Input",
                Feature = inputFeature
            };

            const string ruleTrueOutputName = "RuleTrueOutput";
            var clonedTrueOutputRule = Substitute.For<RuleBase>();
            clonedTrueOutputRule.Name = ruleTrueOutputName;

            var ruleTrueOutput = Substitute.For<RuleBase>();
            ruleTrueOutput.Name = ruleTrueOutputName;
            ruleTrueOutput.Clone().Returns(clonedTrueOutputRule);

            const string ruleFalseOutputName = "RuleFalseOutput";
            var clonedFalseOutputRule = Substitute.For<RuleBase>();
            clonedFalseOutputRule.Name = ruleFalseOutputName;

            var ruleFalseOutput = Substitute.For<RuleBase>();
            ruleFalseOutput.Name = ruleFalseOutputName;
            ruleFalseOutput.Clone().Returns(clonedFalseOutputRule);

            const string conditionName = "Condition";
            var clonedCondition = Substitute.For<ConditionBase>();
            clonedCondition.Name = conditionName;

            var conditionBase = Substitute.For<ConditionBase>();
            conditionBase.Name = conditionName;
            conditionBase.Input = input;
            conditionBase.TrueOutputs.Add(ruleTrueOutput);
            conditionBase.FalseOutputs.Add(ruleFalseOutput);
            conditionBase.Clone().Returns(clonedCondition);

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);
            controlGroup.Conditions.Add(conditionBase);
            controlGroup.Rules.AddRange(new[] {ruleTrueOutput, ruleFalseOutput});

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(4));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(8));

                const int expectedNrOfInputs = 2;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                AssertInputs(inputs, input, expectedNrOfInputs);

                IEnumerable<RuleShape> ruleShapes = actualShapes.OfType<RuleShape>();
                Assert.That(ruleShapes.Count(), Is.EqualTo(4));
                IEnumerable<RuleBase> rules = ruleShapes.Select(r => r.Tag).Cast<RuleBase>();
                AssertRuleWithoutInputsAndOutputs(rules, ruleTrueOutputName, ruleTrueOutput, true);
                AssertRuleWithoutInputsAndOutputs(rules, "Rule - Copy 1", ruleTrueOutput);
                AssertRuleWithoutInputsAndOutputs(rules, ruleFalseOutputName, ruleFalseOutput, true);
                AssertRuleWithoutInputsAndOutputs(rules, "Rule - Copy 2", ruleFalseOutput);

                IEnumerable<ConditionShape> conditionShapes = actualShapes.OfType<ConditionShape>();
                Assert.That(conditionShapes.Count(), Is.EqualTo(2));
                IEnumerable<ConditionBase> actualConditions = conditionShapes.Select(r => r.Tag).Cast<ConditionBase>();

                ConditionBase originalCondition = actualConditions.Single(s => string.Equals(s.Name, conditionBase.Name));
                Assert.That(originalCondition.Input, Is.SameAs(input));
                CollectionAssert.AreEqual(conditionBase.TrueOutputs, originalCondition.TrueOutputs);
                CollectionAssert.AreEqual(conditionBase.FalseOutputs, originalCondition.FalseOutputs);

                ConditionBase copiedCondition = actualConditions.Single(s => string.Equals(s.Name, "Condition - Copy 1"));
                Assert.That(copiedCondition.Input, Is.Not.SameAs(input));                           // There are only two inputs present, therefore the new rule should not match the original input
                Assert.That(copiedCondition.TrueOutputs.Single(), Is.Not.SameAs(ruleTrueOutput));   // Similar for the true outputs
                Assert.That(copiedCondition.FalseOutputs.Single(), Is.Not.SameAs(ruleFalseOutput)); // Similar for the false outputs
            }
        }

        [Test]
        public void GivenHelperWithConditionBaseConnectedWithMathematicalExpressions_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Note the ConditionBase only accepts:
            // - RuleBase
            // - MathematicalExpression
            // - ConditionBase
            // as valid objects for the TrueOutputs and FalseOutputs collection.
            // 
            // As such, the RTC objects Input and Output are not considered as valid items and are ignored for the tests.

            // Given
            IFeature inputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var input = new Input
            {
                Name = "Input",
                Feature = inputFeature
            };

            var expressionTrueOutput = new MathematicalExpression
            {
                Name = "TrueOutputExpression",
                Expression = "TruePotato"
            };
            var expressionFalseOutput = new MathematicalExpression
            {
                Name = "FalseOutputExpression",
                Expression = "FalsePotato"
            };

            const string conditionName = "Condition";
            var clonedCondition = Substitute.For<ConditionBase>();
            clonedCondition.Name = conditionName;

            var conditionBase = Substitute.For<ConditionBase>();
            conditionBase.Name = conditionName;
            conditionBase.Input = input;
            conditionBase.TrueOutputs.Add(expressionTrueOutput);
            conditionBase.FalseOutputs.Add(expressionFalseOutput);
            conditionBase.Clone().Returns(clonedCondition);

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);
            controlGroup.Conditions.Add(conditionBase);
            controlGroup.MathematicalExpressions.AddRange(new[] {expressionTrueOutput, expressionFalseOutput});

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(4));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(8));

                const int expectedNrOfInputs = 2;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                AssertInputs(inputs, input, expectedNrOfInputs);

                IEnumerable<MathematicalExpressionShape> mathematicalExpressionShapes = actualShapes.OfType<MathematicalExpressionShape>();
                Assert.That(mathematicalExpressionShapes.Count(), Is.EqualTo(4));
                IEnumerable<MathematicalExpression> expressions = mathematicalExpressionShapes.Select(r => r.Tag).Cast<MathematicalExpression>();
                AssertExpressionWithoutInput(expressions, expressionTrueOutput.Name, expressionTrueOutput, true);
                AssertExpressionWithoutInput(expressions, "Expression - Copy 1", expressionTrueOutput);
                AssertExpressionWithoutInput(expressions, expressionFalseOutput.Name, expressionFalseOutput, true);
                AssertExpressionWithoutInput(expressions, "Expression - Copy 2", expressionFalseOutput);

                IEnumerable<ConditionShape> conditionShapes = actualShapes.OfType<ConditionShape>();
                Assert.That(conditionShapes.Count(), Is.EqualTo(2));
                IEnumerable<ConditionBase> actualConditions = conditionShapes.Select(r => r.Tag).Cast<ConditionBase>();

                ConditionBase originalCondition = actualConditions.Single(s => string.Equals(s.Name, conditionBase.Name));
                Assert.That(originalCondition.Input, Is.SameAs(input));
                CollectionAssert.AreEqual(conditionBase.TrueOutputs, originalCondition.TrueOutputs);
                CollectionAssert.AreEqual(conditionBase.FalseOutputs, originalCondition.FalseOutputs);

                ConditionBase copiedCondition = actualConditions.Single(s => string.Equals(s.Name, "Condition - Copy 1"));
                Assert.That(copiedCondition.Input, Is.Not.SameAs(input)); // There are only two inputs present, therefore the new rule should not match the original input

                var copiedTrueExpression = (MathematicalExpression) copiedCondition.TrueOutputs.Single();
                Assert.That(copiedTrueExpression, Is.Not.SameAs(expressionTrueOutput)); // Similar for the true outputs
                Assert.That(copiedTrueExpression.Expression, Is.EqualTo(expressionTrueOutput.Expression));

                var copiedFalseExpression = (MathematicalExpression) copiedCondition.FalseOutputs.Single();
                Assert.That(copiedFalseExpression, Is.Not.SameAs(expressionFalseOutput)); // Similar for the false outputs
                Assert.That(copiedFalseExpression.Expression, Is.EqualTo(expressionFalseOutput.Expression));
            }
        }

        [Test]
        public void GivenHelperWithConditionBaseConnectedWithConditions_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Note the ConditionBase only accepts:
            // - RuleBase
            // - MathematicalExpression
            // - ConditionBase
            // as valid objects for the TrueOutputs and FalseOutputs collection.
            // 
            // As such, the RTC objects Input and Output are not considered as valid items and are ignored for the tests.

            // Given
            IFeature inputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var input = new Input
            {
                Name = "Input",
                Feature = inputFeature
            };

            const string conditionTrueOutputName = "ConditionTrueOutput";
            var clonedTrueOutputCondition = Substitute.For<ConditionBase>();
            clonedTrueOutputCondition.Name = conditionTrueOutputName;

            var conditionTrueOutput = Substitute.For<ConditionBase>();
            conditionTrueOutput.Name = conditionTrueOutputName;
            conditionTrueOutput.Clone().Returns(clonedTrueOutputCondition);

            const string conditionFalseOutputName = "ConditionFalseOutput";
            var clonedFalseOutputCondition = Substitute.For<ConditionBase>();
            clonedFalseOutputCondition.Name = conditionFalseOutputName;

            var conditionFalseOutput = Substitute.For<ConditionBase>();
            conditionFalseOutput.Name = conditionFalseOutputName;
            conditionFalseOutput.Clone().Returns(clonedFalseOutputCondition);
            
            const string conditionName = "Condition";
            var clonedCondition = Substitute.For<ConditionBase>();
            clonedCondition.Name = conditionName;

            var conditionBase = Substitute.For<ConditionBase>();
            conditionBase.Name = conditionName;
            conditionBase.Input = input;
            conditionBase.TrueOutputs.Add(conditionTrueOutput);
            conditionBase.FalseOutputs.Add(conditionFalseOutput);
            conditionBase.Clone().Returns(clonedCondition);

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);
            controlGroup.Conditions.AddRange(new[] {conditionBase, conditionTrueOutput, conditionFalseOutput});

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(4));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(8));

                const int expectedNrOfInputs = 2;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                AssertInputs(inputs, input, expectedNrOfInputs);

                IEnumerable<ConditionShape> conditionShapes = actualShapes.OfType<ConditionShape>();
                Assert.That(conditionShapes.Count(), Is.EqualTo(6));
                IEnumerable<ConditionBase> conditions = conditionShapes.Select(r => r.Tag).Cast<ConditionBase>();
                AssertConditionsWithoutInputAndOutputs(conditions, conditionTrueOutput.Name, conditionTrueOutput, true);
                AssertConditionsWithoutInputAndOutputs(conditions, "Condition - Copy 2", conditionTrueOutput);
                AssertConditionsWithoutInputAndOutputs(conditions, conditionFalseOutput.Name, conditionFalseOutput, true);
                AssertConditionsWithoutInputAndOutputs(conditions, "Condition - Copy 3", conditionFalseOutput);

                ConditionBase originalCondition = conditions.Single(s => string.Equals(s.Name, conditionBase.Name));
                Assert.That(originalCondition.Input, Is.SameAs(input));
                CollectionAssert.AreEqual(conditionBase.TrueOutputs, originalCondition.TrueOutputs);
                CollectionAssert.AreEqual(conditionBase.FalseOutputs, originalCondition.FalseOutputs);

                ConditionBase copiedCondition = conditions.Single(s => string.Equals(s.Name, "Condition - Copy 1"));
                Assert.That(copiedCondition.Input, Is.Not.SameAs(input));                                // There are only two inputs present, therefore the new rule should not match the original input
                Assert.That(copiedCondition.TrueOutputs.Single(), Is.Not.SameAs(conditionTrueOutput));   // Similar for the true outputs
                Assert.That(copiedCondition.FalseOutputs.Single(), Is.Not.SameAs(conditionFalseOutput)); // Similar for the false outputs
            }
        }

        private static void AssertRuleWithoutInputsAndOutputs(IEnumerable<RuleBase> actualRules,
                                                              string ruleName,
                                                              RuleBase referenceRule,
                                                              bool isSameAsReferenceRule = false)
        {
            RuleBase rule = actualRules.Single(r => string.Equals(r.Name, ruleName));
            Assert.That(rule.Inputs, Is.Empty);
            Assert.That(rule.Outputs, Is.Empty);
            Assert.That(rule, isSameAsReferenceRule
                                  ? Is.SameAs(referenceRule)
                                  : Is.Not.SameAs(referenceRule));
        }

        private static void AssertExpressionWithoutInput(IEnumerable<MathematicalExpression> actualExpressions,
                                                         string expressionName,
                                                         MathematicalExpression referenceExpression,
                                                         bool isSameAsReferenceExpression = false)
        {
            MathematicalExpression expression = actualExpressions.Single(r => string.Equals(r.Name, expressionName));
            Assert.That(expression.Inputs, Is.Empty);
            Assert.That(expression.Expression, Is.EqualTo(referenceExpression.Expression));
            Assert.That(expression, isSameAsReferenceExpression
                                        ? Is.SameAs(referenceExpression)
                                        : Is.Not.SameAs(referenceExpression));
        }

        private static void AssertConditionsWithoutInputAndOutputs(IEnumerable<ConditionBase> actualConditions,
                                                                   string conditionName,
                                                                   ConditionBase referenceCondition,
                                                                   bool isSameAsReferenceCondition = false)
        {
            ConditionBase signal = actualConditions.Single(r => string.Equals(r.Name, conditionName));
            Assert.That(signal.Input, Is.Null);
            Assert.That(signal.TrueOutputs, Is.Empty);
            Assert.That(signal.FalseOutputs, Is.Empty);
            Assert.That(signal, isSameAsReferenceCondition
                                    ? Is.SameAs(referenceCondition)
                                    : Is.Not.SameAs(referenceCondition));
        }

        private static void AssertInputs(IEnumerable<Input> actualInputs,
                                         Input originalInput,
                                         int expectedNrOfInputs)
        {
            Assert.That(actualInputs.Distinct().Count(), Is.EqualTo(expectedNrOfInputs)); // Perform a distinct check to ensure all references are different
            Assert.That(actualInputs.All(i => ReferenceEquals(i.Feature, originalInput.Feature)), Is.True);
            Assert.That(actualInputs.All(i => string.Equals(i.Name, originalInput.Name)), Is.True);
        }

        private class TestShape : ShapeBase
        {
            protected override void Initialize() {}
        }

        /// <summary>
        /// Helper class to assist with the copy paste actions of the Real Time Control Model.
        /// </summary>
        public class RealTimeControlModelCopyPasteHelperShadow
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(RealTimeControlModelCopyPasteHelperShadow));

            private static RealTimeControlModelCopyPasteHelperShadow instance;
            private readonly List<ShapeBase> copiedShapes;

            private RealTimeControlModelCopyPasteHelperShadow()
            {
                copiedShapes = new List<ShapeBase>();
                IsDataSet = false;
            }

            /// <summary>
            /// Gets the instance of <see cref="RealTimeControlModelCopyPasteHelperShadow"/>.
            /// </summary>
            public static RealTimeControlModelCopyPasteHelperShadow Instance
            {
                get
                {
                    return instance ?? (instance = new RealTimeControlModelCopyPasteHelperShadow());
                }
            }

            /// <summary>
            /// Gets the collection of copied shapes.
            /// </summary>
            public IEnumerable<ShapeBase> CopiedShapes => copiedShapes;

            /// <summary>
            /// Gets the indicator whether the data is set for copying.
            /// </summary>
            public bool IsDataSet { get; private set; }

            /// <summary>
            /// Sets the copied data to the helper.
            /// </summary>
            /// <param name="shapes">The collection of <see cref="ShapeBase"/> to set.</param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="shapes"/>
            /// is <c>null</c>.
            /// </exception>
            public void SetCopiedData(IEnumerable<ShapeBase> shapes)
            {
                if (shapes == null)
                {
                    throw new ArgumentNullException(nameof(shapes));
                }

                IsDataSet = shapes.Any();
                copiedShapes.AddRange(shapes);
            }

            /// <summary>
            /// Clears the data that is set.
            /// </summary>
            public void ClearData()
            {
                IsDataSet = false;
                copiedShapes.Clear();
            }

            /// <summary>
            /// Copies the shapes to the <see cref="ControlGroupEditorController"/>.
            /// </summary>
            /// <param name="controller">
            /// The <see cref="ControlGroupEditorController"/> to copy the shapes to.
            /// </param>
            /// <param name="mea">The location to place the copied shapes at.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is <c>null</c>.</exception>
            public void CopyShapesToController(ControlGroupEditorController controller, Point mea)
            {
                if (controller == null)
                {
                    throw new ArgumentNullException(nameof(controller));
                }

                if (!IsDataSet)
                {
                    return;
                }

                Dictionary<Input, Input> inputMapping = CopyConnectionPointData<Input>();
                Dictionary<Output, Output> outputMapping = CopyConnectionPointData<Output>();

                Dictionary<RuleBase, RuleBase> ruleMapping = CopyRules(inputMapping, outputMapping);
                List<SignalBase> copiedSignals = CopySignals(inputMapping, ruleMapping);
                Dictionary<MathematicalExpression, MathematicalExpression> mathematicalExpressionMapping = CopyMathematicalExpressions(inputMapping);
                List<ConditionBase> copiedConditions = CopyConditions(inputMapping, ruleMapping, mathematicalExpressionMapping);

                ControlGroup controlGroup = controller.ControlGroup;
                List<RuleBase> copiedRules = ruleMapping.Values.ToList();
                List<MathematicalExpression> copiedExpressions = mathematicalExpressionMapping.Values.ToList();
                List<Output> copiedOutputs = outputMapping.Values.ToList();
                List<Input> copiedInputs = inputMapping.Values.ToList();
                PostProcessCopiedData(controlGroup, copiedRules, copiedSignals, copiedExpressions, copiedConditions, copiedOutputs);
                AddDataToController(controller, mea, copiedRules, copiedConditions, copiedInputs, copiedOutputs, copiedSignals, copiedExpressions);
            }

            private static void PostProcessCopiedData(ControlGroup controlGroup, 
                                                      List<RuleBase> copiedRules,
                                                      List<SignalBase> copiedSignals,
                                                      List<MathematicalExpression> copiedExpressions, 
                                                      List<ConditionBase> copiedConditions, 
                                                      List<Output> copiedOutputs)
            {
                RenameCopiedDataWithUniqueNames(copiedRules, controlGroup.Rules, "Rule");
                RenameCopiedDataWithUniqueNames(copiedSignals, controlGroup.Signals, "Signal");
                RenameCopiedDataWithUniqueNames(copiedExpressions, controlGroup.MathematicalExpressions, "Expression");
                RenameCopiedDataWithUniqueNames(copiedConditions, controlGroup.Conditions, "Condition");
                ResetOutputs(copiedOutputs);
            }

            private static void AddDataToController(ControlGroupEditorController controller, 
                                                    Point mea, 
                                                    List<RuleBase> copiedRules,
                                                    List<ConditionBase> copiedConditions, 
                                                    List<Input> inputs, 
                                                    List<Output> copiedOutputs,
                                                    List<SignalBase> copiedSignals,
                                                    List<MathematicalExpression> copiedExpressions)
            {
                controller.AddShapesToControlGroupAndPlace(copiedRules,
                                                           copiedConditions,
                                                           inputs,
                                                           copiedOutputs,
                                                           copiedSignals,
                                                           copiedExpressions,
                                                           mea);

                controller.AddConnections(copiedRules, copiedConditions, copiedSignals, copiedExpressions, true);
            }

            private Dictionary<T, T> CopyConnectionPointData<T>() where T : ConnectionPoint
            {
                IEnumerable<T> inputs = copiedShapes.Select(s => s.Tag).OfType<T>();
                var mapping = new Dictionary<T, T>();
                foreach (T input in inputs)
                {
                    mapping[input] = (T) input.Clone();
                }

                return mapping;
            }

            private Dictionary<RuleBase, RuleBase> CopyRules(IReadOnlyDictionary<Input, Input> inputMapping,
                                                             IReadOnlyDictionary<Output, Output> outputMapping)
            {
                IEnumerable<RuleBase> rules = copiedShapes.Select(s => s.Tag).OfType<RuleBase>();

                var mapping = new Dictionary<RuleBase, RuleBase>();
                foreach (RuleBase rule in rules)
                {
                    var copiedRule = (RuleBase) rule.Clone();
                    SetInputs(copiedRule.Inputs, rule.Inputs, inputMapping);
                    SetOutputs(copiedRule, rule, outputMapping);
                    mapping[rule] = copiedRule;
                }

                return mapping;
            }

            private List<SignalBase> CopySignals(IReadOnlyDictionary<Input, Input> inputMapping,
                                                 IReadOnlyDictionary<RuleBase, RuleBase> ruleMapping)
            {
                IEnumerable<SignalBase> signals = copiedShapes.Select(s => s.Tag).OfType<SignalBase>();

                var copiedSignals = new List<SignalBase>();
                foreach (SignalBase signal in signals)
                {
                    var copiedSignal = (SignalBase) signal.Clone();
                    SetInputs(copiedSignal.Inputs, signal.Inputs, inputMapping);
                    SetRules(copiedSignal.RuleBases, signal.RuleBases, ruleMapping);
                    copiedSignals.Add(copiedSignal);
                }

                return copiedSignals;
            }

            private Dictionary<MathematicalExpression, MathematicalExpression> CopyMathematicalExpressions(IReadOnlyDictionary<Input, Input> inputMapping)
            {
                IEnumerable<MathematicalExpression> mathematicalExpressions = copiedShapes.Select(s => s.Tag).OfType<MathematicalExpression>();

                var expressionMapping = new Dictionary<MathematicalExpression, MathematicalExpression>();
                foreach (MathematicalExpression mathematicalExpression in mathematicalExpressions)
                {
                    var copiedMathematicalExpression = (MathematicalExpression) mathematicalExpression.Clone();
                    SetInputs(copiedMathematicalExpression.Inputs, mathematicalExpression.Inputs, inputMapping);
                    expressionMapping[mathematicalExpression] = copiedMathematicalExpression;
                }

                return expressionMapping;
            }

            private List<ConditionBase> CopyConditions(IReadOnlyDictionary<Input, Input> inputMapping,
                                                       IReadOnlyDictionary<RuleBase, RuleBase> ruleMapping,
                                                       IReadOnlyDictionary<MathematicalExpression, MathematicalExpression> expressionMapping)
            {
                IEnumerable<ConditionBase> conditions = copiedShapes.Select(s => s.Tag).OfType<ConditionBase>();

                var copiedConditions = new List<ConditionBase>();
                var conditionMapping = new Dictionary<RtcBaseObject, RtcBaseObject>();
                foreach (ConditionBase condition in conditions)
                {
                    var copiedCondition = (ConditionBase) condition.Clone();

                    copiedCondition.Input = condition.Input == null ? null : inputMapping[(Input) condition.Input];
                    conditionMapping[condition] = copiedCondition;
                    copiedConditions.Add(copiedCondition);
                }

                // Gather all the objects that can be connected to a condition in the true and false outputs
                Dictionary<RtcBaseObject, RtcBaseObject> rtcObjectMapping = 
                    conditionMapping.Concat(ruleMapping.ToDictionary(kvp => (RtcBaseObject) kvp.Key,kvp => (RtcBaseObject) kvp.Value))
                                    .Concat(expressionMapping.ToDictionary(kvp => (RtcBaseObject) kvp.Key, kvp => (RtcBaseObject) kvp.Value))
                                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Then set all the values
                foreach (ConditionBase originalCondition in conditions)
                {
                    var copiedCondition = (ConditionBase) conditionMapping[originalCondition];

                    SetOutputs(copiedCondition.TrueOutputs, originalCondition.TrueOutputs, rtcObjectMapping);
                    SetOutputs(copiedCondition.FalseOutputs, originalCondition.FalseOutputs, rtcObjectMapping);
                }

                return copiedConditions;
            }

            private static void SetOutputs(IEventedList<RtcBaseObject> targetOutputs, IEnumerable<RtcBaseObject> sourceOutput, IReadOnlyDictionary<RtcBaseObject, RtcBaseObject> outputMapping)
            {
                var objectsToBeAdded = new List<RtcBaseObject>();
                foreach (RtcBaseObject rtcBaseObject in sourceOutput)
                {
                    objectsToBeAdded.Add(outputMapping[rtcBaseObject]);
                }

                targetOutputs.AddRange(objectsToBeAdded);
            }

            private static void ResetOutputs(IEnumerable<Output> outputs)
            {
                foreach (Output output in outputs)
                {
                    log.InfoFormat("It is not possible to copy and paste internal data for control group outputs, the connection to {0} will be reset.", output.Name);
                    output.Reset();
                }
            }

            private static void SetInputs(IEventedList<IInput> targetInputs, IEnumerable<IInput> sourceInputs, IReadOnlyDictionary<Input, Input> inputMapping)
            {
                var inputsToAdd = new List<Input>();
                foreach (IInput sourceInput in sourceInputs)
                {
                    var castInput = (Input) sourceInput;
                    inputsToAdd.Add(inputMapping[castInput]);
                }

                targetInputs.AddRange(inputsToAdd);
            }

            private static void SetInputs(IEventedList<Input> targetInputs, IEnumerable<IInput> sourceInputs, IReadOnlyDictionary<Input, Input> inputMapping)
            {
                var inputsToAdd = new List<Input>();
                foreach (IInput sourceInput in sourceInputs)
                {
                    var castInput = (Input) sourceInput;
                    inputsToAdd.Add(inputMapping[castInput]);
                }

                targetInputs.AddRange(inputsToAdd);
            }

            private static void SetRules(IEventedList<RuleBase> targetRules, IEnumerable<RuleBase> sourceRules, IReadOnlyDictionary<RuleBase, RuleBase> ruleMapping)
            {
                var rulesToAdd = new List<RuleBase>();
                foreach (RuleBase sourceInput in sourceRules)
                {
                    rulesToAdd.Add(ruleMapping[sourceInput]);
                }

                targetRules.AddRange(rulesToAdd);
            }

            private static void SetOutputs(RuleBase target, RuleBase source, IReadOnlyDictionary<Output, Output> outputMapping)
            {
                Output[] outputsToAdd = source.Outputs.Select(sourceOutput => outputMapping[sourceOutput]).ToArray();
                target.Outputs.AddRange(outputsToAdd);
            }

            private static void RenameCopiedDataWithUniqueNames<T>(IEnumerable<T> copiedData, IEnumerable<T> controllerData, string objName)
                where T : RtcBaseObject
            {
                if (!controllerData.Any())
                {
                    return;
                }

                List<T> currentObjects = controllerData.ToList();
                var existingNames = new HashSet<string>(currentObjects.Select(d => d.Name));
                foreach (T copy in copiedData)
                {
                    if (existingNames.Contains(copy.Name))
                    {
                        string uniqueName = RealTimeControlModelHelper.GetUniqueName(objName + " - Copy {0}",
                                                                                     currentObjects, "Copy");
                        copy.Name = uniqueName;
                        existingNames.Add(uniqueName);
                        currentObjects.Add(copy);
                    }
                }
            }
        }
    }
}
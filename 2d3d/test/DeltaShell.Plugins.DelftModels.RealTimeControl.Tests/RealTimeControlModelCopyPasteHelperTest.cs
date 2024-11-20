using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Graph;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlModelCopyPasteHelperTest
    {
        [SetUp]
        public void Setup()
        {
            // As the helper is a singleton, reset its state before every test begins
            var helper = RealTimeControlModelCopyPasteHelper.Instance;
            helper.ClearData();

            // Precondition
            Assert.That(helper.CopiedShapes, Is.Empty);
            Assert.That(helper.IsDataSet, Is.False);
        }

        [Test]
        public void Instance_Always_ReturnsSameInstance()
        {
            // Call
            var firstInstance = RealTimeControlModelCopyPasteHelper.Instance;
            var secondInstance = RealTimeControlModelCopyPasteHelper.Instance;

            // Assert
            Assert.That(firstInstance, Is.SameAs(secondInstance));
        }

        [Test]
        public void Instance_ExpectedProperties()
        {
            // Call
            var instance = RealTimeControlModelCopyPasteHelper.Instance;

            // Assert
            Assert.That(instance.CopiedShapes, Is.Empty);
            Assert.That(instance.IsDataSet, Is.False);
        }

        [Test]
        public void SetCopiedData_ShapesNull_ThrowsArgumentNullException()
        {
            // Setup
            var helper = RealTimeControlModelCopyPasteHelper.Instance;

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
            var helper = RealTimeControlModelCopyPasteHelper.Instance;

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
            var shapes = new[]
            {
                new TestShape(),
                new TestShape(),
                new TestShape()
            };
            var helper = RealTimeControlModelCopyPasteHelper.Instance;

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
            var shapes = new[]
            {
                new TestShape(),
                new TestShape(),
                new TestShape()
            };
            var helper = RealTimeControlModelCopyPasteHelper.Instance;

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
            var helper = RealTimeControlModelCopyPasteHelper.Instance;

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
                var helper = RealTimeControlModelCopyPasteHelper.Instance;

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

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(1));

                // When
                Action call = () => helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                var expectedMessage = $"It is not possible to copy and paste internal data for control group outputs, the connection to {output.Name} will be reset.";
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
        public void GivenHelperWithRuleBasedDataAndInputAsInput_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
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
            rule.Inputs = new EventedList<IInput>(new[]
            {
                input
            });
            rule.Outputs = new EventedList<Output>(new[]
            {
                output
            });
            rule.Clone().Returns(clonedRule);

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(rule);
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
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
        public void GivenHelperWithRuleBasedDataAndMathematicalExpressionAsInput_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Given
            var input = new MathematicalExpression
            {
                Name = "ExpressionName",
                Expression = "Expression"
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
            rule.Inputs = new EventedList<IInput>(new[]
            {
                input
            });
            rule.Outputs = new EventedList<Output>(new[]
            {
                output
            });
            rule.Clone().Returns(clonedRule);

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(rule);
            controlGroup.MathematicalExpressions.Add(input);
            controlGroup.Outputs.Add(output);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(3));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(6));

                const int expectedNrOfInputs = 2;
                IEnumerable<MathematicalExpressionShape> expressionShapes = actualShapes.OfType<MathematicalExpressionShape>();
                Assert.That(expressionShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<MathematicalExpression> expressions = expressionShapes.Select(i => i.Tag).Cast<MathematicalExpression>();
                AssertExpressionWithoutInput(expressions, input.Name, input, true);
                AssertExpressionWithoutInput(expressions, "Expression - Copy 1", input);

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
        public void GivenHelperWithFullyConfiguredRuleBasedData_WhenCopyShapesToControllerWithRuleOnly_ThenOnlyEmptyRuleIsCopied()
        {
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
            rule.Inputs = new EventedList<IInput>(new[]
            {
                input
            });
            rule.Outputs = new EventedList<Output>(new[]
            {
                output
            });
            rule.Clone().Returns(clonedRule);

            var controlGroup = new ControlGroup();
            controlGroup.Rules.Add(rule);
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<RuleShape>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(1));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(4));

                const int expectedNrOfInputs = 1;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                Assert.That(inputs.Single(), Is.SameAs(input));

                IEnumerable<OutputItemShape> outputShapes = actualShapes.OfType<OutputItemShape>();
                Assert.That(outputShapes.Count(), Is.EqualTo(1));
                IEnumerable<Output> actualOutputs = outputShapes.Select(s => s.Tag).Cast<Output>();

                Output originalOutput = actualOutputs.Single();
                Assert.That(originalOutput, Is.SameAs(output));
                Assert.That(originalOutput.Feature, Is.SameAs(outputFeature));

                IEnumerable<RuleShape> ruleShapes = actualShapes.OfType<RuleShape>();
                Assert.That(ruleShapes.Count(), Is.EqualTo(2));

                IEnumerable<RuleBase> rules = ruleShapes.Select(r => r.Tag).Cast<RuleBase>();
                RuleBase originalRule = rules.Single(r => string.Equals(r.Name, rule.Name));
                Assert.That(originalRule, Is.SameAs(rule));
                CollectionAssert.AreEqual(rule.Inputs, originalRule.Inputs);
                CollectionAssert.AreEqual(rule.Outputs, originalRule.Outputs);

                RuleBase copiedRule = rules.Single(r => string.Equals(r.Name, "Rule - Copy 1"));
                Assert.That(copiedRule, Is.Not.SameAs(rule));
                Assert.That(copiedRule.Inputs, Is.Empty);
                Assert.That(copiedRule.Outputs, Is.Empty);
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
            signal.Inputs = new EventedList<Input>(new[]
            {
                input
            });
            signal.RuleBases = new EventedList<RuleBase>(new[]
            {
                rule
            });
            signal.Clone().Returns(clonedSignal);

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);
            controlGroup.Signals.Add(signal);
            controlGroup.Rules.Add(rule);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
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
        public void GivenHelperWithFullyConfiguredSignalBasedData_WhenCopyShapesToControllerWithSignalBasedDataOnly_ThenOnlyEmptySignalBasedDataIsCopied()
        {
            // Given
            IFeature inputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var input = new Input
            {
                Name = "Input",
                Feature = inputFeature
            };

            var rule = Substitute.For<RuleBase>();

            const string signalName = "Signal";
            var clonedSignal = Substitute.For<SignalBase>();
            clonedSignal.Name = signalName;

            var signal = Substitute.For<SignalBase>();
            signal.Name = signalName;
            signal.Inputs = new EventedList<Input>(new[]
            {
                input
            });
            signal.RuleBases = new EventedList<RuleBase>(new[]
            {
                rule
            });
            signal.Clone().Returns(clonedSignal);

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);
            controlGroup.Signals.Add(signal);
            controlGroup.Rules.Add(rule);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<SignalShape>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(1));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(4));

                rule.DidNotReceiveWithAnyArgs().Clone();

                const int expectedNrOfInputs = 1;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                Assert.That(inputs.Single(), Is.SameAs(input));

                IEnumerable<RuleShape> ruleShapes = actualShapes.OfType<RuleShape>();
                Assert.That(ruleShapes.Count(), Is.EqualTo(1));
                IEnumerable<RuleBase> rules = ruleShapes.Select(r => r.Tag).Cast<RuleBase>();
                Assert.That(rules.Single(), Is.SameAs(rule));

                IEnumerable<SignalShape> signalShapes = actualShapes.OfType<SignalShape>();
                Assert.That(signalShapes.Count(), Is.EqualTo(2));
                IEnumerable<SignalBase> actualSignals = signalShapes.Select(r => r.Tag).Cast<SignalBase>();

                SignalBase originalSignal = actualSignals.Single(s => string.Equals(s.Name, signal.Name));
                Assert.That(originalSignal, Is.SameAs(signal));
                CollectionAssert.AreEqual(signal.Inputs, originalSignal.Inputs);
                CollectionAssert.AreEqual(signal.RuleBases, originalSignal.RuleBases);

                SignalBase copiedSignal = actualSignals.Single(s => string.Equals(s.Name, "Signal - Copy 1"));
                Assert.That(copiedSignal, Is.Not.SameAs(signal));
                Assert.That(copiedSignal.Inputs, Is.Empty);
                Assert.That(copiedSignal.RuleBases, Is.Empty);
            }
        }

        [Test]
        public void GivenHelperWithMathematicalExpressionDataAndInputAsInput_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
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

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
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
        public void GivenHelperWithMathematicalExpressionDataAndMathematicalExpressionAsInput_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Given
            var inputExpression = new MathematicalExpression
            {
                Name = "InputExpression",
                Expression = "InputPotato"
            };

            var expression = new MathematicalExpression
            {
                Name = "Expression",
                Expression = "Potato"
            };
            expression.Inputs.Add(inputExpression);

            var controlGroup = new ControlGroup();
            controlGroup.MathematicalExpressions.AddRange(new[]
            {
                inputExpression,
                expression
            });

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(2));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(4));

                IEnumerable<MathematicalExpressionShape> mathematicalExpressionShapes = actualShapes.OfType<MathematicalExpressionShape>();
                Assert.That(mathematicalExpressionShapes.Count(), Is.EqualTo(4));
                IEnumerable<MathematicalExpression> actualMathematicalExpressions = mathematicalExpressionShapes.Select(s => s.Tag).Cast<MathematicalExpression>();
                AssertExpressionWithoutInput(actualMathematicalExpressions, inputExpression.Name, inputExpression, true); // These expressions represent the inputs
                AssertExpressionWithoutInput(actualMathematicalExpressions, "Expression - Copy 1", inputExpression);

                MathematicalExpression originalExpression = actualMathematicalExpressions.Single(o => string.Equals(o.Name, expression.Name));
                Assert.That(originalExpression, Is.SameAs(expression));
                CollectionAssert.AreEqual(expression.Inputs, originalExpression.Inputs);

                MathematicalExpression copiedExpression = actualMathematicalExpressions.Single(s => string.Equals(s.Name, "Expression - Copy 2"));
                Assert.That(copiedExpression, Is.Not.SameAs(expression));
                Assert.That(copiedExpression.Inputs.Single(), Is.Not.SameAs(inputExpression)); // There are only two inputs present, therefore the new rule should not match the original input
            }
        }

        [Test]
        public void GivenHelperWithFullyConfiguredMathematicalExpressionData_WhenCopyShapesToControllerWithMathematicalExpressionOnly_ThenOnlyEmptyMathematicalExpressionCopied()
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
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<MathematicalExpressionShape>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(1));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(3));

                const int expectedNrOfInputs = 1;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                Assert.That(inputs.Single(), Is.SameAs(input));

                IEnumerable<MathematicalExpressionShape> mathematicalExpressionShapes = actualShapes.OfType<MathematicalExpressionShape>();
                Assert.That(mathematicalExpressionShapes.Count(), Is.EqualTo(2));

                IEnumerable<MathematicalExpression> actualMathematicalExpressions = mathematicalExpressionShapes.Select(s => s.Tag).Cast<MathematicalExpression>();
                Assert.That(actualMathematicalExpressions.All(e => string.Equals(e.Expression, expression.Expression)), Is.True);

                MathematicalExpression originalExpression = actualMathematicalExpressions.Single(o => string.Equals(o.Name, expression.Name));
                Assert.That(originalExpression, Is.SameAs(expression));
                CollectionAssert.AreEqual(expression.Inputs, originalExpression.Inputs);

                MathematicalExpression copiedExpression = actualMathematicalExpressions.Single(s => string.Equals(s.Name, "Expression - Copy 1"));
                Assert.That(copiedExpression, Is.Not.SameAs(expression));
                Assert.That(copiedExpression.Inputs, Is.Empty);
            }
        }

        [Test]
        public void GivenHelperWithConditionBaseNothingConnected_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
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

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
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
        public void GivenHelperWithConditionBaseAndMathematicalExpressionAsInput_WhenCopyShapesToController_ThenShapesAndConnectionsCopied()
        {
            // Given
            var input = new MathematicalExpression
            {
                Name = "Input",
                Expression = "ExpressionForTheInput"
            };

            const string conditionName = "Condition";
            var clonedCondition = Substitute.For<ConditionBase>();
            clonedCondition.Name = conditionName;

            var conditionBase = Substitute.For<ConditionBase>();
            conditionBase.Name = conditionName;
            conditionBase.Input = input;
            conditionBase.Clone().Returns(clonedCondition);

            var controlGroup = new ControlGroup();
            controlGroup.Conditions.Add(conditionBase);
            controlGroup.MathematicalExpressions.Add(input);

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(2));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(4));

                const int expectedNrOfInputs = 2;
                IEnumerable<MathematicalExpressionShape> expressionShapes = actualShapes.OfType<MathematicalExpressionShape>();
                Assert.That(expressionShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<MathematicalExpression> expressions = expressionShapes.Select(i => i.Tag).Cast<MathematicalExpression>();
                AssertExpressionWithoutInput(expressions, input.Name, input, true);
                AssertExpressionWithoutInput(expressions, "Expression - Copy 1", input);

                IEnumerable<ConditionShape> conditionShapes = actualShapes.OfType<ConditionShape>();
                Assert.That(conditionShapes.Count(), Is.EqualTo(2));

                IEnumerable<ConditionBase> actualConditions = conditionShapes.Select(r => r.Tag).Cast<ConditionBase>();
                Assert.That(actualConditions.SelectMany(c => c.TrueOutputs), Is.Empty); // No outputs are present and should remain empty
                Assert.That(actualConditions.SelectMany(c => c.FalseOutputs), Is.Empty);

                ConditionBase originalCondition = actualConditions.Single(c => string.Equals(c.Name, conditionBase.Name));
                Assert.That(originalCondition, Is.SameAs(conditionBase));
                Assert.That(originalCondition.Input, Is.SameAs(input));

                ConditionBase copiedCondition = actualConditions.Single(c => string.Equals(c.Name, "Condition - Copy 1"));
                Assert.That(copiedCondition, Is.Not.SameAs(conditionBase));
                Assert.That(copiedCondition.Input, Is.Not.SameAs(input));
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
            controlGroup.Rules.AddRange(new[]
            {
                ruleTrueOutput,
                ruleFalseOutput
            });

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
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
            controlGroup.MathematicalExpressions.AddRange(new[]
            {
                expressionTrueOutput,
                expressionFalseOutput
            });

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
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
            controlGroup.Conditions.AddRange(new[]
            {
                conditionBase,
                conditionTrueOutput,
                conditionFalseOutput
            });

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
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

        [Test]
        public void GivenHelperWithFullyConfiguredConditionBased_WhenCopyShapesToControllerWithConditionBaseDataOnly_ThenOnlyEmptyConditionBaseCopied()
        {
            // Given
            IFeature inputFeature = Substitute.For<IFeature, INotifyPropertyChanged>();
            var input = new Input
            {
                Name = "Input",
                Feature = inputFeature
            };

            var ruleTrueOutput = Substitute.For<RuleBase>();
            var ruleFalseOutput = Substitute.For<RuleBase>();

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
            controlGroup.Rules.AddRange(new[]
            {
                ruleTrueOutput,
                ruleFalseOutput
            });

            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IEnumerable<ShapeBase> shapes = graphControl.GetShapes<ConditionShape>();

                var helper = RealTimeControlModelCopyPasteHelper.Instance;
                helper.SetCopiedData(shapes);

                // Precondition
                Assert.That(helper.CopiedShapes, Has.Count.EqualTo(1));

                // When
                helper.CopyShapesToController(controlGroupEditor.Controller, Point.Empty);

                // Then
                IEnumerable<ShapeBase> actualShapes = graphControl.GetShapes<ShapeBase>();
                Assert.That(actualShapes.Count(), Is.EqualTo(5));

                ruleTrueOutput.DidNotReceiveWithAnyArgs().Clone();
                ruleFalseOutput.DidNotReceiveWithAnyArgs().Clone();

                const int expectedNrOfInputs = 1;
                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(expectedNrOfInputs));
                IEnumerable<Input> inputs = inputShapes.Select(i => i.Tag).Cast<Input>();
                Assert.That(inputs.Single(), Is.SameAs(input));

                IEnumerable<RuleShape> ruleShapes = actualShapes.OfType<RuleShape>();
                Assert.That(ruleShapes.Count(), Is.EqualTo(2));
                IEnumerable<RuleBase> rules = ruleShapes.Select(r => r.Tag).Cast<RuleBase>();
                CollectionAssert.AreEquivalent(new[]
                {
                    ruleTrueOutput,
                    ruleFalseOutput
                }, rules);

                IEnumerable<ConditionShape> conditionShapes = actualShapes.OfType<ConditionShape>();
                Assert.That(conditionShapes.Count(), Is.EqualTo(2));
                IEnumerable<ConditionBase> actualConditions = conditionShapes.Select(r => r.Tag).Cast<ConditionBase>();

                ConditionBase originalCondition = actualConditions.Single(s => string.Equals(s.Name, conditionBase.Name));
                Assert.That(originalCondition.Input, Is.SameAs(input));
                CollectionAssert.AreEqual(conditionBase.TrueOutputs, originalCondition.TrueOutputs);
                CollectionAssert.AreEqual(conditionBase.FalseOutputs, originalCondition.FalseOutputs);

                ConditionBase copiedCondition = actualConditions.Single(s => string.Equals(s.Name, "Condition - Copy 1"));
                Assert.That(copiedCondition.Input, Is.Not.SameAs(input));
                Assert.That(copiedCondition.TrueOutputs, Is.Empty);
                Assert.That(copiedCondition.FalseOutputs, Is.Empty);
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
    }
}
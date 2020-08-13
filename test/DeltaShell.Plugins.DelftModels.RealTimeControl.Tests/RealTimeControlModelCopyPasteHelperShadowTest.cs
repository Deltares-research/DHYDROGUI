using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Graph;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
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
        [Ignore]
        public void GivenHelperWithoutData_WhenCopyShapesToController_ThenNothingHappens()
        {
            // Given
            var controller = Substitute.For<IControlGroupEditorController>();
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Precondition
            Assert.That(helper.CopiedShapes, Is.Empty);

            // When
            helper.CopyShapesToController(null, Point.Empty);

            // Then
            controller.DidNotReceiveWithAnyArgs().AddConnections(null, null, null, null);
            controller.DidNotReceiveWithAnyArgs().AddShapesToControlGroupAndPlace(null, null, null, null, null, null, Point.Empty);
        }

        [Test]
        public void GivenHelperWithRuleBasedData_WhenCopyShapesToController_ThenShapesAndConnectionsCopiedWithUniqueNames()
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

            var clonedRule = Substitute.For<RuleBase>();
            clonedRule.Name = "Rule";
            clonedRule.Inputs = new EventedList<IInput>();
            clonedRule.Outputs = new EventedList<Output>();

            var rule = Substitute.For<RuleBase>();
            rule.Name = "Rule";
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

                IEnumerable<InputItemShape> inputShapes = actualShapes.OfType<InputItemShape>();
                Assert.That(inputShapes.Count(), Is.EqualTo(2));

                IEnumerable<Input> actualInputs = inputShapes.Select(s => s.Tag).Cast<Input>();
                Assert.That(actualInputs.All(i => ReferenceEquals(i.Feature, inputFeature)), Is.True);
                Assert.That(actualInputs.All(i => string.Equals(i.Name, input.Name)), Is.True);

                IEnumerable<OutputItemShape> outputShapes = actualShapes.OfType<OutputItemShape>();
                Assert.That(outputShapes.Count(), Is.EqualTo(2));
                IEnumerable<Output> actualOutputs = outputShapes.Select(s => s.Tag).Cast<Output>();

                Output originalOutput = actualOutputs.Single(o => string.Equals(o.Name, output.Name));
                Assert.That(originalOutput.Feature, Is.SameAs(outputFeature));

                Output copiedOutput = actualOutputs.Single(o => string.Equals(o.Name, "[Not Set]"));
                Assert.That(copiedOutput.Feature, Is.Null);

                IEnumerable<RuleShape> ruleShapes = actualShapes.OfType<RuleShape>();
                Assert.That(ruleShapes.Count(), Is.EqualTo(2));

                IEnumerable<RuleBase> rules = ruleShapes.Select(r => r.Tag).Cast<RuleBase>();
                CollectionAssert.AreEqual(new[] {rule.Name, "Rule - Copy 1"}, rules.Select(r => r.Name));

                RuleBase originalRule = rules.First();
                CollectionAssert.AreEqual(rule.Inputs, originalRule.Inputs);
                CollectionAssert.AreEqual(rule.Outputs, originalRule.Outputs);

                RuleBase copiedRule = rules.Last();
                Assert.That(copiedRule.Inputs.Single(), Is.Not.SameAs(input)); // There are only two inputs present, therefore the new rule should not match the original input
                Assert.That(copiedRule.Outputs.Single(), Is.SameAs(copiedOutput));
            }
        }

        public interface IControlGroupEditorController
        {
            void AddShapesToControlGroupAndPlace(IList<RuleBase> rules, IList<ConditionBase> conditions,
                                                 IList<Input> inputs, IList<Output> outputs,
                                                 IList<SignalBase> signals, IList<MathematicalExpression> mathExpressions,
                                                 Point mea);

            void AddConnections(IList<RuleBase> rules, IList<ConditionBase> conditions,
                                IList<SignalBase> signals, IList<MathematicalExpression> mathExpressions,
                                bool skipValidation = false);
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
            /// Copies the shapes to the <see cref="IControlGroupEditorController"/>.
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

                List<RuleBase> copiedRules = CopyRules(inputMapping, outputMapping);

                ControlGroup controlGroup = controller.ControlGroup;
                RenameCopiedDataWithUniqueNames(copiedRules, controlGroup.Rules, "Rule");

                List<Output> copiedOutputs = outputMapping.Values.ToList();
                ResetOutputs(copiedOutputs);
                controller.AddShapesToControlGroupAndPlace(copiedRules,
                                                           new List<ConditionBase>(), 
                                                           inputMapping.Values.ToList(),
                                                           copiedOutputs,
                                                           new List<SignalBase>(), 
                                                           new List<MathematicalExpression>(),
                                                           mea);

                controller.AddConnections(copiedRules, new List<ConditionBase>(), new List<SignalBase>(), new List<MathematicalExpression>(), true);
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

            private List<RuleBase> CopyRules(IReadOnlyDictionary<Input, Input> inputMapping, 
                                             IReadOnlyDictionary<Output, Output> outputMapping)
            {
                IEnumerable<RuleBase> rules = copiedShapes.Select(s => s.Tag).OfType<RuleBase>();

                var copiedRules = new List<RuleBase>();
                foreach (RuleBase rule in rules)
                {
                    var copiedRule = (RuleBase) rule.Clone();
                    SetInputs(copiedRule, rule, inputMapping);
                    SetOutputs(copiedRule, rule, outputMapping);
                    copiedRules.Add(copiedRule);
                }

                return copiedRules;
            }

            private static void ResetOutputs(IEnumerable<Output> outputs)
            {
                foreach (Output output in outputs)
                {
                    log.InfoFormat("It is not possible to copy and paste internal data for control group outputs, the connection to {0} will be reset.", output.Name);
                    output.Reset();
                }
            }

            private static void SetInputs(RuleBase target, RuleBase source, IReadOnlyDictionary<Input, Input> inputMapping)
            {
                var inputsToAdd = new List<Input>();
                foreach (IInput sourceInput in source.Inputs)
                {
                    var castInput = (Input) sourceInput;
                    inputsToAdd.Add(inputMapping[castInput]);
                }

                target.Inputs.AddRange(inputsToAdd);
            }

            private static void SetOutputs(RuleBase target, RuleBase source, IReadOnlyDictionary<Output, Output> outputMapping)
            {
                Output[] outputsToAdd = source.Outputs.Select(sourceOutput => outputMapping[sourceOutput]).ToArray();
                target.Outputs.AddRange(outputsToAdd);
            }

            private static void RenameCopiedDataWithUniqueNames<T>(IEnumerable<T> copiedData, IEnumerable<T> originalData, string objName)
                where T : RtcBaseObject
            {
                if (!originalData.Any())
                {
                    return;
                }

                var existingNames = new HashSet<string>(originalData.Select(d => d.Name));
                foreach (T copy in copiedData)
                {
                    if (existingNames.Contains(copy.Name))
                    {
                        copy.Name = RealTimeControlModelHelper.GetUniqueName(objName + " - Copy {0}",
                                                                             originalData, "Copy");
                    }
                }
            }
        }
    }
}
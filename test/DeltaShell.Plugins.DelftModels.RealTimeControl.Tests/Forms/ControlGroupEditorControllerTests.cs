using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using Netron.GraphLib;
using Netron.GraphLib.UI;
using NSubstitute;
using NUnit.Framework;
using Connection = Netron.GraphLib.Connection;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms
{
    [TestFixture]
    public class ControlGroupEditorControllerTests
    {
        private ControlGroupEditorController controller;
        private ControlGroup controlGroup;
        private GraphControl graphControl;

        [SetUp]
        public void SetUp()
        {
            controller = new ControlGroupEditorController();
            controlGroup = new ControlGroup();
            graphControl = new GraphControl();

            controller.ControlGroup = controlGroup;
            controller.GraphControl = graphControl;
        }

        [Test]
        public void AddingDomainObjectToControlGroupAddsShapeToGraphControl()
        {
            controlGroup.Rules.Add(new PIDRule());
            controlGroup.Conditions.Add(new StandardCondition());
            controlGroup.Inputs.Add(new Input());
            controlGroup.Outputs.Add(new Output());
            controlGroup.Signals.Add(new LookupSignal());

            Assert.AreEqual(1, controlGroup.Rules.Count());
            Assert.AreEqual(1, controlGroup.Conditions.Count());
            Assert.AreEqual(1, controlGroup.Inputs.Count());
            Assert.AreEqual(1, controlGroup.Outputs.Count());
            Assert.AreEqual(1, controlGroup.Signals.Count());

            Assert.AreEqual(1, graphControl.Shapes.OfType<RuleShape>().Count());
            Assert.AreEqual(1, graphControl.Shapes.OfType<ConditionShape>().Count());
            Assert.AreEqual(1, graphControl.Shapes.OfType<InputItemShape>().Count());
            Assert.AreEqual(1, graphControl.Shapes.OfType<OutputItemShape>().Count());
            Assert.AreEqual(1, graphControl.Shapes.OfType<SignalShape>().Count());
        }

        [Test]
        public void ConnectingItemsDoesNotCreateNewShapes()
        {
            var rule = new PIDRule();
            var input = new Input();
            controlGroup.Rules.Add(rule);
            controlGroup.Inputs.Add(input);

            rule.Inputs.Add(input);

            Assert.AreEqual(1, graphControl.Shapes.OfType<RuleShape>().Count());
            Assert.AreEqual(1, graphControl.Shapes.OfType<InputItemShape>().Count());
        }

        [Test]
        public void RemovingRtcDomainObjectFromControlGroupRemovesShapeFromGraphControl()
        {
            Assert.AreEqual(0, graphControl.Shapes.OfType<RuleShape>().Count());

            var rule1 = new PIDRule("test1");
            controlGroup.Rules.Add(rule1);
            Assert.AreEqual(1, graphControl.Shapes.OfType<RuleShape>().Count());

            var rule2 = new PIDRule("test2") {LongName = "Long text test2"};
            controlGroup.Rules.Add(rule2);
            Assert.AreEqual(2, graphControl.Shapes.OfType<RuleShape>().Count());

            controlGroup.Rules.Remove(rule1);
            Assert.AreEqual(1, graphControl.Shapes.OfType<RuleShape>().Count());

            RuleShape ruleShape = graphControl.Shapes.OfType<RuleShape>().FirstOrDefault();
            Assert.NotNull(ruleShape);
            Assert.AreEqual(rule2.Name, ruleShape.Title);
            Assert.AreEqual(rule2.LongName, ruleShape.Text);
        }

        [Test]
        public void RemovingShapeObjectFromGraphRemovesRtcObjectFromControlGroup()
        {
            var rule1 = new PIDRule("test1");
            controlGroup.Rules.Add(rule1);
            var rule2 = new PIDRule("test2");
            controlGroup.Rules.Add(rule2);
            graphControl.Shapes.Remove(graphControl.Shapes.OfType<RuleShape>().FirstOrDefault());

            Assert.AreEqual(1, controlGroup.Rules.Count());
            Assert.AreEqual(rule2.Name, controlGroup.Rules.FirstOrDefault().Name);
        }

        [Test]
        public void RenamingRtcDomainObjectRenamesShape()
        {
            var rule = new PIDRule("test1");
            controlGroup.Rules.Add(rule);

            rule.Name = "test2";
            rule.LongName = "Long name test2";

            RuleShape ruleShape = graphControl.Shapes.OfType<RuleShape>().FirstOrDefault();

            Assert.NotNull(ruleShape);
            Assert.AreEqual(rule.Name, ruleShape.Title);
            Assert.AreEqual(rule.LongName, ruleShape.Text);
        }

        [Test]
        public void ChangingRuleOverwritesTagInShapeWithNewRule()
        {
            var rule1 = new PIDRule();
            controlGroup.Rules.Add(rule1);
            var rule2 = new IntervalRule();
            controlGroup.Rules[0] = rule2;

            Assert.AreEqual(rule2, graphControl.Shapes.OfType<RuleShape>().FirstOrDefault().Tag);
        }

        [Test]
        public void ConvertDomainObjectToShapeObject()
        {
            var controlGroupEditorController = new ControlGroupEditorController();
            Assert.IsTrue(controlGroupEditorController.ObjectToShape(new PIDRule()) is RuleShape);
            Assert.IsTrue(controlGroupEditorController.ObjectToShape(new StandardCondition()) is ConditionShape);
            Assert.IsTrue(controlGroupEditorController.ObjectToShape(new Input()) is InputItemShape);
            Assert.IsTrue(controlGroupEditorController.ObjectToShape(new Output()) is OutputItemShape);
            Assert.IsTrue(controlGroupEditorController.ObjectToShape(new LookupSignal()) is SignalShape);
        }

        [Test]
        public void RemovingShapeFromGraphControlRemovesCorrespondingDomainObject()
        {
            var rule = new PIDRule();
            controlGroup.Rules.Add(rule);

            Assert.AreEqual(1, controlGroup.Rules.Count);

            ShapeCollection shapes = controller.GraphControl.Shapes;
            controller.GraphControlShapesOnShapeRemoved(null, shapes[0]);

            Assert.AreEqual(0, controlGroup.Rules.Count);
        }

        [Test]
        public void ConvertPIDRuleToIntervalRule()
        {
            var rule = new PIDRule {Name = "test"};
            controlGroup.Rules.Add(rule);
            Assert.AreEqual(1, controlGroup.Rules.Count);

            // implicit action made by ControlGroup.OnCollectionChanged
            var changeCounter = 0;
            graphControl.OnShapeAdded += (sender, shape) => changeCounter++;
            graphControl.OnShapeRemoved += (sender, shape) => changeCounter++;

            controller.ConvertRuleTypeTo(controlGroup.Rules[0], typeof(IntervalRule));
            Assert.AreEqual(0, changeCounter);

            Assert.AreEqual(1, controlGroup.Rules.Count);
            Assert.AreEqual(typeof(IntervalRule), controlGroup.Rules[0].GetType());
            Assert.AreEqual(rule.Name, controlGroup.Rules[0].Name);
            Assert.AreEqual(rule.LongName, controlGroup.Rules[0].LongName);
        }

        [Test]
        public void ConvertStandardConditionToTimeCondition()
        {
            var condition = new StandardCondition {Name = "test"};
            controlGroup.Conditions.Add(condition);
            Assert.AreEqual(1, controlGroup.Conditions.Count);

            // implicit action made by ControlGroup.OnCollectionChanged
            var changeCounter = 0;
            graphControl.OnShapeAdded += (sender, shape) => changeCounter++;
            graphControl.OnShapeRemoved += (sender, shape) => changeCounter++;

            controller.ConvertConditionTypeTo(condition, typeof(TimeCondition));
            Assert.AreEqual(0, changeCounter);

            Assert.AreEqual(1, controlGroup.Conditions.Count);
            Assert.AreEqual(typeof(TimeCondition), controlGroup.Conditions[0].GetType());
            Assert.AreEqual(condition.Name, controlGroup.Conditions[0].Name);
            Assert.AreEqual(condition.LongName, controlGroup.Conditions[0].LongName);
        }

        [Test]
        public void ConvertStandardConditionToTimeConditionShouldDisconnectInputFromTimeCondition()
        {
            var condition = new StandardCondition {Name = "test"};
            var input = new Input
            {
                ParameterName = "InParam",
                Feature = new RtcTestFeature {Name = "In"}
            };
            condition.Input = input;
            controlGroup.Conditions.Add(condition);
            controlGroup.Inputs.Add(input);

            ControlGroup controllerControlGroup = controller.ControlGroup;
            controller.AddConnections(controllerControlGroup.Rules, controllerControlGroup.Conditions, controllerControlGroup.Signals, controllerControlGroup.MathematicalExpressions);
            Assert.AreEqual(1, graphControl.Connections.Count);
            controller.ConvertConditionTypeTo(condition, typeof(TimeCondition));
            Assert.AreEqual(0, graphControl.Connections.Count);
        }

        [Test]
        public void ConversionDoesNotRequireGraphControl()
        {
            controller.GraphControl = null;
            ConvertPIDRuleToIntervalRule();
        }

        [Test]
        public void ConversionOfRuleTypeDoesNotAffectShape()
        {
            var rule = new PIDRule {Name = "test"};
            controlGroup.Rules.Add(rule);

            RuleShape shape = graphControl.Shapes.OfType<RuleShape>().FirstOrDefault();

            Assert.AreEqual(rule.Name, ((RuleBase) shape.Tag).Name);
            Assert.IsTrue(shape.Tag is PIDRule);

            controller.ConvertRuleTypeTo(controlGroup.Rules[0], typeof(IntervalRule));

            Assert.AreEqual(rule.Name, ((RuleBase) shape.Tag).Name);
            Assert.AreEqual(rule.LongName, ((RuleBase) shape.Tag).LongName);
            Assert.IsTrue(shape.Tag is IntervalRule);
        }

        [Test]
        public void AddObjectAndShapeAtSpecificLocation()
        {
            var objecten = new List<object>();
            objecten.Add(new Input());
            List<RuleBase> rule = objecten.Where(c => c is RuleBase).Cast<RuleBase>().ToList();
            List<ConditionBase> condition = objecten.Where(c => c is ConditionBase).Cast<ConditionBase>().ToList();
            List<Input> input = objecten.Where(c => c is Input).Cast<Input>().ToList();
            List<Output> output = objecten.Where(c => c is Output).Cast<Output>().ToList();
            List<SignalBase> signal = objecten.Where(c => c is SignalBase).Cast<SignalBase>().ToList();
            List<MathematicalExpression> mathExpression = objecten.Where(c => c is MathematicalExpression).Cast<MathematicalExpression>().ToList();
            controller.AddShapesToControlGroupAndPlace(rule, condition, input, output, signal, mathExpression, new Point(10, 11));

            Assert.AreEqual(1, graphControl.Shapes.OfType<InputItemShape>().Count());
            Assert.AreEqual(10, graphControl.Shapes[0].X);
            Assert.AreEqual(11, graphControl.Shapes[0].Y);

            objecten.Add(new HydraulicRule());
            List<RuleBase> rule2 = objecten.Where(c => c is RuleBase).Cast<RuleBase>().ToList();
            List<ConditionBase> condition2 = objecten.Where(c => c is ConditionBase).Cast<ConditionBase>().ToList();
            var input2 = new List<Input>();
            List<Output> output2 = objecten.Where(c => c is Output).Cast<Output>().ToList();
            List<SignalBase> signal2 = objecten.Where(c => c is SignalBase).Cast<SignalBase>().ToList();
            List<MathematicalExpression> mathExpression2 = objecten.Where(c => c is MathematicalExpression).Cast<MathematicalExpression>().ToList();
            controller.AddShapesToControlGroupAndPlace(rule2, condition2, input2, output2, signal2, mathExpression2, new Point(100, 110));

            Assert.AreEqual(1, graphControl.Shapes.OfType<InputItemShape>().Count());
            Assert.AreEqual(1, graphControl.Shapes.OfType<RuleShape>().Count());
            Assert.AreEqual(100, graphControl.Shapes[1].X);
            Assert.AreEqual(110, graphControl.Shapes[1].Y);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemovingFeatureResetsInputToDefault()
        {
            var input = new Input
            {
                ParameterName = "p",
                Feature = new RtcTestFeature {Name = "f"}
            };

            controlGroup.Inputs.Add(input);

            InputItemShape itemShape = graphControl.Shapes.OfType<InputItemShape>().FirstOrDefault();

            Assert.NotNull(itemShape);
            Assert.AreEqual("f_p", itemShape.Title);

            input.Feature = null;

            Assert.AreEqual("[Not Set]", itemShape.Title);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddInputConnectionToMathematicalExpressionAddsTagToShape()
        {
            // Given
            controlGroup.Inputs.Add(new Input());
            controlGroup.MathematicalExpressions.Add(new MathematicalExpression());
            controlGroup.MathematicalExpressions.Add(new MathematicalExpression());

            Connector inputAConnector = graphControl.Shapes[0].Connectors[0];
            Connector inputBConnector = graphControl.Shapes[1].Connectors[2];
            Connector mathExpressionConnector = graphControl.Shapes[2].Connectors[1];

            // Preconditions
            Assert.That(inputAConnector.Name, Is.EqualTo("Bottom"));
            Assert.That(inputBConnector.Name, Is.EqualTo("Bottom"));
            Assert.That(mathExpressionConnector.Name, Is.EqualTo("Top"));

            // When
            graphControl.AddConnection(inputAConnector, mathExpressionConnector);
            graphControl.AddConnection(inputBConnector, mathExpressionConnector);

            // Then
            Assert.That(inputAConnector.Connections[0].Text, Is.EqualTo("A"));
            Assert.That(inputBConnector.Connections[0].Text, Is.EqualTo("B"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddConditionToMathematicalExpressionDoesNotThrowException()
        {
            // Given
            var condition = new StandardCondition();
            var mathematicalExpression = new MathematicalExpression();
            controlGroup.Conditions.Add(condition);
            controlGroup.MathematicalExpressions.Add(mathematicalExpression);
            Shape conditionShape = controller.GraphControl.Shapes[0];
            Shape mathExpression = controller.GraphControl.Shapes[1];

            // When
            Connector mathExpressionConnector = mathExpression.Connectors[0];
            Connector conditionShapeConnector = conditionShape.Connectors[3];
            Assert.That(mathExpressionConnector.Name, Is.EqualTo("Left"));
            Assert.That(conditionShapeConnector.Name, Is.EqualTo("Bottom"));
            TestDelegate testAction = () => controller.GraphControl.AddConnection(conditionShapeConnector, mathExpressionConnector);

            // Then
            Assert.That(testAction, Throws.Nothing);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemovingExistingConnectionFromMathematicalExpressionReshufflesTags()
        {
            // Given
            var firstInput = new Input
            {
                ParameterName = "p",
                Feature = new RtcTestFeature { Name = "f" }
            };
            var secondInput = new Input
            {
                ParameterName = "p",
                Feature = new RtcTestFeature { Name = "f" }
            };
            var mathematicalExpression = new MathematicalExpression();
            controlGroup.Inputs.Add(firstInput);
            controlGroup.Inputs.Add(secondInput);
            controlGroup.MathematicalExpressions.Add(mathematicalExpression);
            Shape firstInputShape = controller.GraphControl.Shapes[0];
            Shape secondInputShape = controller.GraphControl.Shapes[1];
            Shape mathExpression = controller.GraphControl.Shapes[2];

            // With
            Assert.That(mathExpression.Connectors[1].Name, Is.EqualTo("Top"));
            Assert.That(firstInputShape.Connectors[0].Name, Is.EqualTo("Bottom"));
            Assert.That(secondInputShape.Connectors[0].Name, Is.EqualTo("Bottom"));
            controller.GraphControl.AddConnection(firstInputShape.Connectors[0], mathExpression.Connectors[1]);
            controller.GraphControl.AddConnection(secondInputShape.Connectors[0], mathExpression.Connectors[1]);

            Assert.That(controller.GraphControl.Connections.Count, Is.EqualTo(2));
            Connection firstConnection = controller.GraphControl.Connections[0];
            Assert.That(firstConnection.From.BelongsTo, Is.EqualTo(firstInputShape));
            Assert.That(firstConnection.Text, Is.EqualTo("A"));
            Assert.That(controller.GraphControl.Connections[1].Text, Is.EqualTo("B"));
            
            // When
            controller.GraphControl.Connections.Remove(firstConnection);

            // Then
            Assert.That(controller.GraphControl.Connections.Count, Is.EqualTo(1));
            Assert.That(controller.GraphControl.Connections[0].Text, Is.EqualTo("B"));
            Assert.That(controller.GraphControl.Connections[0].From.BelongsTo, Is.EqualTo(secondInputShape));
        }

        [Test]
        public void RemovingControlGroupFromControllerCleansGraphControl()
        {
            controlGroup.Rules.Add(new PIDRule());
            Assert.AreEqual(1, controller.GraphControl.Shapes.Count);
            controller.ControlGroup = null;
            Assert.AreEqual(0, controller.GraphControl.Shapes.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RenamingFeatureOnOutputChangesTextOnShape()
        {
            var feature = new RtcTestFeature {Name = "feature"};
            var output = new Output
            {
                Feature = feature,
                ParameterName = "parameter"
            };

            controlGroup.Outputs.Add(output);

            feature.Name = "feature2"; // change

            Assert.AreEqual("feature2_parameter", ((ShapeBase) graphControl.Shapes[0]).Title, "changing feature name should update shape text");
        }

        [Test]
        public void AllowConditionToConditionRelation()
        {
            var condition1 = new StandardCondition();
            var condition2 = new StandardCondition();
            condition1.TrueOutputs.Add(condition2);

            var controlGroupOfConditions = new ControlGroup();

            controlGroupOfConditions.Conditions.Add(condition1);
            controlGroupOfConditions.Conditions.Add(condition2);

            controller.ControlGroup = controlGroupOfConditions;

            Assert.AreEqual(1, graphControl.Connections.Count);

            Connection connection = graphControl.Connections[0];

            Assert.IsTrue(ControlGroupEditorController.ConnectionIs(connection));
        }

        [Test]
        public void InputCanConnectToInput()
        {
            var source = new Input();
            var target = new Input();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void InputCanConnectToCondition()
        {
            var source = new Input();
            var target = new StandardCondition();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(1, allowedConnections.Count);
            Assert.AreEqual(ConnectorType.Top, allowedConnections[ConnectorType.Bottom]);
        }

        [Test]
        public void InputCanConnectToRule()
        {
            var input = new Input();
            var hydraulicRule = new HydraulicRule();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(input, hydraulicRule);
            Assert.AreEqual(1, allowedConnections.Count);
            Assert.AreEqual(ConnectorType.Top, allowedConnections[ConnectorType.Bottom]);
        }

        [Test]
        public void InputCanConnectToSignal()
        {
            var input = new Input();
            var lookupSignal = new LookupSignal();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(input, lookupSignal);
            Assert.AreEqual(1, allowedConnections.Count);
            Assert.AreEqual(ConnectorType.Top, allowedConnections[ConnectorType.Bottom]);
        }

        [Test]
        public void InputCanConnectToOutput()
        {
            var input = new Input();
            var output = new Output();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(input, output);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void ConditionCanConnectToInput()
        {
            var source = new StandardCondition();
            var target = new Input();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void ConditionCanConnectToCondition()
        {
            var source = new StandardCondition();
            var target = new StandardCondition();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(2, allowedConnections.Count);
            Assert.AreEqual(ConnectorType.Left, allowedConnections[ConnectorType.Right]);
            Assert.AreEqual(ConnectorType.Left, allowedConnections[ConnectorType.Bottom]);
        }

        [Test]
        public void ConditionCanConnectToRule()
        {
            var source = new StandardCondition();
            var target = new HydraulicRule();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(2, allowedConnections.Count);
            Assert.AreEqual(ConnectorType.Left, allowedConnections[ConnectorType.Right]);
            Assert.AreEqual(ConnectorType.Left, allowedConnections[ConnectorType.Bottom]);
        }

        [Test]
        public void ConditionCanConnectToOutput()
        {
            var source = new StandardCondition();
            var output = new Output();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, output);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void ConditionCanConnectToSignal()
        {
            var source = new StandardCondition();
            var output = new LookupSignal();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, output);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void RuleCanConnectToInput()
        {
            var source = new HydraulicRule();
            var target = new Input();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void RuleCanConnectToCondition()
        {
            var source = new HydraulicRule();
            var target = new StandardCondition();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void RuleCanConnectToRule()
        {
            var source = new HydraulicRule();
            var target = new HydraulicRule();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void RuleCanConnectToSignal()
        {
            var source = new HydraulicRule();
            var target = new LookupSignal();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void RuleCanConnectToOutput()
        {
            var source = new HydraulicRule();
            var output = new Output();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, output);
            Assert.AreEqual(1, allowedConnections.Count);
            Assert.AreEqual(ConnectorType.Left, allowedConnections[ConnectorType.Right]);
        }

        [Test]
        public void OutputCanConnectToInput()
        {
            var source = new Output();
            var target = new Input();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void OutputCanConnectToCondition()
        {
            var source = new Output();
            var target = new StandardCondition();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void OutputCanConnectToRule()
        {
            var source = new Output();
            var target = new HydraulicRule();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void OutputCanConnectToSignal()
        {
            var source = new Output();
            var target = new LookupSignal();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void OutputCanConnectToOutput()
        {
            var source = new Output();
            var output = new Output();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, output);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void SignalCanConnectToInput()
        {
            var source = new LookupSignal();
            var target = new Input();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void SignalCanConnectToCondition()
        {
            var source = new LookupSignal();
            var target = new StandardCondition();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void SignalCanConnectToCertainRules()
        {
            var source = new LookupSignal();
            var target = new HydraulicRule();

            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
            Assert.IsFalse(target.CanBeLinkedFromSignal());

            var target2 = new PIDRule();
            Dictionary<ConnectorType, ConnectorType> allowedConnections2 = GetAllowedConnections(source, target2);
            Assert.AreEqual(1, allowedConnections2.Count);
            Assert.IsTrue(target2.CanBeLinkedFromSignal());
            Assert.AreEqual(ConnectorType.Bottom, allowedConnections2[ConnectorType.Right]);
        }

        [Test]
        public void SignalCanConnectToOutput()
        {
            var source = new LookupSignal();
            var target = new Output();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void SignalCanConnectToSignal()
        {
            var source = new LookupSignal();
            var target = new LookupSignal();
            Dictionary<ConnectorType, ConnectorType> allowedConnections = GetAllowedConnections(source, target);
            Assert.AreEqual(0, allowedConnections.Count);
        }

        [Test]
        public void ConditionCanConnectToOnly1Condition()
        {
            var source = new StandardCondition();
            var target = new StandardCondition();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, target, ConnectorType.Left));
            ControlGroupEditorController.Connect(source, ConnectorType.Right, target);
            var secondRuie = new HydraulicRule();
            Assert.IsFalse(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, secondRuie, ConnectorType.Left));
            var secondCondition = new StandardCondition();
            Assert.IsFalse(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, secondCondition, ConnectorType.Left));
        }

        [Test]
        public void ConditionCanConnectToOnly1Rule()
        {
            var source = new StandardCondition();
            var target = new HydraulicRule();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, target, ConnectorType.Left));
            ControlGroupEditorController.Connect(source, ConnectorType.Right, target);
            var secondRuie = new HydraulicRule();
            Assert.IsFalse(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, secondRuie, ConnectorType.Left));
            var secondCondition = new StandardCondition();
            Assert.IsFalse(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, secondCondition, ConnectorType.Left));
        }

        [Test]
        public void InputCanConnectToMultipleRuleOrConditions()
        {
            var source = new Input();
            var target = new HydraulicRule();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Bottom, target, ConnectorType.Top));
            ControlGroupEditorController.Connect(source, ConnectorType.Bottom, target);
            var secondRuie = new HydraulicRule();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Bottom, secondRuie, ConnectorType.Top));
            ControlGroupEditorController.Connect(source, ConnectorType.Bottom, secondRuie);
            var secondCondition = new StandardCondition();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Bottom, secondCondition, ConnectorType.Top));
            ControlGroupEditorController.Connect(source, ConnectorType.Bottom, secondCondition);
        }

        [Test]
        public void OutputCanConnectFromMultipleRules()
        {
            var source = new HydraulicRule();
            var target = new Output();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, target, ConnectorType.Left));
            ControlGroupEditorController.Connect(source, ConnectorType.Right, target);
            var secondRuie = new HydraulicRule();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(secondRuie, ConnectorType.Right, target, ConnectorType.Left));
            ControlGroupEditorController.Connect(secondRuie, ConnectorType.Right, target);
            var thirdRule = new HydraulicRule();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(thirdRule, ConnectorType.Right, target, ConnectorType.Left));
            ControlGroupEditorController.Connect(thirdRule, ConnectorType.Right, target);
        }

        [Test]
        public void RuleCanConnectFromMultipleConditions()
        {
            var source = new StandardCondition();
            var target = new HydraulicRule();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, target, ConnectorType.Left));
            ControlGroupEditorController.Connect(source, ConnectorType.Right, target);

            // can not connect the same condition again
            Assert.IsFalse(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Right, target, ConnectorType.Left));
            Assert.IsFalse(ControlGroupEditorController.IsConnectionAllowed(source, ConnectorType.Bottom, target, ConnectorType.Left));

            var secondCondition = new StandardCondition();
            Assert.IsTrue(ControlGroupEditorController.IsConnectionAllowed(secondCondition, ConnectorType.Right, target, ConnectorType.Left));
            ControlGroupEditorController.Connect(secondCondition, ConnectorType.Right, target);
        }

        [Test]
        public void ControlGroupModelEventsAreIgnoredInControllerCollectionChanged()
        {
            graphControl = new GraphControl();

            controller.ControlGroup = controlGroup;

            var collectionChangedCount = 0;
            ((INotifyCollectionChanged) controller.ControlGroup).CollectionChanged += (s, e) =>
            {
                collectionChangedCount++;
            };

            controlGroup.Inputs.Add(new Input());
            Assert.AreEqual(1, collectionChangedCount);
        }

        [TestFixture]
        public static class GivenInput
        {
            private static Input input;

            [SetUp]
            public static void Arrange()
            {
                input = new Input();
            }

            [Test]
            public static void WhenDisconnectingRule_ThenInputRemovedFromRule()
            {
                var rule = Substitute.For<RuleBase>();
                rule.Inputs.Add(input);

                ControlGroupEditorController.Disconnect(input, rule);

                Assert.That(rule.Inputs, Is.Empty);
            }

            [Test]
            public static void WhenDisconnectingSignal_ThenInputRemovedFromSignal()
            {
                var signal = Substitute.For<SignalBase>();
                signal.Inputs.Add(input);

                ControlGroupEditorController.Disconnect(input, signal);

                Assert.That(signal.Inputs, Is.Empty);
            }

            [Test]
            public static void WhenDisconnectingCondition_ThenInputRemovedFromCondition()
            {
                var condition = Substitute.For<ConditionBase>();
                condition.Input = input;

                ControlGroupEditorController.Disconnect(input, condition);

                Assert.That(condition.Input, Is.Null);
            }

            [Test]
            public static void WhenDisconnectingMathematicalExpression_ThenInputRemovedFromMathematicalExpression()
            {
                var mathematicalExpression = new MathematicalExpression();
                mathematicalExpression.Inputs.Add(input);

                ControlGroupEditorController.Disconnect(input, mathematicalExpression);

                Assert.That(mathematicalExpression.Inputs, Is.Empty);
            }
        }

        [TestFixture]
        public static class GivenMathematicalExpression
        {
            private static MathematicalExpression mathematicalExpression;

            [SetUp]
            public static void Arrange()
            {
                mathematicalExpression = new MathematicalExpression();
            }

            [Test]
            public static void WhenDisconnectingRule_ThenInputRemovedFromRule()
            {
                var rule = Substitute.For<RuleBase>();
                rule.Inputs.Add(mathematicalExpression);

                ControlGroupEditorController.Disconnect(mathematicalExpression, rule);

                Assert.That(rule.Inputs, Is.Empty);
            }

            [Test]
            public static void WhenDisconnectingCondition_ThenInputRemovedFromCondition()
            {
                var condition = Substitute.For<ConditionBase>();
                condition.Input = mathematicalExpression;

                ControlGroupEditorController.Disconnect(mathematicalExpression, condition);

                Assert.That(condition.Input, Is.Null);
            }

            [Test]
            public static void WhenDisconnectingMathematicalExpression_ThenInputRemovedFromMathematicalExpression()
            {
                var mathematicalExpression2 = new MathematicalExpression();
                mathematicalExpression2.Inputs.Add(mathematicalExpression);

                ControlGroupEditorController.Disconnect(mathematicalExpression, mathematicalExpression2);

                Assert.That(mathematicalExpression2.Inputs, Is.Empty);
            }
        }

        [TestFixture]
        public static class GivenCondition
        {
            private static ConditionBase condition;

            [SetUp]
            public static void Arrange()
            {
                condition = Substitute.For<ConditionBase>();
            }

            [Test]
            public static void WhenDisconnectingRtcBaseObject_ThenRtcBaseObjectRemovedFromConditionOutputs()
            {
                var rtcBaseObject = Substitute.For<RtcBaseObject>();
                condition.FalseOutputs.Add(rtcBaseObject);
                condition.TrueOutputs.Add(rtcBaseObject);

                ControlGroupEditorController.Disconnect(condition, rtcBaseObject);

                Assert.That(condition.FalseOutputs, Is.Empty);
                Assert.That(condition.TrueOutputs, Is.Empty);
            }
        }

        [TestFixture]
        public static class GivenRule
        {
            private static RuleBase rule;

            [SetUp]
            public static void Arrange()
            {
                rule = Substitute.For<RuleBase>();
            }

            [Test]
            public static void WhenDisconnectingOutput_ThenOutputRemovedFromRuleOutputs()
            {
                var output = Substitute.For<Output>();
                rule.Outputs.Add(output);

                ControlGroupEditorController.Disconnect(rule, output);

                Assert.That(rule.Outputs, Is.Empty);
            }
        }

        [TestFixture]
        public static class GivenSignal
        {
            private static SignalBase signal;

            [SetUp]
            public static void Arrange()
            {
                signal = Substitute.For<SignalBase>();
            }

            [Test]
            public static void WhenDisconnectingOutput_ThenOutputRemovedFromRuleOutputs()
            {
                var ruleBase = Substitute.For<RuleBase>();
                signal.RuleBases.Add(ruleBase);

                ControlGroupEditorController.Disconnect(signal, ruleBase);

                Assert.That(signal.RuleBases, Is.Empty);
            }
        }

        private static Dictionary<ConnectorType, ConnectorType> GetAllowedConnections(object from, object to)
        {
            Array connectors = Enum.GetValues(typeof(ConnectorType));
            var allowed = new Dictionary<ConnectorType, ConnectorType>();
            foreach (ConnectorType source in connectors)
            {
                foreach (ConnectorType target in connectors)
                {
                    if (ControlGroupEditorController.IsConnectionAllowed(from, source, to, target))
                    {
                        allowed[source] = target;
                    }
                }
            }

            return allowed;
        }
    }
}
using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms
{
    [TestFixture]
    public class ShapeConnectionsRulesControllerTest
    {
        private static ConnectorType targetBottomConnectorType;

        [SetUp]
        public static void SetupConnectorsTypes()
        {
            targetBottomConnectorType = ConnectorType.Bottom;
        }

        [Test]
        public static void GivenNullSourceShapeExceptionIsThrown()
        {
            ShapeBase sourceShape = null;
            var targetShape = Substitute.For<ShapeBase>();
            Assert.Throws<ArgumentNullException>(
                () => ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape,
                                                                                  targetShape,
                                                                                  targetBottomConnectorType));
        }

        [Test]
        public static void GivenNullTargetShapeExceptionIsThrown()
        {
            ShapeBase targetShape = null;
            var sourceShape = Substitute.For<ShapeBase>();
            Assert.Throws<ArgumentNullException>(
                () => ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape,
                                                                                  targetShape,
                                                                                  targetBottomConnectorType));
        }

        [Test]
        public void IsConnectorSourceCompatibleWithConnectorDestination_SourceShapeSameAsTargetShape_ReturnsFalse()
        {
            // Setup
            var targetShape = Substitute.For<ShapeBase>();
            ShapeBase sourceShape = targetShape;

            // Call
            bool result = ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape,
                                                                                      targetShape,
                                                                                      targetBottomConnectorType);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [TestCaseSource(nameof(GetOutputCompatibleConnectionsTestCaseData))]
        public static void GivenOutputShapeToTargetShapeCorrectConnectorsAreSelected(ShapeBase sourceShape, ShapeBase targetShape, ConnectorType connectorType, bool isCompatible)
        {
            bool result = ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape, targetShape, connectorType);
            Assert.That(result, Is.EqualTo(isCompatible));
        }

        [Test]
        [TestCaseSource(nameof(GetInputCompatibleConnectionsTestCaseData))]
        public static void GivenInputShapeToTargetShapeCorrectConnectorsAreSelected(ShapeBase sourceShape, ShapeBase targetShape, ConnectorType connectorType, bool isCompatible)
        {
            bool result = ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape, targetShape, connectorType);
            Assert.That(result, Is.EqualTo(isCompatible));
        }

        [Test]
        [TestCaseSource(nameof(GetConditionShapeCompatibleConnectionsTestCaseData))]
        public static void GivenConditionShapeToTargetShapeCorrectConnectorsAreSelected(ShapeBase sourceShape, ShapeBase targetShape, ConnectorType connectorType, bool isCompatible)
        {
            bool result = ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape, targetShape, connectorType);
            Assert.That(result, Is.EqualTo(isCompatible));
        }

        [Test]
        [TestCaseSource(nameof(GetSignalShapeCompatibleConnectionsTestCaseData))]
        public static void GivenSignalShapeToTargetShapeCorrectConnectorsAreSelected(ShapeBase sourceShape, ShapeBase targetShape, ConnectorType connectorType, bool isCompatible)
        {
            bool result = ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape, targetShape, connectorType);
            Assert.That(result, Is.EqualTo(isCompatible));
        }

        [Test]
        [TestCaseSource(nameof(GetRuleShapeCompatibleConnectionsTestCaseData))]
        public static void GivenRuleShapeToTargetShapeCorrectConnectorsAreSelected(ShapeBase sourceShape, ShapeBase targetShape, ConnectorType connectorType, bool isCompatible)
        {
            bool result = ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape, targetShape, connectorType);
            Assert.That(result, Is.EqualTo(isCompatible));
        }

        [Test]
        [TestCaseSource(nameof(GetMathematicalExpressionShapeCompatibleConnectionsTestCaseData))]
        public static void GivenMathematicalExpressionShapeToTargetShapeCorrectConnectorsAreSelected(ShapeBase sourceShape, ShapeBase targetShape, ConnectorType connectorType, bool isCompatible)
        {
            bool result = ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape, targetShape, connectorType);
            Assert.That(result, Is.EqualTo(isCompatible));
        }

        private class TestShape : ShapeBase
        {
            protected override void Initialize()
            {
                // Test object and thus nothing should happen
            }
        }

        private static IEnumerable<TestCaseData> GetOutputCompatibleConnectionsTestCaseData()
        {
            yield return new TestCaseData(new OutputItemShape(), new TestShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new OutputItemShape(), new TestShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new OutputItemShape(), new TestShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new OutputItemShape(), new TestShape(), ConnectorType.Right, false);
        }

        private static IEnumerable<TestCaseData> GetInputCompatibleConnectionsTestCaseData()
        {
            yield return new TestCaseData(new InputItemShape(), new ConditionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new InputItemShape(), new ConditionShape(), ConnectorType.Top, true);
            yield return new TestCaseData(new InputItemShape(), new ConditionShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new InputItemShape(), new ConditionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new InputItemShape(), new SignalShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new InputItemShape(), new SignalShape(), ConnectorType.Top, true);
            yield return new TestCaseData(new InputItemShape(), new SignalShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new InputItemShape(), new SignalShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new InputItemShape(), new RuleShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new InputItemShape(), new RuleShape(), ConnectorType.Top, true);
            yield return new TestCaseData(new InputItemShape(), new RuleShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new InputItemShape(), new RuleShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new InputItemShape(), new MathematicalExpressionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new InputItemShape(), new MathematicalExpressionShape(), ConnectorType.Top, true);
            yield return new TestCaseData(new InputItemShape(), new MathematicalExpressionShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new InputItemShape(), new MathematicalExpressionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new InputItemShape(), new InputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new InputItemShape(), new InputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new InputItemShape(), new InputItemShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new InputItemShape(), new InputItemShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new InputItemShape(), new OutputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new InputItemShape(), new OutputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new InputItemShape(), new OutputItemShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new InputItemShape(), new OutputItemShape(), ConnectorType.Right, false);
        }

        private static IEnumerable<TestCaseData> GetConditionShapeCompatibleConnectionsTestCaseData()
        {
            yield return new TestCaseData(new ConditionShape(), new InputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new ConditionShape(), new InputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new ConditionShape(), new InputItemShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new ConditionShape(), new InputItemShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new ConditionShape(), new SignalShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new ConditionShape(), new SignalShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new ConditionShape(), new SignalShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new ConditionShape(), new SignalShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new ConditionShape(), new RuleShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new ConditionShape(), new RuleShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new ConditionShape(), new RuleShape(), ConnectorType.Left, true);
            yield return new TestCaseData(new ConditionShape(), new RuleShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new ConditionShape(), new MathematicalExpressionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new ConditionShape(), new MathematicalExpressionShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new ConditionShape(), new MathematicalExpressionShape(), ConnectorType.Left, true);
            yield return new TestCaseData(new ConditionShape(), new MathematicalExpressionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new ConditionShape(), new ConditionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new ConditionShape(), new ConditionShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new ConditionShape(), new ConditionShape(), ConnectorType.Left, true);
            yield return new TestCaseData(new ConditionShape(), new ConditionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new ConditionShape(), new OutputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new ConditionShape(), new OutputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new ConditionShape(), new OutputItemShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new ConditionShape(), new OutputItemShape(), ConnectorType.Right, false);
        }

        private static IEnumerable<TestCaseData> GetSignalShapeCompatibleConnectionsTestCaseData()
        {
            yield return new TestCaseData(new SignalShape(), new InputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new SignalShape(), new InputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new SignalShape(), new InputItemShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new SignalShape(), new InputItemShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new SignalShape(), new ConditionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new SignalShape(), new ConditionShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new SignalShape(), new ConditionShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new SignalShape(), new ConditionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new SignalShape(), new RuleShape(), ConnectorType.Bottom, true);
            yield return new TestCaseData(new SignalShape(), new RuleShape(), ConnectorType.Top, true);
            yield return new TestCaseData(new SignalShape(), new RuleShape(), ConnectorType.Left, true);
            yield return new TestCaseData(new SignalShape(), new RuleShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new SignalShape(), new MathematicalExpressionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new SignalShape(), new MathematicalExpressionShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new SignalShape(), new MathematicalExpressionShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new SignalShape(), new MathematicalExpressionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new SignalShape(), new SignalShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new SignalShape(), new SignalShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new SignalShape(), new SignalShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new SignalShape(), new SignalShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new SignalShape(), new OutputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new SignalShape(), new OutputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new SignalShape(), new OutputItemShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new SignalShape(), new OutputItemShape(), ConnectorType.Right, false);
        }

        private static IEnumerable<TestCaseData> GetRuleShapeCompatibleConnectionsTestCaseData()
        {
            yield return new TestCaseData(new RuleShape(), new InputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new RuleShape(), new InputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new RuleShape(), new InputItemShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new RuleShape(), new InputItemShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new RuleShape(), new ConditionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new RuleShape(), new ConditionShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new RuleShape(), new ConditionShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new RuleShape(), new ConditionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new RuleShape(), new SignalShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new RuleShape(), new SignalShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new RuleShape(), new SignalShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new RuleShape(), new SignalShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new RuleShape(), new MathematicalExpressionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new RuleShape(), new MathematicalExpressionShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new RuleShape(), new MathematicalExpressionShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new RuleShape(), new MathematicalExpressionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new RuleShape(), new RuleShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new RuleShape(), new RuleShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new RuleShape(), new RuleShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new RuleShape(), new RuleShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new RuleShape(), new OutputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new RuleShape(), new OutputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new RuleShape(), new OutputItemShape(), ConnectorType.Left, true);
            yield return new TestCaseData(new RuleShape(), new OutputItemShape(), ConnectorType.Right, false);
        }

        private static IEnumerable<TestCaseData> GetMathematicalExpressionShapeCompatibleConnectionsTestCaseData()
        {
            yield return new TestCaseData(new MathematicalExpressionShape(), new InputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new InputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new InputItemShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new InputItemShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new MathematicalExpressionShape(), new ConditionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new ConditionShape(), ConnectorType.Top, true);
            yield return new TestCaseData(new MathematicalExpressionShape(), new ConditionShape(), ConnectorType.Left, true);
            yield return new TestCaseData(new MathematicalExpressionShape(), new ConditionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new MathematicalExpressionShape(), new SignalShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new SignalShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new SignalShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new SignalShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new MathematicalExpressionShape(), new RuleShape(), ConnectorType.Bottom, true);
            yield return new TestCaseData(new MathematicalExpressionShape(), new RuleShape(), ConnectorType.Top, true);
            yield return new TestCaseData(new MathematicalExpressionShape(), new RuleShape(), ConnectorType.Left, true);
            yield return new TestCaseData(new MathematicalExpressionShape(), new RuleShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new MathematicalExpressionShape(), new MathematicalExpressionShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new MathematicalExpressionShape(), ConnectorType.Top, true);
            yield return new TestCaseData(new MathematicalExpressionShape(), new MathematicalExpressionShape(), ConnectorType.Left, false);
            yield return new TestCaseData(new MathematicalExpressionShape(), new MathematicalExpressionShape(), ConnectorType.Right, false);

            yield return new TestCaseData(new RuleShape(), new OutputItemShape(), ConnectorType.Bottom, false);
            yield return new TestCaseData(new RuleShape(), new OutputItemShape(), ConnectorType.Top, false);
            yield return new TestCaseData(new RuleShape(), new OutputItemShape(), ConnectorType.Left, true);
            yield return new TestCaseData(new RuleShape(), new OutputItemShape(), ConnectorType.Right, false);
        }
    }
}
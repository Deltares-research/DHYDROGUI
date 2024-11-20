using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.NodePresenters
{
    [TestFixture]
    public class ControlGroupNodePresenterTest
    {
        [Test]
        public void ControlGroupNodePresenter_AssignsNameImage_ToNode()
        {
            // arrange
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();
            var guiMock = Substitute.For<GuiPlugin>();
            const string name = @"ControlGroupTestName";
            var controlGroup = new ControlGroup() {Name = name};
            var controlGroupNodePresenter = new ControlGroupNodePresenter(guiMock);

            // Act
            controlGroupNodePresenter.UpdateNode(parentNode, node, controlGroup);

            // Assert
            node.Received(1).Text = name;
            node.Received(1).Image = Arg.Any<Bitmap>();
        }

        [Test]
        public void ControlGroupNodePresenter_GetChildNodeObjects_NotNull()
        {
            // arrange
            var node = Substitute.For<ITreeNode>();
            var guiMock = Substitute.For<GuiPlugin>();
            const string name = @"ControlGroupTestName";
            var controlGroup = new ControlGroup {Name = name};
            var controlGroupNodePresenter = new ControlGroupNodePresenter(guiMock);

            // Act
            IEnumerable results = controlGroupNodePresenter.GetChildNodeObjects(controlGroup, node);

            // Assert
            Assert.IsNotNull(results);
        }

        [Test]
        [TestCaseSource(nameof(GetChildNodeObjectsData))]
        public void GetChildNodeObjects_ExpectedResults(object element, Action<ControlGroup, object> addElement)
        {
            // arrange
            var guiMock = Substitute.For<GuiPlugin>();

            const string name = @"ControlGroupTestName";
            var controlGroup = new ControlGroup {Name = name};

            addElement.Invoke(controlGroup, element);

            var controlGroupNodePresenter = new ControlGroupNodePresenter(guiMock);
            var node = Substitute.For<ITreeNode>();

            // Act
            IEnumerable result = controlGroupNodePresenter.GetChildNodeObjects(controlGroup, node);

            // Assert
            Assert.That(result, Is.EqualTo(new[]
            {
                element
            }));
        }

        private static IEnumerable<TestCaseData> GetChildNodeObjectsData()
        {
            void AddInput(ControlGroup c, object input) => c.Inputs.Add((Input) input);
            yield return new TestCaseData(new Input(), (Action<ControlGroup, object>) AddInput);

            void AddOutput(ControlGroup c, object output) => c.Outputs.Add((Output) output);
            yield return new TestCaseData(new Output(), (Action<ControlGroup, object>) AddOutput);

            void AddCondition(ControlGroup c, object condition) => c.Conditions.Add((ConditionBase) condition);
            yield return new TestCaseData(new StandardCondition(), (Action<ControlGroup, object>) AddCondition);

            void AddRule(ControlGroup c, object rule) => c.Rules.Add((RuleBase) rule);
            yield return new TestCaseData(new PIDRule(), (Action<ControlGroup, object>) AddRule);

            void AddSignal(ControlGroup c, object signal) => c.Signals.Add((SignalBase) signal);
            yield return new TestCaseData(new LookupSignal(), (Action<ControlGroup, object>) AddSignal);

            void AddMathematicalExpression(ControlGroup c, object mathematicalExpression) =>
                c.MathematicalExpressions.Add((MathematicalExpression) mathematicalExpression);

            yield return new TestCaseData(new MathematicalExpression(),
                                          (Action<ControlGroup, object>) AddMathematicalExpression);
        }
    }
}
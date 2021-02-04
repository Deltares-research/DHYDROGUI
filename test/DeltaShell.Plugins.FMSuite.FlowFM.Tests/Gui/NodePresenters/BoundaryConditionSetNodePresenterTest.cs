using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.NodePresenters
{
    [TestFixture]
    public class BoundaryConditionSetNodePresenterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var nodePresenter = new BoundaryConditionSetNodePresenter();

            // Assert
            Assert.That(nodePresenter, Is.InstanceOf<FMSuiteNodePresenterBase<BoundaryConditionSet>>());
        }

        [Test]
        public void CanRenameNode_Always_ReturnsTrue()
        {
            // Setup
            var nodePresenter = new BoundaryConditionSetNodePresenter();

            // Call
            bool canRenameNode = nodePresenter.CanRenameNode(null);

            // Assert
            Assert.That(canRenameNode, Is.True);
        }

        [Test]
        public void OnNodeRenamed_Always_ReturnsTrue()
        {
            // Setup
            const string newName = "NewName";

            var feature = new Feature2D();
            var set = new BoundaryConditionSet
            {
                Feature = feature
            };

            var nodePresenter = new BoundaryConditionSetNodePresenter();

            // Call
            nodePresenter.OnNodeRenamed(set, newName);

            // Assert
            Assert.That(feature.Name, Is.EqualTo(newName));
        }

        [Test]
        public void UpdateNode_NodeDataWithFeature_UpdatesNode()
        {
            // Setup
            const string featureName = "Feature Name";
            var feature = new Feature2D
            {
                Name = featureName
            };
            var nodeData = new BoundaryConditionSet
            {
                Feature = feature
            };

            var node = Substitute.For<ITreeNode>();
            var nodePresenter = new BoundaryConditionSetNodePresenter();

            // Precondition
            Assert.That(node.Image, Is.Null);
            Assert.That(node.Text, Is.Empty);

            // Call
            nodePresenter.UpdateNode(null, node, nodeData);

            // Assert
            Assert.That(node.Image, Is.Not.Null);
            Assert.That(node.Text, Is.EqualTo(featureName));
        }

        [Test]
        public void UpdateNode_NodeDataWithoutFeature_UpdatesNode()
        {
            // Setup
            var nodeData = new BoundaryConditionSet();

            var node = Substitute.For<ITreeNode>();
            var nodePresenter = new BoundaryConditionSetNodePresenter();

            // Precondition
            Assert.That(nodeData.Feature, Is.Null);
            Assert.That(node.Image, Is.Null);
            Assert.That(node.Text, Is.Empty);

            // Call
            nodePresenter.UpdateNode(null, node, nodeData);

            // Assert
            Assert.That(node.Image, Is.Not.Null);
            Assert.That(node.Text, Is.EqualTo("<error>"));
        }

        [Test]
        public void RemoveNodeData_WithParentNodeIncompatibleType_ReturnsFalseAndKeepsSelection()
        {
            // Setup
            var guiSelection = new object();
            var gui = Substitute.For<IGui>();
            gui.Selection = guiSelection;
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var boundaryCondition = new BoundaryConditionSet();

            var nodePresenter = new BoundaryConditionSetNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(null, boundaryCondition);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(gui.Selection, Is.SameAs(guiSelection));
        }

        [Test]
        public void RemoveNodeData_WithBoundaryConditionSetCollectionParentNodeContainingNodeData_ReturnsTrueAndRemovesNodeDataAndResetsGuiSelection()
        {
            // Setup
            var gui = Substitute.For<IGui>();
            gui.Selection = new object();
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var boundaryCondition = new BoundaryConditionSet();
            var parentNode = new List<BoundaryConditionSet>
            {
                boundaryCondition
            };

            var nodePresenter = new BoundaryConditionSetNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(parentNode, boundaryCondition);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(parentNode, Is.Empty);
            Assert.That(gui.Selection, Is.Null);
        }

        [Test]
        public void RemoveNodeData_WithBoundaryConditionSetCollectionParentNodeNotContainingNodeData_ReturnsFalseAndDoesNotRemoveNodeDataAndGuiSelection()
        {
            // Setup
            var guiSelection = new object();
            var gui = Substitute.For<IGui>();
            gui.Selection = guiSelection;
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var unaffectedBoundaryCondition = new BoundaryConditionSet();
            var parentNode = new List<BoundaryConditionSet>
            {
                unaffectedBoundaryCondition
            };

            var nodePresenter = new BoundaryConditionSetNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(parentNode, new BoundaryConditionSet());

            // Assert
            Assert.That(result, Is.False);
            Assert.That(parentNode.Single(), Is.SameAs(unaffectedBoundaryCondition));
            Assert.That(gui.Selection, Is.SameAs(guiSelection));
        }

        [Test]
        public void RemoveNodeData_WithFmModelTreeShortCutParentNodeContainingNodeData_ReturnsTrueAndRemovesNodeDataAndResetsGuiSelection()
        {
            // Setup
            var gui = Substitute.For<IGui>();
            gui.Selection = new object();
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var boundaryCondition = new BoundaryConditionSet();
            var boundaryConditionSets = new List<BoundaryConditionSet>
            {
                boundaryCondition
            };
            var parentNode = new FmModelTreeShortcut(null, null, null, boundaryConditionSets);

            var nodePresenter = new BoundaryConditionSetNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(parentNode, boundaryCondition);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(boundaryConditionSets, Is.Empty);
            Assert.That(gui.Selection, Is.Null);
        }

        [Test]
        public void RemoveNodeData_WithFmModelTreeShortCutParentNodeNotContainingNodeData_ReturnsFalseAndDoesNotRemoveNodeDataAndGuiSelection()
        {
            // Setup
            var guiSelection = new object();
            var gui = Substitute.For<IGui>();
            gui.Selection = guiSelection;
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var unaffectedBoundaryCondition = new BoundaryConditionSet();
            var boundaryConditionSets = new List<BoundaryConditionSet>
            {
                unaffectedBoundaryCondition
            };
            var parentNode = new FmModelTreeShortcut(null, null, null, boundaryConditionSets);

            var nodePresenter = new BoundaryConditionSetNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(parentNode, new BoundaryConditionSet());

            // Assert
            Assert.That(result, Is.False);
            Assert.That(boundaryConditionSets.Single(), Is.SameAs(unaffectedBoundaryCondition));
            Assert.That(gui.Selection, Is.SameAs(guiSelection));
        }
    }
}
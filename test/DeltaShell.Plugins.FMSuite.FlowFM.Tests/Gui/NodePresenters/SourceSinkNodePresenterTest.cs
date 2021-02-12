using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.NodePresenters
{
    [TestFixture]
    public class SourceSinkNodePresenterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var nodePresenter = new SourceSinkNodePresenter();

            // Assert
            Assert.That(nodePresenter, Is.InstanceOf<FMSuiteNodePresenterBase<SourceAndSink>>());
        }

        [Test]
        public void CanRenameNode_Always_ReturnsTrue()
        {
            // Setup
            var nodePresenter = new SourceSinkNodePresenter();

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
            var sourceAndSink = new SourceAndSink
            {
                Feature = feature
            };

            var nodePresenter = new SourceSinkNodePresenter();

            // Call
            nodePresenter.OnNodeRenamed(sourceAndSink, newName);

            // Assert
            Assert.That(feature.Name, Is.EqualTo(newName));
        }

        [Test]
        public void UpdateNode_NodeDataWithFeature_UpdatesNode()
        {
            // Setup
            const string featureName = "Feature Name";
            var geometry = Substitute.For<IGeometry>();
            geometry.Coordinates.Returns(new Coordinate[0]);
            var feature = new Feature2D
            {
                Name = featureName,
                Geometry = geometry
            };
            var sourceAndSink = new SourceAndSink
            {
                Feature = feature
            };

            var node = Substitute.For<ITreeNode>();
            var nodePresenter = new SourceSinkNodePresenter();

            // Precondition
            Assert.That(node.Image, Is.Null);
            Assert.That(node.Text, Is.Empty);

            // Call
            nodePresenter.UpdateNode(null, node, sourceAndSink);

            // Assert
            Assert.That(node.Image, Is.Not.Null);
            Assert.That(node.Text, Is.EqualTo(featureName));
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

            var boundaryCondition = new SourceAndSink();

            var nodePresenter = new SourceSinkNodePresenter
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
        public void RemoveNodeData_WithSourceAndSinkCollectionParentNodeContainingNodeData_ReturnsTrueAndRemovesNodeDataAndResetsGuiSelection()
        {
            // Setup
            var gui = Substitute.For<IGui>();
            gui.Selection = new object();
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var sourceAndSink = new SourceAndSink();
            var parentNode = new List<SourceAndSink>
            {
                sourceAndSink
            };

            var nodePresenter = new SourceSinkNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(parentNode, sourceAndSink);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(parentNode, Is.Empty);
            Assert.That(gui.Selection, Is.Null);
        }

        [Test]
        public void RemoveNodeData_WithSourceAndSinkCollectionParentNodeNotContainingNodeData_ReturnsFalseAndDoesNotRemoveNodeDataAndGuiSelection()
        {
            // Setup
            var guiSelection = new object();
            var gui = Substitute.For<IGui>();
            gui.Selection = guiSelection;
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var unaffectedSourceAndSink = new SourceAndSink();
            var parentNode = new List<SourceAndSink>
            {
                unaffectedSourceAndSink
            };

            var nodePresenter = new SourceSinkNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(parentNode, new SourceAndSink());

            // Assert
            Assert.That(result, Is.False);
            Assert.That(parentNode.Single(), Is.SameAs(unaffectedSourceAndSink));
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

            var sourceAndSink = new SourceAndSink();
            var sourceAndSinks = new List<SourceAndSink>
            {
                sourceAndSink
            };
            var parentNode = new FmModelTreeShortcut(null, null, null, sourceAndSinks);

            var nodePresenter = new SourceSinkNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(parentNode, sourceAndSink);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(sourceAndSinks, Is.Empty);
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

            var unaffectedSourceAndSink = new SourceAndSink();
            var boundaryConditionSets = new List<SourceAndSink>
            {
                unaffectedSourceAndSink
            };
            var parentNode = new FmModelTreeShortcut(null, null, null, boundaryConditionSets);

            var nodePresenter = new SourceSinkNodePresenter
            {
                GuiPlugin = guiPlugin
            };

            // Call
            bool result = nodePresenter.RemoveNodeData(parentNode, new SourceAndSink());

            // Assert
            Assert.That(result, Is.False);
            Assert.That(boundaryConditionSets.Single(), Is.SameAs(unaffectedSourceAndSink));
            Assert.That(gui.Selection, Is.SameAs(guiSelection));
        }
    }
}
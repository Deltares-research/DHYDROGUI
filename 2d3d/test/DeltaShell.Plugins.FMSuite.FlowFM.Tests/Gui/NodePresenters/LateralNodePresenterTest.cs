using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.NodePresenters
{
    [TestFixture]
    public class LateralNodePresenterTest
    {
        [Test]
        public void CanRenameNode_ReturnsTrue()
        {
            // Setup
            var nodePresenter = new LateralNodePresenter();

            // Assert
            Assert.That(nodePresenter.CanRenameNode(null), Is.True);
        }

        [Test]
        public void OnNodeRenamed_RenamesLateral()
        {
            // Setup
            var nodePresenter = new LateralNodePresenter();
            var lateral = new Lateral { Feature = new Feature2D() };
            const string newName = "some_new_name";

            // Call
            nodePresenter.OnNodeRenamed(lateral, newName);

            // Assert
            Assert.That(lateral.Name, Is.EqualTo(newName));
        }

        [Test]
        public void UpdateNode_UpdatesNodeTextAndImage()
        {
            var nodePresenter = new LateralNodePresenter();
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();
            var nodeData = new Lateral
            {
                Feature = new Feature2D
                {
                    Name = "some_name",
                    Geometry = Substitute.For<IGeometry>()
                }
            };

            // Call
            nodePresenter.UpdateNode(parentNode, node, nodeData);

            // Assert
            Assert.That(node.Text, Is.EqualTo("some_name"));
            Assert.That(node.Image, Is.Not.Null);
        }

        [Test]
        public void RemoveNodeData_RemovedTheLateralFeatureFromTheModel()
        {
            // Setup
            LateralNodePresenter nodePresenter = CreateNodePresenterWithGui();

            var nodeData = new Lateral
            {
                Feature = new Feature2D
                {
                    Name = "some_name",
                    Geometry = Substitute.For<IGeometry>()
                }
            };
            var model = new WaterFlowFMModel();
            model.Laterals.Add(nodeData);

            Assert.That(model.LateralFeatures, Does.Contain(nodeData.Feature));

            FmModelTreeShortcut parentNodeData = CreatFmModelTreeShortcutWithModel(model);

            // Call
            nodePresenter.RemoveNodeData(parentNodeData, nodeData);

            // Assert
            Assert.That(model.Laterals, Is.Empty);
            Assert.That(model.LateralFeatures, Is.Empty);
        }

        private static FmModelTreeShortcut CreatFmModelTreeShortcutWithModel(WaterFlowFMModel model)
        {
            return new FmModelTreeShortcut(string.Empty, new Bitmap(1, 1), model, null);
        }

        private static LateralNodePresenter CreateNodePresenterWithGui()
        {
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui.Returns(Substitute.For<IGui>());
            var nodePresenter = new LateralNodePresenter { GuiPlugin = guiPlugin };

            return nodePresenter;
        }
    }
}
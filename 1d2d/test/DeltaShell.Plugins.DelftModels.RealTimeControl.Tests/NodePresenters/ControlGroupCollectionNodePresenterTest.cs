using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.NodePresenters
{
    [TestFixture]
    public class ControlGroupCollectionNodePresenterTest
    {
        [Test]
        public void UpdateNode_AssignsNameTagImageToNode()
        {
            // arrange
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();
            var controlGroup = new ControlGroup();
            var list = new EventedList<ControlGroup> {controlGroup};

            var controlGroupCollectionNodePresenter = new ControlGroupCollectionNodePresenter();

            // Act
            controlGroupCollectionNodePresenter.UpdateNode(parentNode, node, list);

            // Assert
            node.Received(1).Text = "Control Groups";
            node.Received(1).Tag = list;
            node.Received(1).Image = Arg.Any<Bitmap>();
        }

        [Test]
        public void UpdateNode_GetChildNodeObjectsReturnsSameList()
        {
            // arrange
            var node = Substitute.For<ITreeNode>();
            var controlGroup = new ControlGroup();
            var list = new EventedList<ControlGroup> {controlGroup};

            var controlGroupCollectionNodePresenter = new ControlGroupCollectionNodePresenter();

            IEnumerable Call() => controlGroupCollectionNodePresenter.GetChildNodeObjects(list, node);

            // Assert
            CollectionAssert.AreEqual(list, Call());
        }

        [Test]
        public void UpdateNode_GetContextMenu()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var node = Substitute.For<ITreeNode>();
            node.Parent.Tag.Returns(realTimeControlModel);

            var controlGroupCollectionNodePresenter = new ControlGroupCollectionNodePresenter();
            var guiPluginMock = Substitute.For<GuiPlugin>();
            controlGroupCollectionNodePresenter.GuiPlugin = guiPluginMock;
            var nodeData = new object();

            controlGroupCollectionNodePresenter.GetContextMenu(node, nodeData);

            guiPluginMock.Received(1).GetContextMenu(realTimeControlModel, nodeData);
        }
    }
}
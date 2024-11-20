using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.HydroRegionTreeView.NodePresenters
{
    [TestFixture]
    public class NetworkRouteTreeViewNodePresenterTest
    {
        private GuiPlugin pluginGui;
        private Route route;
        private NetworkRouteTreeViewNodePresenter nodePresenter;

        [SetUp]
        public void Setup()
        {
            pluginGui = Substitute.For<GuiPlugin>();
            pluginGui.Gui = Substitute.For<IGui>();
            nodePresenter = new NetworkRouteTreeViewNodePresenter(pluginGui);
            route = new Route();
        }

        [Test]
        public void GivenNodes_WhenUpdateNode_ThenReturnExpectedText()
        {
            //Arrange
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();
            const string expectedText = "route (empty)";

            //Act
            nodePresenter.UpdateNode(parentNode, node, route);

            //Assert
            Assert.That(node.Text, Is.EqualTo(expectedText));
        }

        [Test]
        public void GivenTreeNode_WhenCanRenameNode_ThenReturnTrue()
        {
            //Arrange, Act & Assert
            Assert.That(nodePresenter.CanRenameNode(Substitute.For<ITreeNode>()), Is.True);
        }

        [Test]
        public void GivenNewName_WhenOnNodeRenamed_ThenRouteHasNewName()
        {
            //Arrange
            route.Name = "oldName";
            const string newName = "newName";

            //Act
            nodePresenter.OnNodeRenamed(route, newName);

            //Assert
            Assert.That(route.Name, Is.EqualTo(newName));
        }

        [Test]
        public void GivenNoGuiPlugin_WhenGetContextMenu_ThenReturnNull()
        {
            //Arrange
            var nodePresenterWithoutGuiPlugin = new NetworkRouteTreeViewNodePresenter(null);

            //Act & Assert
            Assert.That(nodePresenterWithoutGuiPlugin.GetContextMenu(Substitute.For<ITreeNode>(), route), Is.Null);
        }

        [Test]
        public void GivenTreeNode_WhenGetContextMenu_ThenExpectPluginGuiGetContextMenuCalled()
        {
            //Arrange
            var treeNode = Substitute.For<ITreeNode>();

            //Act
            _ = nodePresenter.GetContextMenu(treeNode, route);

            //Assert
            pluginGui.Received(1).GetContextMenu(treeNode, route);
        }

        [Test]
        public void GivenRoute_WhenCanRemove_ThenReturnTrue()
        {
            //Arrange, Act & Assert
            Assert.That(nodePresenter.CanRemove(route, route), Is.True);
        }

        [Test]
        public void GivenNetworkWithRoute_WhenRemoveNodeData_ThenRouteIsRemovedFromNetwork()
        {
            //Arrange
            var network = new HydroNetwork();
            network.Routes.Add(route);

            Assert.That(network.Routes.Contains(route), Is.True);

            //Act
            bool removeNodeData = nodePresenter.RemoveNodeData(route, route);

            //Assert
            Assert.That(removeNodeData, Is.True);
            Assert.That(network.Routes.Contains(route), Is.False);
        }
    }
}
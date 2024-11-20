using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FMClassMapFileFunctionStoreNodePresenterTest
    {
        [Test]
        public void GivenATreeNodeAndAFMClassMapFileFunctionStoreWithASetPath_WhenUpdateNodeIsCalled_ThenCorrectTextIsSetOnNodeAndImageIsNotNull()
        {
            // Given
            var mocks = new MockRepository();
            var treeView = mocks.StrictMock<ITreeView>();
            const string path = "some_file_path";
            var nodeData = Substitute.For<IFMClassMapFileFunctionStore>();
            nodeData.Path.Returns(path);
            var treeNode = new TreeNode(treeView);
            var nodePresenter = new FMClassMapFileFunctionStoreNodePresenter();
            Assert.IsNull(treeNode.Image);

            // When
            nodePresenter.UpdateNode(null, treeNode, nodeData);

            // Then
            Assert.AreEqual(path, treeNode.Text);
            Assert.NotNull(treeNode.Image);
        }

        [Test]
        public void GivenAClassMapFileFunctionStoreWithAFunctionAndAGrid_WhenGetChildNodeObjectsIsCalled_ThenCorrectDataItemsAreYielded()
        {
            // Given
            var store = Substitute.For<IFMClassMapFileFunctionStore>();
            store.Functions = new EventedList<IFunction>(new List<IFunction>());
            
            const string coverageName = "coverage_name";
            var function = new UnstructuredGridCellCoverage(new UnstructuredGrid(), true) {Name = coverageName};
            store.Functions.Add(function);
            var guiPlugin = Substitute.For<GuiPlugin>();
            var nodePresenter = new FMClassMapFileFunctionStoreNodePresenter {GuiPlugin = guiPlugin};

            // When
            List<DataItem> childNodes = nodePresenter.GetChildNodeObjects(store, null).OfType<DataItem>().ToList();

            // Then
            Assert.AreEqual(2, childNodes.Count);
            Assert.IsNotNull(childNodes.FirstOrDefault(c => c.Tag == coverageName));
            Assert.IsNotNull(childNodes.FirstOrDefault(c => c.Tag == "grid"));
        }
    }
}
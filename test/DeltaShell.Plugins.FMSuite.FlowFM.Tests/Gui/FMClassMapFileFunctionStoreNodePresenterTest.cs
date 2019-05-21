using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FunctionStores;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;
using System.Linq;

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
            var nodeData = new FMClassMapFileFunctionStore(path);
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
            var mocks = new MockRepository();       
            var store = mocks.Stub<FMClassMapFileFunctionStore>(string.Empty);       
            const string coverageName = "coverage_name";
            var function = new UnstructuredGridCellCoverage(new UnstructuredGrid(), true) {Name = coverageName };
            store.Functions.Add(function);
            var guiPlugin = mocks.Stub<GuiPlugin>();
            var nodePresenter = new FMClassMapFileFunctionStoreNodePresenter { GuiPlugin = guiPlugin };

            // When
            var childNodes = nodePresenter.GetChildNodeObjects(store, null).OfType<DataItem>().ToList();

            // Then
            Assert.AreEqual(2, childNodes.Count);
            Assert.IsNotNull(childNodes.FirstOrDefault(c=>c.Tag == coverageName));
            Assert.IsNotNull(childNodes.FirstOrDefault(c => c.Tag == "grid"));
        }
    }
}

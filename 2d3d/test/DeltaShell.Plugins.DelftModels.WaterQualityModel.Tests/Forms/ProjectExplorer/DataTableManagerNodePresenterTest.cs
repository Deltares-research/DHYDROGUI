using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms.ProjectExplorer
{
    [TestFixture]
    public class DataTableManagerNodePresenterTest
    {
        [Test]
        public void DefaultConstructor_ExpectedValues()
        {
            // setup

            // call
            var presenter = new DataTableManagerNodePresenter();

            // assert
            Assert.IsInstanceOf<TreeViewNodePresenterBase<DataTableManager>>(presenter);
            Assert.AreEqual(typeof(DataTableManager), presenter.NodeTagType);
            Assert.IsNull(presenter.TreeView);
        }

        [Test]
        public void UpdateNode_InitializesTreeNode()
        {
            // setup
            var treeNode = new TreeNode(null);
            object oldTag = treeNode.Tag;
            Assert.IsNull(treeNode.Image, "Test precondition: Constructed treeNode has null for Image property.");

            var manager = new DataTableManager {Name = "Test"};

            var presenter = new DataTableManagerNodePresenter();

            // call
            presenter.UpdateNode(null, treeNode, manager);

            // assert
            Assert.AreEqual(oldTag, treeNode.Tag,
                            "Should not set Tag as DataTableManager is exposed in DataItem for NodePresenters.");
            Assert.AreSame(manager.Name, treeNode.Text);
            Assert.IsNotNull(treeNode.Image);
        }

        [Test]
        public void GetChildNodeObjects_Always_ReturnEmptyCollection()
        {
            // setup
            var presenter = new DataTableManagerNodePresenter();

            // call
            object[] childNodes = presenter.GetChildNodeObjects(null, null).OfType<object>().ToArray();

            // assert
            Assert.IsEmpty(childNodes);
        }

        [Test]
        public void CanRenameNode_Always_ReturnFalse()
        {
            // setup
            var presenter = new DataTableManagerNodePresenter();

            // call
            bool allowRenaming = presenter.CanRenameNode(null);

            // assert
            Assert.IsFalse(allowRenaming);
        }

        [Test]
        public void CanRenameNodeTo_Always_ReturnFalse()
        {
            // setup
            var presenter = new DataTableManagerNodePresenter();

            // call
            bool allowRenaming = presenter.CanRenameNodeTo(null, "new name");

            // assert
            Assert.IsFalse(allowRenaming);
        }

        [Test]
        public void OnNodeRenamed_Always_SetNameOfDataTableManager()
        {
            // setup
            var manager = new DataTableManager();
            var presenter = new DataTableManagerNodePresenter();

            // call
            presenter.OnNodeRenamed(manager, "new name");

            // assert
            Assert.AreEqual("new name", manager.Name);
        }

        [Test]
        public void CanDrop_Always_ReturnNone()
        {
            // setup
            var presenter = new DataTableManagerNodePresenter();

            // call
            DragOperations dragDropActions = presenter.CanDrop(null, null, null, DragOperations.All);

            // assert
            Assert.AreEqual(DragOperations.None, dragDropActions);
        }

        [Test]
        public void CanRemove_Always_ReturnFalse()
        {
            // setup
            var presenter = new DataTableManagerNodePresenter();

            // call
            bool allowRemove = presenter.CanRemove(null, null);

            // assert
            Assert.IsFalse(allowRemove);
        }

        [Test]
        public void RemoveNodeData_Always_ReturnFalse()
        {
            // setup
            var presenter = new DataTableManagerNodePresenter();

            // call
            bool removeSuccessful = presenter.RemoveNodeData(null, null);

            // assert
            Assert.IsFalse(removeSuccessful);
        }

        [Test]
        public void CanDrag_Always_ReturnNone()
        {
            // setup
            var presenter = new DataTableManagerNodePresenter();

            // call
            DragOperations dragOperations = presenter.CanDrag(null);

            // assert
            Assert.AreEqual(DragOperations.None, dragOperations);
        }
    }
}
using System;
using System.IO;
using System.Linq;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FMFileBasedTreeViewTest
    {
        [Test]
        public void CheckNodeConstructor()
        {
            var parentDirectory = Environment.CurrentDirectory;
            const string parentFile = "parentFile.txt";
            var fullParentPath = Path.Combine(parentDirectory, parentFile);
            var parentNode = new FileBasedModelItem("parent", fullParentPath);

            Assert.AreEqual(fullParentPath, parentNode.FilePath);
            Assert.AreEqual(parentDirectory, parentNode.Directory);
            Assert.AreEqual(parentFile, parentNode.FileName);
            Assert.IsFalse(parentNode.FileExists);
            Assert.IsFalse(parentNode.DirectChildren.Any());
            Assert.IsNull(parentNode.Parent);
        }

        [Test]
        public void CheckChildNodeConstruction()
        {
            var parentDirectory = Environment.CurrentDirectory;
            const string parentFile = "parentFile.txt";
            var fullParentPath = Path.Combine(parentDirectory, parentFile);
            var parentNode = new FileBasedModelItem("parent", fullParentPath);

            const string subDirectory = "childDirectory";
            const string childFile = "childFile.txt";
            var relativePath = Path.Combine(subDirectory, childFile);

            var childItem = parentNode.AddChildItem("child", relativePath);

            Assert.IsNotNull(childItem);
            Assert.AreEqual(childItem, parentNode.DirectChildren.FirstOrDefault());
            Assert.AreEqual(Path.Combine(parentDirectory, subDirectory), childItem.Directory);
            Assert.AreEqual(childFile, childItem.FileName);
            Assert.IsFalse(childItem.FileExists);
            Assert.AreEqual(parentNode, childItem.Parent);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowToyModelTreeView()
        {
            var treeView = new TreeView();
            treeView.NodePresenters.Add(new WaterFlowFMFileBasedItemNodePresenter());
            var node = new FileBasedModelItem("parent", "parentFile.txt");
            node.AddChildItem("childFile1.txt", "child1");
            node.AddChildItem("childFile2.txt", "child2");
            var thirdChild = node.AddChildItem("childFile3.txt", "child3");
            thirdChild.AddChildItem("grandChildFile", "grandChild");
            treeView.Data = node;
            WindowsFormsTestHelper.ShowModal(treeView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowHarlingenFileStructure()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);
            var view = new WaterFlowFMFileStructureView {Data = model};
            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}

using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui.NodePresenters
{
    [TestFixture]
    public class FMSuiteNodePresenterBaseTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var presenter = new TestFMSuiteNodePresenterBase();

            // Assert
            Assert.That(presenter, Is.InstanceOf<TreeViewNodePresenterBaseForPluginGui<object>>());
        }

        [Test]
        public void UpdateNode_WithArguments_UpdatesNode()
        {
            // Setup
            const string expectedNodeText = "ExpectedNodeTest";
            Bitmap expectedNodeImage = Resources.add;

            var treeNode = Substitute.For<ITreeNode>();
            var presenter = new TestFMSuiteNodePresenterBase
            {
                NodeText = expectedNodeText,
                NodeImage = expectedNodeImage
            };

            var nodeData = new object();

            // Precondition
            Assert.That(treeNode.Text, Is.Empty);
            Assert.That(treeNode.Image, Is.Null);

            // Call
            presenter.UpdateNode(null, treeNode, nodeData);

            // Assert
            Assert.That(treeNode.Text, Is.EqualTo(expectedNodeText));
            Assert.That(treeNode.Image, Is.SameAs(expectedNodeImage));
        }

        [Test]
        public void UpdateNode_WithArguments_CallsFunctionWithExpectedArguments()
        {
            // Setup
            var treeNode = Substitute.For<ITreeNode>();
            var presenter = new TestFMSuiteNodePresenterBase
            {
                NodeText = "Some text",
                NodeImage = Resources.add
            };

            var nodeData = new object();

            // Precondition
            Assert.That(treeNode.Text, Is.Empty);
            Assert.That(treeNode.Image, Is.Null);

            // Call
            presenter.UpdateNode(null, treeNode, nodeData);

            // Assert
            Assert.That(presenter.IsDataEmptyCalled, Is.True);
            Assert.That(presenter.IsGetNodeImageCalled, Is.True);
            Assert.That(presenter.IsGetNodeTextCalled, Is.True);

            Assert.That(presenter.GetNodeImageArgument, Is.SameAs(nodeData));
            Assert.That(presenter.GetNodeTextArgument, Is.SameAs(nodeData));
        }

        [Test]
        public void ResetGuiSelection_Always_ResetsGUISelection()
        {
            // Setup
            var gui = Substitute.For<IGui>();
            gui.Selection = new object();
            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var presenter = new TestFMSuiteNodePresenterBase
            {
                GuiPlugin = guiPlugin
            };

            // Call
            presenter.PublicResetGuiSelection();

            // Assert
            Assert.That(gui.Selection, Is.Null);
        }

        private class TestFMSuiteNodePresenterBase : FMSuiteNodePresenterBase<object>
        {
            public object GetNodeTextArgument { get; private set; }
            public object GetNodeImageArgument { get; private set; }
            public bool IsGetNodeTextCalled { get; private set; }
            public bool IsGetNodeImageCalled { get; private set; }
            public bool IsDataEmptyCalled { get; private set; }

            public string NodeText { set; private get; }
            public Image NodeImage { set; private get; }

            public void PublicResetGuiSelection()
            {
                ResetGuiSelection();
            }

            protected override string GetNodeText(object data)
            {
                GetNodeTextArgument = data;
                IsGetNodeTextCalled = true;

                return NodeText;
            }

            protected override bool IsDataEmpty()
            {
                IsDataEmptyCalled = true;
                return base.IsDataEmpty();
            }

            protected override Image GetNodeImage(object data)
            {
                IsGetNodeImageCalled = true;
                GetNodeImageArgument = data;
                return NodeImage;
            }
        }
    }
}
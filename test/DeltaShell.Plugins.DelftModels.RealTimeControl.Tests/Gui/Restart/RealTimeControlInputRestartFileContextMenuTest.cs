using System;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Gui.Restart
{
    [TestFixture]
    public class RealTimeControlInputRestartFileContextMenuTest
    {
        [Test]
        public void Constructor_RealTimeControlRestartFileNull_ThrowsArgumentNullException()
        {
            // Setup
            var node = Substitute.For<ITreeNode>();

            // Call
            void Call() => new RealTimeControlInputRestartFileContextMenu(null, node);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("restartFile"));
        }

        [Test]
        public void Constructor_NodeNull_ThrowsArgumentNullException()
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile();

            // Call
            void Call() => new RealTimeControlInputRestartFileContextMenu(restartFile, null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("node"));
        }

        [Test]
        public void Constructor_NodeDoesNotBelongToModel_InitializesInstanceCorrectly()
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile();
            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Returns((ITreeNode) null);

            // Call
            var menu = new RealTimeControlInputRestartFileContextMenu(restartFile, node);

            // Assert
            Assert.That(menu.ContextMenuStrip.Items, Is.Empty);
        }

        [Test]
        public void Constructor_InputRealTimeControlRestartFile_InitializesInstanceCorrectly()
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile();
            var node = Substitute.For<ITreeNode>();
            var model = new RealTimeControlModel();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartInput = restartFile;

            // Call
            var menu = new RealTimeControlInputRestartFileContextMenu(restartFile, node);

            // Assert
            ToolStripItemCollection toolStripItems = menu.ContextMenuStrip.Items;
            Assert.That(toolStripItems, Has.Count.EqualTo(3));
            Assert.That(toolStripItems[0].Text, Is.EqualTo("Remove restart"));
            Assert.That(toolStripItems[1].Text, Is.EqualTo("Use last restart"));
            Assert.That(toolStripItems[2], Is.InstanceOf<ToolStripSeparator>());
        }

        [Test]
        public void GivenRealTimeControlModel_WhenRestartInputEmpty_ThenRemoveRestartOptionDisabled()
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile();
            var node = Substitute.For<ITreeNode>();
            var model = new RealTimeControlModel();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartInput = restartFile;

            // Call
            var menu = new RealTimeControlInputRestartFileContextMenu(restartFile, node);

            // Assert
            ToolStripItem removeRestartItem = menu.ContextMenuStrip.Items[0];
            Assert.That(removeRestartItem.Text, Is.EqualTo("Remove restart"));
            Assert.That(removeRestartItem.Enabled, Is.False);
        }

        [Test]
        public void GivenRealTimeControlModel_WhenRestartInputNotEmpty_ThenRemoveRestartOptionEnabled()
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile("filename", "content");
            var node = Substitute.For<ITreeNode>();
            var model = new RealTimeControlModel();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartInput = restartFile;

            // Call
            var menu = new RealTimeControlInputRestartFileContextMenu(restartFile, node);

            // Assert
            ToolStripItem removeRestartItem = menu.ContextMenuStrip.Items[0];
            Assert.That(removeRestartItem.Text, Is.EqualTo("Remove restart"));
            Assert.That(removeRestartItem.Enabled, Is.True);
        }
    }
}
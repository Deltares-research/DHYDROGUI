using System;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.IO.RestartFiles;
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
        public void ClickingRemoveRestartToolStripItem_RestartInputIsEmptied()
        {
            // Setup
            var node = Substitute.For<ITreeNode>();
            var model = Substitute.For<IRealTimeControlModel>();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartInput = new RealTimeControlRestartFile("restart.file", "file content");

            var menu = new RealTimeControlInputRestartFileContextMenu(model.RestartInput, node);

            ToolStripItem removeRestartItem = menu.ContextMenuStrip.Items[0];

            // Preconditions
            Assert.That(removeRestartItem.Text, Is.EqualTo("Remove restart"));
            Assert.That(model.RestartInput.IsEmpty, Is.False);

            // Call
            removeRestartItem.PerformClick();

            // Assert
            Assert.That(model.RestartInput.IsEmpty, Is.True);
            model.Received().MarkOutputOutOfSync();
        }

        [Test]
        public void ClickingUseLastRestart_RestartInputIsSetWithLastOutputRestart()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                var node = Substitute.For<ITreeNode>();
                var model = Substitute.For<IRealTimeControlModel>();
                node.Parent.Parent.Tag.Returns(model);
                model.RestartInput = new RealTimeControlRestartFile();
                model.RestartOutput.Returns(new EventedList<RestartFile>(new[]
                {
                    new RestartFile(temp.CreateFile("restart_a.file", "content a")),
                    new RestartFile(temp.CreateFile("restart_b.file", "content b")),
                    new RestartFile(temp.CreateFile("restart_c.file", "content c")),
                }));

                var menu = new RealTimeControlInputRestartFileContextMenu(model.RestartInput, node);

                ToolStripItem useLastRestart = menu.ContextMenuStrip.Items[1];

                // Preconditions
                Assert.That(useLastRestart.Text, Is.EqualTo("Use last restart"));
                Assert.That(model.RestartInput.IsEmpty, Is.True);

                // Call
                useLastRestart.PerformClick();

                // Assert
                Assert.That(model.RestartInput.IsEmpty, Is.False);
                Assert.That(model.RestartInput.Name, Is.EqualTo("restart_c.file"));
                Assert.That(model.RestartInput.Content, Is.EqualTo("content c"));
                model.Received().MarkOutputOutOfSync();
            }
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
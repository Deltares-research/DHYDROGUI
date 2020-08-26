using System;
using System.Collections.Generic;
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
    public class RealTimeControlRestartFileContextMenuTest
    {
        [Test]
        public void Constructor_RealTimeControlRestartFileNull_ThrowsArgumentNullException()
        {
            // Setup
            var node = Substitute.For<ITreeNode>();

            // Call
            void Call() => new RealTimeControlRestartFileContextMenu(null, node);

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
            void Call() => new RealTimeControlRestartFileContextMenu(restartFile, null);

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
            var menu = new RealTimeControlRestartFileContextMenu(restartFile, node);

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
            var menu = new RealTimeControlRestartFileContextMenu(restartFile, node);

            // Assert
            ToolStripItemCollection toolStripItems = menu.ContextMenuStrip.Items;
            Assert.That(toolStripItems, Has.Count.EqualTo(3));
            Assert.That(toolStripItems[0].Text, Is.EqualTo("Remove restart"));
            Assert.That(toolStripItems[1].Text, Is.EqualTo("Use last restart"));
            Assert.That(toolStripItems[2], Is.InstanceOf<ToolStripSeparator>());
        }

        [Test]
        public void Constructor_OutputRealTimeControlRestartFile_InitializesInstanceCorrectly()
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile();
            var node = Substitute.For<ITreeNode>();
            var model = new RealTimeControlModel();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartOutput = new EventedList<RealTimeControlRestartFile> {restartFile};

            // Call
            var menu = new RealTimeControlRestartFileContextMenu(restartFile, node);

            // Assert
            ToolStripItemCollection toolStripItems = menu.ContextMenuStrip.Items;
            Assert.That(toolStripItems, Has.Count.EqualTo(1));
            Assert.That(toolStripItems[0].Text, Is.EqualTo("Use as restart"));
        }
    }
}
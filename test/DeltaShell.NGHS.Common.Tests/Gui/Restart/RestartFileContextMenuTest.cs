using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.NGHS.Common.Gui.Restart;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Gui.Restart
{
    [TestFixture]
    public class RestartFileContextMenuTest
    {
        [Test]
        public void Constructor_RestartFileNull_ThrowsArgumentNullException()
        {
            // Setup
            var node = Substitute.For<ITreeNode>();

            // Call
            void Call() => new RestartFileContextMenu(null, node);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("restartFile"));
        }

        [Test]
        public void Constructor_NodeNull_ThrowsArgumentNullException()
        {
            // Setup
            var restartFile = new RestartFile();

            // Call
            void Call() => new RestartFileContextMenu(restartFile, null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("node"));
        }

        [Test]
        public void Constructor_NodeDoesNotBelongToModel_InitializesInstanceCorrectly()
        {
            // Setup
            var restartFile = new RestartFile();
            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Returns((ITreeNode) null);

            // Call
            var menu = new RestartFileContextMenu(restartFile, node);

            // Assert
            Assert.That(menu.ContextMenuStrip.Items, Is.Empty);
        }

        [Test]
        public void Constructor_InputRestartFile_InitializesInstanceCorrectly()
        {
            // Setup
            var restartFile = new RestartFile();
            var node = Substitute.For<ITreeNode>();
            var model = Substitute.For<IRestartModel>();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartInput.Returns(restartFile);

            // Call
            var menu = new RestartFileContextMenu(restartFile, node);

            // Assert
            ToolStripItemCollection toolStripItems = menu.ContextMenuStrip.Items;
            Assert.That(toolStripItems, Has.Count.EqualTo(3));
            Assert.That(toolStripItems[0].Text, Is.EqualTo("Remove restart"));
            Assert.That(toolStripItems[1].Text, Is.EqualTo("Use last restart"));
            Assert.That(toolStripItems[2], Is.InstanceOf<ToolStripSeparator>());
        }

        [Test]
        public void Constructor_OutputRestartFile_InitializesInstanceCorrectly()
        {
            // Setup
            var restartFile = new RestartFile();
            var node = Substitute.For<ITreeNode>();
            var model = Substitute.For<IRestartModel>();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartOutput.Returns(new List<RestartFile> {restartFile});

            // Call
            var menu = new RestartFileContextMenu(restartFile, node);

            // Assert
            ToolStripItemCollection toolStripItems = menu.ContextMenuStrip.Items;
            Assert.That(toolStripItems, Has.Count.EqualTo(1));
            Assert.That(toolStripItems[0].Text, Is.EqualTo("Use as restart"));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
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
            var model = Substitute.For<ITimeDependentRestartModel>();
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
            var model = Substitute.For<ITimeDependentRestartModel>();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartOutput.Returns(new List<RestartFile> {restartFile});

            // Call
            var menu = new RestartFileContextMenu(restartFile, node);

            // Assert
            ToolStripItemCollection toolStripItems = menu.ContextMenuStrip.Items;
            Assert.That(toolStripItems, Has.Count.EqualTo(1));
            Assert.That(toolStripItems[0].Text, Is.EqualTo("Use as restart"));
        }

        [Test]
        public void ClickingRemoveRestartToolStripItem_RestartInputIsEmptied()
        {
            // Setup
            var model = Substitute.For<ITimeDependentRestartModel>();
            model.UseRestart.Returns(true);
            model.RestartInput = new RestartFile("path/to/restart.file");

            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Tag.Returns(model);

            var menu = new RestartFileContextMenu(model.RestartInput, node);

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
            var model = Substitute.For<ITimeDependentRestartModel>();
            model.RestartInput = new RestartFile();
            model.RestartOutput.Returns(new List<RestartFile>
            {
                new RestartFile("path/to/restart1.file"),
                new RestartFile("path/to/restart2.file"),
                new RestartFile("path/to/restart3.file"),
            });

            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Tag.Returns(model);

            var menu = new RestartFileContextMenu(model.RestartInput, node);

            ToolStripItem useLastRestart = menu.ContextMenuStrip.Items[1];

            // Preconditions
            Assert.That(useLastRestart.Text, Is.EqualTo("Use last restart"));

            // Call
            useLastRestart.PerformClick();

            // Assert
            Assert.That(model.RestartInput.IsEmpty, Is.False);
            Assert.That(model.RestartInput, Is.Not.SameAs(model.RestartOutput.Last()));
            Assert.That(model.RestartInput.Path, Is.EqualTo(model.RestartOutput.Last().Path));

            model.Received().MarkOutputOutOfSync();
        }

        [Test]
        public void ClickingUseAsRestart_RestartInputIsSetWithSelectedOutputRestart()
        {
            // Setup
            var model = Substitute.For<ITimeDependentRestartModel>();
            var restartOutputFile = new RestartFile("path/to/restart.file");
            model.RestartOutput.Returns(new List<RestartFile> {restartOutputFile});

            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Tag.Returns(model);

            var menu = new RestartFileContextMenu(restartOutputFile, node);

            ToolStripItem useAsRestart = menu.ContextMenuStrip.Items[0];

            // Preconditions
            Assert.That(useAsRestart.Text, Is.EqualTo("Use as restart"));

            // Call
            useAsRestart.PerformClick();

            // Assert
            Assert.That(model.RestartInput.IsEmpty, Is.False);
            Assert.That(model.RestartInput, Is.Not.SameAs(restartOutputFile));
            Assert.That(model.RestartInput.Path, Is.EqualTo(restartOutputFile.Path));

            model.Received().MarkOutputOutOfSync();
        }
    }
}
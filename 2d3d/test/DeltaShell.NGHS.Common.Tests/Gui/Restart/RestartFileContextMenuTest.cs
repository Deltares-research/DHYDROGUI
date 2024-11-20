using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Common.Gui.Restart;
using DeltaShell.NGHS.Common.Restart;
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
            void Call() => new RestartFileContextMenu<RestartFileStub>(null, node);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("restartFile"));
        }

        [Test]
        public void Constructor_NodeNull_ThrowsArgumentNullException()
        {
            // Setup
            var restartFile = new RestartFileStub();

            // Call
            void Call() => new RestartFileContextMenu<RestartFileStub>(restartFile, null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("node"));
        }

        [Test]
        public void Constructor_NodeDoesNotBelongToModel_InitializesInstanceCorrectly()
        {
            // Setup
            var restartFile = new RestartFileStub();
            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Returns((ITreeNode) null);

            // Call
            var menu = new RestartFileContextMenu<RestartFileStub>(restartFile, node);

            // Assert
            Assert.That(menu.ContextMenuStrip.Items, Is.Empty);
        }

        [Test]
        public void Constructor_InputRestartFile_InitializesInstanceCorrectly()
        {
            // Setup
            var restartFile = new RestartFileStub();
            var node = Substitute.For<ITreeNode>();
            var model = Substitute.For<IRestartModel<RestartFileStub>>();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartInput.Returns(restartFile);

            // Call
            var menu = new RestartFileContextMenu<RestartFileStub>(restartFile, node);

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
            var restartFile = new RestartFileStub();
            var node = Substitute.For<ITreeNode>();
            var model = Substitute.For<IRestartModel<RestartFileStub>>();
            node.Parent.Parent.Tag.Returns(model);
            model.RestartOutput.Returns(new List<RestartFileStub> {restartFile});

            // Call
            var menu = new RestartFileContextMenu<RestartFileStub>(restartFile, node);

            // Assert
            ToolStripItemCollection toolStripItems = menu.ContextMenuStrip.Items;
            Assert.That(toolStripItems, Has.Count.EqualTo(1));
            Assert.That(toolStripItems[0].Text, Is.EqualTo("Use as restart"));
        }

        [Test]
        public void ClickingRemoveRestartToolStripItem_RestartInputIsEmptied()
        {
            // Setup
            var model = Substitute.For<IRestartModel<RestartFileStub>, ITimeDependentModel>();
            model.UseRestart.Returns(true);
            model.RestartInput = new RestartFileStub("restart.file");

            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Tag.Returns(model);

            var menu = new RestartFileContextMenu<RestartFileStub>(model.RestartInput, node);

            ToolStripItem removeRestartItem = menu.ContextMenuStrip.Items[0];

            // Preconditions
            Assert.That(removeRestartItem.Text, Is.EqualTo("Remove restart"));
            Assert.That(model.RestartInput.IsEmpty, Is.False);

            // Call
            removeRestartItem.PerformClick();

            // Assert
            Assert.That(model.RestartInput.IsEmpty, Is.True);
            ((ITimeDependentModel)model).Received(1).MarkOutputOutOfSync();
        }

        [Test]
        public void ClickingUseLastRestart_RestartInputIsSetWithLastOutputRestart()
        {
            // Setup
            var model = Substitute.For<IRestartModel<RestartFileStub>, ITimeDependentModel>();
            model.RestartInput = new RestartFileStub();
            var restartFiles = new List<RestartFileStub>
            {
                new RestartFileStub("restart1.file"),
                new RestartFileStub("restart2.file"),
                new RestartFileStub("restart3.file"),
            };
            model.RestartOutput.Returns(restartFiles);

            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Tag.Returns(model);

            var menu = new RestartFileContextMenu<RestartFileStub>(model.RestartInput, node);

            ToolStripItem useLastRestart = menu.ContextMenuStrip.Items[1];

            // Preconditions
            Assert.That(useLastRestart.Text, Is.EqualTo("Use last restart"));

            // Call
            useLastRestart.PerformClick();

            // Assert
            model.Received(1).SetRestartInputToDuplicateOf(restartFiles.LastOrDefault());
            ((ITimeDependentModel)model).Received(1).MarkOutputOutOfSync();
        }

        [Test]
        public void ClickingUseAsRestart_RestartInputIsSetWithSelectedOutputRestart()
        {
            // Setup
            var model = Substitute.For<IRestartModel<RestartFileStub>, ITimeDependentModel>();
            RestartFileStub restartOutputFile = new RestartFileStub("restart.file");
            model.RestartOutput.Returns(new List<RestartFileStub> {restartOutputFile});

            var node = Substitute.For<ITreeNode>();
            node.Parent.Parent.Tag.Returns(model);

            var menu = new RestartFileContextMenu<RestartFileStub>(restartOutputFile, node);

            ToolStripItem useAsRestart = menu.ContextMenuStrip.Items[0];

            // Preconditions
            Assert.That(useAsRestart.Text, Is.EqualTo("Use as restart"));

            // Call
            useAsRestart.PerformClick();

            // Assert
            model.Received(1).SetRestartInputToDuplicateOf(restartOutputFile);
            ((ITimeDependentModel)model).Received(1).MarkOutputOutOfSync();
        }

        [Test]
        public void RestartFileContextMenu_WhenRestartInputEmpty_RemoveRestartItemIsDisabled()
        {
            // Setup
            var restartFile = new RestartFileStub();
            var node = Substitute.For<ITreeNode>();
            var model = Substitute.For<IRestartModel<RestartFileStub>, ITimeDependentModel>();
            model.RestartInput = restartFile;
            node.Parent.Parent.Tag.Returns(model);

            // Call
            var menu = new RestartFileContextMenu<RestartFileStub>(restartFile, node);

            // Assert
            ToolStripItem removeRestartItem = menu.ContextMenuStrip.Items[0];
            Assert.That(removeRestartItem.Text, Is.EqualTo("Remove restart"));
            Assert.That(removeRestartItem.Enabled, Is.False);
        }

        [Test]
        public void RestartFileContextMenu_WhenRestartInputNotEmpty_RemoveRestartOptionIsEnabled()
        {
            // Setup
            var restartFile = new RestartFileStub( "file.ext" );
            var node = Substitute.For<ITreeNode>();
            var model = Substitute.For<IRestartModel<RestartFileStub>, ITimeDependentModel>();
            model.UseRestart.Returns(!restartFile.IsEmpty);
            model.RestartInput = restartFile;
            node.Parent.Parent.Tag.Returns(model);
            Assert.That(!restartFile.IsEmpty);

            // Call
            var menu = new RestartFileContextMenu<RestartFileStub>(restartFile, node);

            // Assert
            ToolStripItem removeRestartItem = menu.ContextMenuStrip.Items[0];
            Assert.That(removeRestartItem.Text, Is.EqualTo("Remove restart"));
            Assert.That(removeRestartItem.Enabled, Is.True);
        }
    }
}
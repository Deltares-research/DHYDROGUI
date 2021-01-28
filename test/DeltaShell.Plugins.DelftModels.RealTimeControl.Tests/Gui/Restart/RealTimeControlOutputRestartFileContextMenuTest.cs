using System;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Gui.Restart
{
    [TestFixture]
    public class RealTimeControlOutputRestartFileContextMenuTest
    {
        [Test]
        public void Constructor_RealTimeControlRestartFileNull_ThrowsArgumentNullException()
        {
            // Setup
            var node = Substitute.For<ITreeNode>();

            // Call
            void Call() => new RealTimeControlOutputRestartFileContextMenu(null, node);

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
            void Call() => new RealTimeControlOutputRestartFileContextMenu(restartFile, null);

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
            var menu = new RealTimeControlOutputRestartFileContextMenu(restartFile, node);

            // Assert
            Assert.That(menu.ContextMenuStrip.Items, Is.Empty);
        }

        [Test]
        public void ClickingUseAsRestart_RestartInputIsSetWithSelectedOutputRestart()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var model = Substitute.For<IRealTimeControlModel>();
                var restartOutputFile = new RestartFile(temp.CreateFile("restart.file", "content restart file"));
                model.RestartOutput.Returns(new EventedList<RestartFile>(new []{restartOutputFile}));

                var node = Substitute.For<ITreeNode>();
                node.Parent.Parent.Tag.Returns(model);

                var menu = new RealTimeControlOutputRestartFileContextMenu(restartOutputFile, node);

                ToolStripItem useAsRestart = menu.ContextMenuStrip.Items[0];

                // Preconditions
                Assert.That(useAsRestart.Text, Is.EqualTo("Use as restart"));

                // Call
                useAsRestart.PerformClick();

                // Assert
                Assert.That(model.RestartInput.IsEmpty, Is.False);
                Assert.That(model.RestartInput, Is.Not.SameAs(restartOutputFile));
                Assert.That(model.RestartInput.Name, Is.EqualTo(restartOutputFile.Name));
                Assert.That(model.RestartInput.Content, Is.EqualTo("content restart file"));
                
                model.Received().MarkOutputOutOfSync();
            }
        }
    }
}
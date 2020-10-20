using System;
using DelftTools.Controls;
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
    }
}
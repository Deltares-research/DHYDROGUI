using System;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Gui.Restart
{
    [TestFixture]
    public class RealTimeControlOutputRestartFileNodePresenterTest
    {
        [Test]
        public void Constructor_InitializesInstance()
        {
            // Setup
            var guiPlugin = Substitute.For<GuiPlugin>();

            // Call
            var nodePresenter = new RealTimeControlOutputRestartFileNodePresenter(guiPlugin);

            // Assert
            Assert.That(nodePresenter.GuiPlugin, Is.SameAs(guiPlugin));
        }

        [Test]
        public void Constructor_GuiPluginNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RealTimeControlOutputRestartFileNodePresenter(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("guiPlugin"));
        }

        [Test]
        public void UpdateNode_ForEmptyRestartFile()
        {
            // Setup
            var nodePresenter = Create.For<RealTimeControlOutputRestartFileNodePresenter>();
            var node = Substitute.For<ITreeNode>();
            var nodeData = new RestartFile();

            // Call
            nodePresenter.UpdateNode(null, node, nodeData);

            // Assert
            Assert.That(node.Text, Is.EqualTo("Restart: empty"));
        }

        [Test]
        public void UpdateNode_ForNotEmptyRestartFile()
        {
            // Setup
            var nodePresenter = Create.For<RealTimeControlOutputRestartFileNodePresenter>();
            var node = Substitute.For<ITreeNode>();
            var nodeData = new RestartFile("path/to/restart.file");

            // Call
            nodePresenter.UpdateNode(null, node, nodeData);

            // Assert
            Assert.That(node.Text, Is.EqualTo("restart.file"));
        }

        [Test]
        public void UpdateNode_NodeNull_ThrowsArgumentNullException()
        {
            // Setup
            var nodePresenter = Create.For<RealTimeControlOutputRestartFileNodePresenter>();
            var nodeData = new RestartFile();

            // Call
            void Call() => nodePresenter.UpdateNode(null, null, nodeData);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("node"));
        }

        [Test]
        public void UpdateNode_NodeDataNull_ThrowsArgumentNullException()
        {
            // Setup
            var nodePresenter = Create.For<RealTimeControlOutputRestartFileNodePresenter>();
            var node = Substitute.For<ITreeNode>();

            // Call
            void Call() => nodePresenter.UpdateNode(null, node, null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("nodeData"));
        }
    }
}
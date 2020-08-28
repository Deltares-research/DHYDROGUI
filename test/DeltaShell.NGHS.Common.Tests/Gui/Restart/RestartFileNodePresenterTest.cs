using System;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.Restart;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Gui.Restart
{
    [TestFixture]
    public class RestartFileNodePresenterTest
    {
        [Test]
        public void Constructor_InitializesInstance()
        {
            // Setup
            var guiPlugin = Substitute.For<GuiPlugin>();

            // Call
            var nodePresenter = new RestartFileNodePresenter(guiPlugin);

            // Assert
            Assert.That(nodePresenter.GuiPlugin, Is.SameAs(guiPlugin));
        }

        [Test]
        public void Constructor_GuiPluginNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RestartFileNodePresenter(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("guiPlugin"));
        }

        [Test]
        public void UpdateNode_ForEmptyRestartFile()
        {
            // Setup
            var nodePresenter = Create.For<RestartFileNodePresenter>();
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
            var nodePresenter = Create.For<RestartFileNodePresenter>();
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
            var nodePresenter = Create.For<RestartFileNodePresenter>();
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
            var nodePresenter = Create.For<RestartFileNodePresenter>();
            var node = Substitute.For<ITreeNode>();

            // Call
            void Call() => nodePresenter.UpdateNode(null, node, null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("nodeData"));
        }
    }
}
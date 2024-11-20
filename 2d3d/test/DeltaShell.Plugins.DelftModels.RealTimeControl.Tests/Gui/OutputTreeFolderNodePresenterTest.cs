using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Gui
{
    [TestFixture]
    public class OutputTreeFolderNodePresenterTest
    {
        [Test]
        public void UpdateNode_DataNull_ThrowsArgumentNullException()
        {
            // Arrange
            var nodePresenter = new OutputTreeFolderNodePresenter();

            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();
            
            // Act
            void Call() => nodePresenter.UpdateNode(parentNode, node, null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("nodeData"));
        }

        [Test]
        public void UpdateNode_NodeNull_ThrowsArgumentNullException()
        {
            // Arrange
            var nodePresenter = new OutputTreeFolderNodePresenter();

            var parentNode = Substitute.For<ITreeNode>();
            var model = Substitute.For<IModel>();
            var outputTreeFolder = new OutputTreeFolder(model, Enumerable.Empty<object>(), "test");

            // Act
            void Call() => nodePresenter.UpdateNode(parentNode, null, outputTreeFolder);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("node"));
        }

        [Test]
        public void UpdateNode_ShouldSetCorrectValuesForTextTagAndImageProperties()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var outputTreeFolder = new OutputTreeFolder(model,Enumerable.Empty<object>(), "test");

            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();

            var nodePresenter = new OutputTreeFolderNodePresenter();

            // Act
            nodePresenter.UpdateNode(parentNode, node, outputTreeFolder);

            // Assert
            Assert.AreEqual("test", node.Text);
            Assert.AreSame(outputTreeFolder, node.Tag);
            node.Received(1).Image = Arg.Any<Bitmap>();
        }

        [Test]
        public void GetChildNodeObjects_ShouldSetCorrectValuesForTextTagAndImageProperties()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var list = Substitute.For<IEnumerable<string>>();
            var outputTreeFolder = new OutputTreeFolder(model, list, "test");

            var node = Substitute.For<ITreeNode>();

            var nodePresenter = new OutputTreeFolderNodePresenter();

            // Act
            IEnumerable retrievedChildren = nodePresenter.GetChildNodeObjects(outputTreeFolder, node);

            // Assert
            Assert.AreSame(list, retrievedChildren);
        }
    }
}
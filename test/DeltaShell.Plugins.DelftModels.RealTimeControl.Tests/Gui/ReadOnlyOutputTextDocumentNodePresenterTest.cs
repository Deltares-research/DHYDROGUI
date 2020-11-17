using System;
using System.Drawing;
using DelftTools.Controls;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Gui
{
    [TestFixture]
    public class ReadOnlyOutputTextDocumentNodePresenterTest
    {
        [Test]
        [TestCase("testname.xml")]
        [TestCase("testname.csv")]
        [TestCase("testname.test")]
        [TestCase("testname")]
        [TestCase("")]
        public void UpdateNode_ShouldSetCorrectValuesForTextTagAndImageProperties(string fileName)
        {
            // Arrange
            var document = new ReadOnlyOutputTextDocument(fileName, "testcontent");

            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();

            var nodePresenter = new ReadOnlyOutputTextDocumentNodePresenter();

            // Act
            nodePresenter.UpdateNode(parentNode, node, document);

            // Assert
            Assert.AreEqual(fileName, node.Text);
            Assert.AreSame(document, node.Tag);
            node.Received(1).Image = Arg.Any<Bitmap>();
        }

        [Test]
        public void UpdateNode_NodeDataNull_ThrowsArgumentNullException()
        {
            // Arrange
            var nodePresenter = new ReadOnlyOutputTextDocumentNodePresenter();

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
            var nodePresenter = new ReadOnlyOutputTextDocumentNodePresenter();

            var parentNode = Substitute.For<ITreeNode>();
            var document = new ReadOnlyOutputTextDocument("test", "testcontent");

            // Act
            void Call() => nodePresenter.UpdateNode(parentNode, null, document);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("node"));
        }
    }
}
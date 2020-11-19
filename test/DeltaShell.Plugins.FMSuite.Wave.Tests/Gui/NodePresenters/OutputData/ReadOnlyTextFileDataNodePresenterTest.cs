using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.NodePresenters.OutputData
{
    [TestFixture]
    public class ReadOnlyTextFileDataNodePresenterTest
    {
        [Test]
        public void UpdateNode_ExpectedResults()
        {
            // Setup
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();

            const string documentName = "someDocumentName.txt";
            const string content = "Cooooooontent!";

            var textData = new ReadOnlyTextFileData(documentName, content);

            var nodePresenter = new ReadOnlyTextFileDataNodePresenter();

            // Call
            nodePresenter.UpdateNode(parentNode, node, textData);

            // Assert
            Assert.That(node.Text, Is.EqualTo(documentName));
            Assert.That(node.Image, Is.Not.Null);
        }
    }
}
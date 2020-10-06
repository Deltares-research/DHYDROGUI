using System.Collections;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.NodePresenters.OutputData
{
    [TestFixture]
    public class WaveOutputDataNodePresenterTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var nodePresenter = new WaveOutputDataNodePresenter();

            // Assert
            Assert.That(nodePresenter, Is.InstanceOf<TreeViewNodePresenterBase<IWaveOutputData>>());
        }

        [Test]
        public void UpdateNode_SetsCorrectNodeTextAndImage()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();

            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            // Call
            nodePresenter.UpdateNode(parentNode, node, nodeData);

            // Assert
            Assert.That(node.Text, Is.EqualTo("Output"));
            Assert.That(node.Image, Is.Not.Null);
        }

        [Test]
        public void GetChildNodeObjects_ReturnsWaveOutputDataChildren()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            // Call
            IEnumerable result = nodePresenter.GetChildNodeObjects(nodeData, node);

            // Assert
            // TODO: this should be updated once more child objects are added
            Assert.That(result, Is.Empty);
        }
    }
}
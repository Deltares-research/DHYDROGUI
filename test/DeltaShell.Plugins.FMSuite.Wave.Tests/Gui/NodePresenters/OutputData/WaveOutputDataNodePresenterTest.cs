using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Shell.Gui.Swf;
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
        public void GetChildNodeObjects_ReturnsWaveOutputDataDiagnosticFiles()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();


            var readOnlyData = new List<ReadOnlyTextFileData>
            {
                new ReadOnlyTextFileData("1.txt", "1"),
                new ReadOnlyTextFileData("2.txt", "2"),

            };

            nodeData.DiagnosticFiles.Returns(readOnlyData);

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>().ToList();

            // Assert
            int expectedNElements = readOnlyData.Count;
            Assert.That(result.Count, Is.EqualTo(expectedNElements));

            foreach (ReadOnlyTextFileData readOnlyTextFileData in readOnlyData)
            {
                Assert.That(result, Has.Member(readOnlyTextFileData));
            }
        }

        [Test]
        public void GetChildNodeObjects_SpectraFilesEmpty_ReturnsNoSpectraOutputFolder()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            nodeData.SpectraFiles.Returns(new List<ReadOnlyTextFileData>());

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>().ToList();

            // Assert
            object outputFolder = result.FirstOrDefault(x => x is TreeFolder tf && tf.Text == "Spectra");
            Assert.That(outputFolder, Is.Null);
        }

        [Test]
        public void GetChildNodeObjects_SpectraFilesNotEmpty_ReturnsNoSpectraOutputFolder()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            var spectraFiles = new[]
            {
                new ReadOnlyTextFileData("Wave.sp1", "Spooky spooky"),
                new ReadOnlyTextFileData("Wave.sp2", "skeletons"),
            };
            nodeData.SpectraFiles.Returns(spectraFiles);

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>().ToList();

            // Assert
            var outputFolder = result.FirstOrDefault(x => x is TreeFolder tf && tf.Text == "Spectra") as TreeFolder;
            Assert.That(outputFolder, Is.Not.Null);

            IEnumerable<ReadOnlyTextFileData> children = outputFolder.ChildItems.Cast<ReadOnlyTextFileData>();
            Assert.That(children, Is.EquivalentTo(spectraFiles));
        }
    }
}
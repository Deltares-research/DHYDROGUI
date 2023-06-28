using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Functions;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.TextData;
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

            var readOnlyData = new EventedList<ReadOnlyTextFileData>
            {
                new ReadOnlyTextFileData("1.txt", "1", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("2.txt", "2", ReadOnlyTextFileDataType.Default)
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

            nodeData.SpectraFiles.Returns(new EventedList<ReadOnlyTextFileData>());

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

            var spectraFiles = new EventedList<ReadOnlyTextFileData>
            {
                new ReadOnlyTextFileData("Wave.sp1", "Spooky spooky", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("Wave.sp2", "skeletons", ReadOnlyTextFileDataType.Default)
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
        
        [Test]
        public void GetChildNodeObjects_SwanFilesEmpty_ReturnsNoSwanOutputFolder()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            nodeData.SwanFiles.Returns(new EventedList<ReadOnlyTextFileData>());

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>()
                                               .ToList();

            // Assert
            object outputFolder = result.FirstOrDefault(x => x is TreeFolder tf && tf.Text == "SWAN input files");
            Assert.That(outputFolder, Is.Null);
        }

        [Test]
        public void GetChildNodeObjects_SwanFilesNotEmpty_ReturnsSwanOutputFolder()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            var swanFiles = new EventedList<ReadOnlyTextFileData>
            {
                new ReadOnlyTextFileData("INPUT_1_20060105_000000", "PROJECT 1", ReadOnlyTextFileDataType.Default),
                new ReadOnlyTextFileData("INPUT_1_20060105_000000", "PROJECT 2", ReadOnlyTextFileDataType.Default)
            };
            nodeData.SwanFiles.Returns(swanFiles);

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>()
                                               .ToList();

            // Assert
            var outputFolder = result.FirstOrDefault(x => x is TreeFolder tf && tf.Text == "SWAN input files") as TreeFolder;
            Assert.That(outputFolder, Is.Not.Null);

            IEnumerable<ReadOnlyTextFileData> children = outputFolder.ChildItems.Cast<ReadOnlyTextFileData>();
            Assert.That(children, Is.EquivalentTo(swanFiles));
        }

        [Test]
        public void GetChildNodeObjects_WavmFileFunctionStoresEmpty_ReturnsNoMapFilesOutputFolder()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            nodeData.WavmFileFunctionStores.Returns(new EventedList<IWavmFileFunctionStore>());

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>().ToList();

            // Assert
            object outputFolder = result.FirstOrDefault(x => x is TreeFolder tf && tf.Text == "Map Files");
            Assert.That(outputFolder, Is.Null);
        }

        [Test]
        public void GetChildNodeObjects_WavmFileFunctionStoresNotEmpty_ReturnsMapFilesOutputFolder()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            var functionStore = Substitute.For<IWavmFileFunctionStore>();
            functionStore.Functions = new EventedList<IFunction>(new[]
            {
                Substitute.For<IFunction>()
            });
            var wavmFileFunctionStores = new EventedList<IWavmFileFunctionStore>
            {
                functionStore,
                functionStore,
                functionStore
            };

            nodeData.WavmFileFunctionStores.Returns(wavmFileFunctionStores);

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>().ToList();

            // Assert
            var outputFolder = result.FirstOrDefault(x => x is TreeFolder tf && tf.Text == "Map Files") as TreeFolder;
            Assert.That(outputFolder, Is.Not.Null);

            IEnumerable<IWavmFileFunctionStore> children = outputFolder.ChildItems.Cast<IWavmFileFunctionStore>();
            Assert.That(children, Is.EquivalentTo(wavmFileFunctionStores));
        }

        [Test]
        public void GetChildNodeObjects_WavhFileFunctionStoresEmpty_ReturnsNoHisFilesOutputFolder()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            nodeData.WavhFileFunctionStores.Returns(new EventedList<IWavhFileFunctionStore>());

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>().ToList();

            // Assert
            object outputFolder = result.FirstOrDefault(x => x is TreeFolder tf && tf.Text == "His Files");
            Assert.That(outputFolder, Is.Null);
        }

        [Test]
        public void GetChildNodeObjects_WavhFileFunctionStoresNotEmpty_ReturnsHisFilesOutputFolder()
        {
            // Setup
            var nodePresenter = new WaveOutputDataNodePresenter();
            var node = Substitute.For<ITreeNode>();
            var nodeData = Substitute.For<IWaveOutputData>();

            var functionStore = Substitute.For<IWavhFileFunctionStore>();
            functionStore.Functions = new EventedList<IFunction>(new[]
            {
                Substitute.For<IFunction>()
            });
            var wavhFileFunctionStores = new EventedList<IWavhFileFunctionStore>
            {
                functionStore,
                functionStore,
                functionStore
            };
            nodeData.WavhFileFunctionStores.Returns(wavhFileFunctionStores);

            // Call
            List<object> result = nodePresenter.GetChildNodeObjects(nodeData, node)
                                               .Cast<object>().ToList();

            // Assert
            var outputFolder = result.FirstOrDefault(x => x is TreeFolder tf && tf.Text == "His Files") as TreeFolder;
            Assert.That(outputFolder, Is.Not.Null);

            IEnumerable<IWavhFileFunctionStore> children = outputFolder.ChildItems.Cast<IWavhFileFunctionStore>();
            Assert.That(children, Is.EquivalentTo(wavhFileFunctionStores));
        }
    }
}
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.NodePresenters.OutputData
{
    [TestFixture]
    public class WavhFileFunctionStoreNodePresenterTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var presenter = new WavhFileFunctionStoreNodePresenter();

            // Assert
            Assert.That(presenter, Is.InstanceOf<TreeViewNodePresenterBase<WavhFileFunctionStore>>());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateNode_ExpectedResults()
        {
            // Setup
            var presenter = new WavhFileFunctionStoreNodePresenter();
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();

            using (var tempDir = new TemporaryDirectory())
            {
                string ncPath = tempDir.CopyTestDataFileToTempDirectory("./WaveOutputDataHarvesterTest/wavh-Waves.nc");
                var functionStore = new WavhFileFunctionStore(ncPath);

                // Call
                presenter.UpdateNode(parentNode, node, functionStore);

                // Assert 
                node.Received(1).Text = "wavh-Waves.nc";
                node.Received(1).Image = Arg.Is<Bitmap>(x => x != null);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetChildNodeObjects_WavhFileFunctionStore_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaveModel>();

            var application = Substitute.For<IApplication>();
            application.GetAllModelsInProject().Returns(new[] { model });

            var gui = Substitute.For<IGui>();
            gui.Application = application;

            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var presenter = new WavhFileFunctionStoreNodePresenter
            {
                GuiPlugin = guiPlugin,
            };

            var node = Substitute.For<ITreeNode>();

            using (var tempDir = new TemporaryDirectory())
            {
                string ncPath = tempDir.CopyTestDataFileToTempDirectory("./WaveOutputDataHarvesterTest/wavh-Waves.nc");
                var functionStore = new WavhFileFunctionStore(ncPath);

                model.WaveOutputData.WavhFileFunctionStores.Returns(new EventedList<WavhFileFunctionStore>
                {
                    functionStore
                });

                // Call
                IList<object> result = presenter.GetChildNodeObjects(functionStore, node)
                                                .Cast<object>()
                                                .ToList();

                // Assert 
                // 27 functions in the functionStore.
                Assert.That(result.Count, Is.EqualTo(11)); 
            }
        }
    }
}
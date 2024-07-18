using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers.OutputData;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.NodePresenters.OutputData
{
    [TestFixture]
    public class WavmFileFunctionStoreNodePresenterTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var presenter = new WavmFileFunctionStoreNodePresenter();

            // Assert
            Assert.That(presenter, Is.InstanceOf<TreeViewNodePresenterBase<IWavmFileFunctionStore>>());
        }

        [Test]
        public void UpdateNode_ExpectedResults()
        {
            // Setup
            var presenter = new WavmFileFunctionStoreNodePresenter();
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();
            var functionStore = Substitute.For<IWavmFileFunctionStore>();
            functionStore.Path.Returns("wavm-Waves.nc");

            // Call
            presenter.UpdateNode(parentNode, node, functionStore);

            // Assert 
            node.Received(1).Text = "wavm-Waves.nc";
            node.Received(1).Image = Arg.Is<Bitmap>(x => x != null);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetChildNodeObjects_StandAloneWavmFileFunctionStore_ExpectedResults()
        {
            // Setup
            var application = Substitute.For<IApplication>();
            application.ProjectService.Project.Returns(new Project());

            var gui = Substitute.For<IGui>();
            gui.Application = application;

            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var presenter = new WavmFileFunctionStoreNodePresenter {GuiPlugin = guiPlugin};

            var node = Substitute.For<ITreeNode>();

            using (var tempDir = new TemporaryDirectory())
            {
                string ncPath = tempDir.CopyTestDataFileToTempDirectory("./WaveOutputDataHarvesterTest/wavm-Waves.nc");
                var functionStore = new WavmFileFunctionStore(ncPath);

                // Call
                IList<object> result = presenter.GetChildNodeObjects(functionStore, node)
                                                .Cast<object>()
                                                .ToList();

                // Assert 
                // 27 functions in the functionStore + grid.
                Assert.That(result.Count, Is.EqualTo(28));
            }
        }

        [Test]
        public void GetChildNodeObjects_WavmFileFunctionStoreInModel_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaveModel>();

            var application = Substitute.For<IApplication>();
            AddToProject(model, application.ProjectService);

            var gui = Substitute.For<IGui>();
            gui.Application = application;

            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var presenter = new WavmFileFunctionStoreNodePresenter {GuiPlugin = guiPlugin};
            var node = Substitute.For<ITreeNode>();

            var functionStore = new TestWavmFileFunctionStore
            {
                Functions = new EventedList<IFunction>(new[]
                {
                    Substitute.For<IFunction>(),
                    Substitute.For<IFunction>()
                })
            };
            
            model.WaveOutputData.WavmFileFunctionStores.Returns(new EventedList<IWavmFileFunctionStore> {functionStore});

            // Call
            IList<object> result = presenter.GetChildNodeObjects(functionStore, node)
                                            .Cast<object>()
                                            .ToList();

            // Assert 
            Assert.That(result.Count, Is.EqualTo(2));
        }
        
        private static void AddToProject(object obj, IProjectService projectService)
        {
            var project = new Project();
            project.RootFolder.Add(obj);
            projectService.Project.Returns(project);
        }
    }
}
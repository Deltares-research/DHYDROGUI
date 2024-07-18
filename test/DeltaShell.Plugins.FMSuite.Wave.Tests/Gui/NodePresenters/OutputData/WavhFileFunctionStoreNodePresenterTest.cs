using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers.Providers.OutputData;
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
            Assert.That(presenter, Is.InstanceOf<TreeViewNodePresenterBase<IWavhFileFunctionStore>>());
        }

        [Test]
        public void UpdateNode_ExpectedResults()
        {
            // Setup
            var presenter = new WavhFileFunctionStoreNodePresenter();
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();

            var functionStore = Substitute.For<IWavhFileFunctionStore>();
            functionStore.Path.Returns("wavh-Waves.nc");

            // Call
            presenter.UpdateNode(parentNode, node, functionStore);

            // Assert 
            node.Received(1).Text = "wavh-Waves.nc";
            node.Received(1).Image = Arg.Is<Bitmap>(x => x != null);
        }

        [Test]
        public void GetChildNodeObjects_WavhFileFunctionStore_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaveModel>();

            var application = Substitute.For<IApplication>();
            AddToProject(model, application.ProjectService);

            var gui = Substitute.For<IGui>();
            gui.Application = application;

            var guiPlugin = Substitute.For<GuiPlugin>();
            guiPlugin.Gui = gui;

            var presenter = new WavhFileFunctionStoreNodePresenter {GuiPlugin = guiPlugin};
            var node = Substitute.For<ITreeNode>();

            var functionStore = new TestWavhFileFunctionStore
            {
                Functions = new EventedList<IFunction>(new[]
                {
                    Substitute.For<IFunction>(),
                    Substitute.For<IFunction>()
                })
            };

            model.WaveOutputData.WavhFileFunctionStores.Returns(new EventedList<IWavhFileFunctionStore> {functionStore});

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

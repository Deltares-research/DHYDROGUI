using System;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.Wpf)]
    public class WaveModelNodePresenterTest
    {
        private static IGui CreateRunningGui()
        {
            IGui gui = new DHYDROGuiBuilder().WithWaves().Build();

            gui.Run();

            return gui;
        }

        [Test]
        public void ShowWaveProjectExplorer()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var model = new WaveModel(mdwPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    project.RootFolder.Add(model);
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        public void FmModelShouldBeReplacedWhenImportedInRootFolder()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            mdwPath = TestHelper.CreateLocalCopy(mdwPath);

            using (IGui gui = CreateRunningGui())
            {
                IApplication app = gui.Application;
                Project project = app.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    // Add water flow model to project
                    project.RootFolder.Add(new WaveModel());

                    // Check model name
                    WaveModel targetModel = project.RootFolder.Models.OfType<WaveModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Does.Contain("Waves"));

                    // Import new water flow model
                    WaveModelFileImporter importer = app.FileImporters.OfType<WaveModelFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mdwPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = project.RootFolder.Models.OfType<WaveModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Does.Contain("tst"));
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        public void FmModelShouldBeReplacedWhenImportedInFolder()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            mdwPath = TestHelper.CreateLocalCopy(mdwPath);

            using (IGui gui = CreateRunningGui())
            {
                IApplication app = gui.Application;
                Project project = app.ProjectService.CreateProject();
                
                Action mainWindowShown = delegate
                {
                    // Add new folder to project
                    project.RootFolder.Add(new Folder("Test Folder"));

                    // Check folder name
                    Folder testFolder = project.RootFolder.Folders.FirstOrDefault();
                    Assert.IsNotNull(testFolder);
                    Assert.That(testFolder.Name, Does.Contain("Test Folder"));

                    // Add new water flow model to the new folder and check its name
                    testFolder.Add(new WaveModel());
                    WaveModel targetModel =
                        testFolder.Models.OfType<WaveModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Does.Contain("Waves"));

                    // Import new water flow model
                    WaveModelFileImporter importer = app.FileImporters.OfType<WaveModelFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mdwPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = testFolder.Models.OfType<WaveModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Does.Contain("tst"));
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }
    }
}
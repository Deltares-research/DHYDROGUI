using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.Wpf)]
    public class WaveModelNodePresenterTest
    {
        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new WaveGuiPlugin(),
                new WaveApplicationPlugin()
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();

        }
        [Test]
        public void ShowWaveProjectExplorer()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var model = new WaveModel(mdwPath);
            
            using (var gui = CreateGui())
            {
                IApplication app = gui.Application;
                
                gui.Run();
                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    Project project = app.Project;
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

            using (var gui = CreateGui())
            {
                IApplication app = gui.Application;
                gui.Run();
                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    // Add water flow model to project
                    Project project = app.Project;
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

            using (var gui = CreateGui())
            {
                IApplication app = gui.Application;
                gui.Run();
                app.CreateNewProject();
                
                Action mainWindowShown = delegate
                {
                    // Add new folder to project
                    Project project = app.Project;
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
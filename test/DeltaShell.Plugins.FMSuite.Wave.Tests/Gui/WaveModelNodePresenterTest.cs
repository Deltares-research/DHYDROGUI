using System;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveModelNodePresenterTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWaveProjectExplorer()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var model = new WaveModel(mdwPath);

            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new WaveGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = app.Project;
                    project.RootFolder.Add(model);
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void FmModelShouldBeReplacedWhenImportedInRootFolder()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            mdwPath = TestHelper.CreateLocalCopy(mdwPath);

            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new WaveGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    // Add water flow model to project
                    Project project = app.Project;
                    project.RootFolder.Add(new WaveModel());

                    // Check model name
                    WaveModel targetModel = project.RootFolder.Models.OfType<WaveModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Is.StringContaining("Waves"));

                    // Import new water flow model
                    WaveModelFileImporter importer = app.FileImporters.OfType<WaveModelFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mdwPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = project.RootFolder.Models.OfType<WaveModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Is.StringContaining("tst"));
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void FmModelShouldBeReplacedWhenImportedInFolder()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            mdwPath = TestHelper.CreateLocalCopy(mdwPath);

            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new WaveGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    // Add new folder to project
                    Project project = app.Project;
                    project.RootFolder.Add(new Folder("Test Folder"));

                    // Check folder name
                    Folder testFolder = project.RootFolder.Folders.FirstOrDefault();
                    Assert.IsNotNull(testFolder);
                    Assert.That(testFolder.Name, Is.StringContaining("Test Folder"));

                    // Add new water flow model to the new folder and check its name
                    testFolder.Add(new WaveModel());
                    WaveModel targetModel =
                        testFolder.Models.OfType<WaveModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Is.StringContaining("Waves"));

                    // Import new water flow model
                    WaveModelFileImporter importer = app.FileImporters.OfType<WaveModelFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mdwPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = testFolder.Models.OfType<WaveModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Is.StringContaining("tst"));
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }
    }
}
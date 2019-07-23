using System;
using System.Windows;
using DelftTools.TestUtils;
using DelftTools.Utils.Editing;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveUndoRedoIntegrationTest
    {
        private WaveModel model;
        private Action mainWindowShown;
        private Action onMainWindowShown;
        private DeltaShellGui gui;
        private Window mainWindow;

        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());

            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new WaveApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaveGuiPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());

            gui.Run();

            var project = app.Project;

            // add data
            model = new WaveModel();
            project.RootFolder.Add(model);

            mainWindow = (Window)gui.MainWindow;

            // wait until gui starts
            mainWindowShown = () =>
            {
                gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));
                gui.UndoRedoManager.TrackChanges = true;
                onMainWindowShown();
            };
        }

        [TearDown]
        public void TearDown()
        {
            gui.UndoRedoManager.TrackChanges = false;
            gui.Dispose();
            onMainWindowShown = null;
            mainWindowShown = null;
            LogHelper.ResetLogging();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.UndoRedo)]
        [Category(TestCategory.WorkInProgress)]
        public void AddOrDeleteOuterDomainShouldNotDisconnectObjectTest()
        {
            onMainWindowShown = () =>
                {
                    var domain = model.OuterDomain;
                    var newDomain = new WaveDomainData("addedDomain");

                    model.BeginEdit(new DefaultEditAction("begin"));
                    model.OuterDomain = newDomain;
                    model.AddSubDomain(newDomain, domain);
                    model.EndEdit();

                    domain.SpectralDomainData.NDir = 10;

                    model.BeginEdit("Delete outer domain ...");
                    var newOuterDomain = model.OuterDomain.SubDomains[0];
                    model.OuterDomain.SubDomains.Clear();// disconnect
                    newOuterDomain.SuperDomain = null;
                    model.OuterDomain = newOuterDomain;
                    model.EndEdit();
                   
                    model.OuterDomain.SpectralDomainData.NDir = 20;

                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }
    }
}

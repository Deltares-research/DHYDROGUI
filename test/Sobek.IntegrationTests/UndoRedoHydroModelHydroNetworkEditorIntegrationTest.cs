using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.UndoRedo)]
    public class UndoRedoHydroModelHydroNetworkEditorIntegrationTest : UndoRedoHydroRegionTestBase
    {
        private HydroModel hydroModel;

        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());

            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new HydroModelGuiPlugin());

            
            gui.Run();

            project = app.Project;

            // add data
            var builder = new HydroModelBuilder();
            hydroModel = builder.BuildModel(ModelGroup.All);

            // remove anything but flow
            var activitiesToRemove = hydroModel.Activities.Where(a => !(a is WaterFlowModel1D)).ToList();
            foreach(var activity in activitiesToRemove)
            {
                hydroModel.Activities.Remove(activity);
            }

            project.RootFolder.Add(hydroModel);

            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            // wait until gui starts
            mainWindow.Loaded += delegate
                {
                    network = hydroModel.Region.SubRegions.OfType<HydroNetwork>().First();
                    basin = hydroModel.Region.SubRegions.OfType<DrainageBasin>().First();
                    gui.CommandHandler.OpenView(hydroModel, typeof(ProjectItemMapView));

                    ProjectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

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
            LogHelper.ResetLogging();
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void BranchSplit()
        {
            onMainWindowShown = () =>
            {
                var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                HydroNetworkHelper.SplitChannelAtNode(branch, new Coordinate(50, 0));

                AssertNetworkAsExpected("after split", 3, 2);
                AssertNumUndoableActions("after split", 2);

                gui.UndoRedoManager.Undo();

                AssertNetworkAsExpected("after undo", 2, 1);
                AssertNumUndoableActions("after undo", 1);

                gui.UndoRedoManager.Redo();

                AssertNetworkAsExpected("after redo", 3, 2);
                AssertNumUndoableActions("at end", 2);
            };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void AddSubRegion()
        {
            onMainWindowShown = () =>
                {
                    var emptyHydroModel = new HydroModel();

                    project.RootFolder.Add(emptyHydroModel);

                    emptyHydroModel.Region.SubRegions.Add(new DrainageBasin());

                    Assert.AreEqual(emptyHydroModel.Region.SubRegions.Count, 1);
                    AssertNumUndoableActions("after do", 2);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(emptyHydroModel.Region.SubRegions.Count, 0);
                    AssertNumUndoableActions("after undo", 1);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(emptyHydroModel.Region.SubRegions.Count, 1);
                    AssertNumUndoableActions("after undo", 2);
                };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void DeleteSubRegion()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var emptyHydroModel = new HydroModel();

                    project.RootFolder.Add(emptyHydroModel);

                    emptyHydroModel.Region.SubRegions.Add(new DrainageBasin());
                    
                    gui.UndoRedoManager.TrackChanges = true;

                    emptyHydroModel.Region.SubRegions.RemoveAt(0);

                    Assert.AreEqual(emptyHydroModel.Region.SubRegions.Count, 0);
                    AssertNumUndoableActions("after do", 1);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(emptyHydroModel.Region.SubRegions.Count, 1);
                    AssertNumUndoableActions("after undo", 0);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(emptyHydroModel.Region.SubRegions.Count, 0);
                    AssertNumUndoableActions("after undo", 1);
                };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void DeleteSubRegionFromExisting()
        {
            onMainWindowShown = () =>
                {
                    hydroModel.Region.SubRegions.RemoveAt(0);

                    Assert.AreEqual(hydroModel.Region.SubRegions.Count, 2);
                    AssertNumUndoableActions("after do", 1);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(hydroModel.Region.SubRegions.Count, 3);
                    AssertNumUndoableActions("after undo", 0);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(hydroModel.Region.SubRegions.Count, 2);
                    AssertNumUndoableActions("after redo", 1);
                };
            WpfTestHelper.ShowModal(mainWindow);
        }
    }
}
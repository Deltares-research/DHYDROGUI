using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{

    [TestFixture]
    public class WaveGuiPluginTest
    {
        [Test]
        public void AfterImportingModelOrGridFileMapShouldBeZoomedToExtents()
        {
            var mocks = new MockRepository();
            var app = mocks.Stub<IApplication>();
            var gui = mocks.Stub<IGui>();
            var mainWindow = mocks.Stub<IMainWindow>();
            var projectExplorer = mocks.Stub<IProjectExplorer>();
            var treeView = mocks.Stub<ITreeView>();
            var runner = mocks.Stub<IActivityRunner>();
            var mapView = mocks.Stub<MapView>();
            var map = mocks.Stub<Map>();

            gui.Application = app;
            mapView.Map = map;
            gui.Expect(g => g.MainWindow).Return(mainWindow).Repeat.Any();
            mainWindow.Expect(mw => mw.ProjectExplorer).Return(projectExplorer).Repeat.Any();
            projectExplorer.Expect(pe => pe.TreeView).Return(treeView).Repeat.Any();

            using (var gisPlugin = new SharpMapGisGuiPlugin())
            {
                using (var waveGuiPlugin = new WaveGuiPlugin())
                {
                    Func<MapView> myGetActiveMapViewFunc = () => mapView;
                    SetStaticField<WaveGuiPlugin>(waveGuiPlugin, "getActiveMapViewFunc", myGetActiveMapViewFunc);

                    var waveModel = new WaveModel();
                    
                    app.Expect(a => a.ActivityRunner).Return(runner).Repeat.Any();

                    var waveBoundaryFileImporter = new WaveBoundaryFileImporter();
                    var boundaryFileImportActivity = mocks.Stub<FileImportActivity>(waveBoundaryFileImporter, waveModel.OuterDomain);

                    var waveModelFileImporter = new WaveModelFileImporter();
                    var modelFileImportActivity = mocks.Stub<FileImportActivity>(waveModelFileImporter, waveModel.OuterDomain);

                    var waveGridFileImporter = new WaveGridFileImporter(waveGuiPlugin.Name, () => new[] { waveModel });
                    var gridFileImportActivity = mocks.Stub<FileImportActivity>(waveGridFileImporter, waveModel.OuterDomain);

                    mocks.ReplayAll();

                    waveGuiPlugin.Gui = gui;

                    // zoom-to-extents should not be called when importer is not gridfile importer or modelimporter
                    runner.Raise(r => r.ActivityStatusChanged += null, boundaryFileImportActivity, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Finished));
                    treeView.AssertWasNotCalled(tv => tv.Refresh());
                    map.AssertWasNotCalled(m => m.ZoomToExtents());

                    // gridfile importer
                    runner.Raise(r => r.ActivityStatusChanged += null, gridFileImportActivity, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Finished));
                    treeView.AssertWasNotCalled(tv => tv.Refresh());
                    map.AssertWasCalled(m => m.ZoomToExtents(), opt => opt.Repeat.Once());

                    // modelfile importer
                    runner.Raise(r => r.ActivityStatusChanged += null, modelFileImportActivity, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Finished));
                    treeView.AssertWasCalled(tv => tv.Refresh(), opt => opt.Repeat.Once());
                    map.AssertWasCalled(m => m.ZoomToExtents(), opt => opt.Repeat.Twice());

                    map.VerifyAllExpectations();
                }
            }
        }

        private void SetStaticField<T>(object obj, string fieldName, object value)
        {
            var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Instance
                                                          | BindingFlags.NonPublic | BindingFlags.Static
                                                          | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            if (fieldInfo == null)
            {
                throw new ArgumentOutOfRangeException("fieldName");
            }

            fieldInfo.SetValue(obj, value);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void DoubleClickingOutputItemProjectShouldEnableMapLayer()
        {
            var mdwPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"outputMapView\Waves.mdw"));

            using (var gui = new DeltaShellGui())
            {
                var guiPlugin = new WaveGuiPlugin();
                gui.Plugins.Add(new SharpMapGisGuiPlugin());               
                gui.Plugins.Add(guiPlugin);

                gui.Run();

                // reimport model 
                for (var i = 0; i < 2; i++)
                {
                    var model = new WaveModel(mdwPath);

                    gui.Application.Project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                    //open mapview
                    DoubleClickOutputItemAndAssertLayerIsOn(model, gui, guiPlugin, "hsign");

                    // close all views:
                    gui.DocumentViews.Clear();
                    Assert.AreEqual(0, gui.DocumentViews.Count);

                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    // try with already open view:
                    DoubleClickOutputItemAndAssertLayerIsOn(model, gui, guiPlugin, "depth");

                    gui.DocumentViews.Clear();

                    gui.CommandHandler.DeleteProjectItem(model);
                }
            }
        }

        private static void DoubleClickOutputItemAndAssertLayerIsOn(WaveModel model, IGui gui, GuiPlugin guiPlugin, string itemName)
        {
            var wavmFileFunctionStoreNodePresenter = new WavmFileFunctionStoreNodePresenter(){GuiPlugin = guiPlugin};
            var wavmFileFunctionStore = model.WavmFunctionStores.FirstOrDefault();
            Assert.NotNull(wavmFileFunctionStore);

            var outputItem =
                wavmFileFunctionStoreNodePresenter.GetChildNodeObjects(wavmFileFunctionStore, null)
                    .OfType<IDataItem>().FirstOrDefault(di => di.Name == itemName);
            Assert.NotNull(outputItem);
                        
            //double click
            gui.Selection = outputItem;
            gui.CommandHandler.OpenViewForSelection(typeof(ProjectItemMapView));
            Assert.AreEqual(1, gui.DocumentViews.Count);

            var activeMapView = WaveGuiPlugin.ActiveMapView;
            Assert.NotNull(activeMapView);

            var visibleLayerNames = new List<string>
            {
                "Boundaries",
                "Boundary Conditions",
                "Boundary from sp2",
                "Obstacles",
                "Observation Points",
                "Observation Cross-Sections",
                "Grid (Grid_001)",
                "Bathymetry (Outer)",
                itemName
            };

            var allLayers = activeMapView.Map.GetAllLayers(false);

            var coverageLayer = allLayers.FirstOrDefault(l => l.Name == itemName);
            Assert.IsNotNull(coverageLayer);

            var otherLayers = allLayers.Where(l => !visibleLayerNames.Contains(l.Name));
            Assert.NotNull(otherLayers);

            Assert.IsTrue(coverageLayer.Visible);
            Assert.IsFalse(otherLayers.Any(l=>l.Visible));
        }
    }
}

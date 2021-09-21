using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Layers;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WaterFlowFMMapIntegrationTest
    {
        private static string GetBendProfPath()
        {
            return TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void TestRunningSmallModelWithManyTimeSteps()
        {
            var mduPath = TestHelper.GetTestFilePath(@"smallModelWithManyTimeSteps\r01.mdu");

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                gui.MainWindow.Show();

                var model = new WaterFlowFMModel(mduPath);

                app.Project.RootFolder.Add(model);

                var sw = new Stopwatch();
                sw.Start();

                gui.Application.ActivityRunner.Enqueue(model);

                while (gui.Application.IsActivityRunningOrWaiting(model))
                {
                    Application.DoEvents();
                }

                sw.Stop();
                Assert.Less(sw.ElapsedMilliseconds, 30000);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCentralMapForFMModel()
        {
            var mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);

                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView)); //open central map
                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    gui.DocumentViews.Remove(gui.DocumentViews.ActiveView); // close view
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void OpenCloseCentralMapForFMModelCheckEventLeaks()
        {
            var mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);

                    // check subscribers
                    var eventsBefore = TestReferenceHelper.FindEventSubscriptions(model.Bathymetry, true);
                    
                    // open & close central map
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView)); //open central map
                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    gui.CommandHandler.RemoveAllViewsForItem(model); // close central map
                    Assert.IsNull(gui.DocumentViews.ActiveView);

                    // check event subscribers
                    var eventsAfter = TestReferenceHelper.FindEventSubscriptions(model.Bathymetry, true);

                    Assert.AreEqual(eventsBefore, eventsAfter, "#events bathymetry coverage");
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.WorkInProgress)] // fails on build server
        public void OpenCloseCentralMapForFMModelCheckLayerDoesNotLinger()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);
                    
                    // open central map
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView)); //open central map
                    
                    // get a weak reference to a layer
                    var layerRef = GetWeakLayerRef(FlowFMGuiPlugin.ActiveMapView);
                    
                    // close central map
                    gui.CommandHandler.RemoveAllViewsForItem(model); // close central map
                    
                    // make sure selection is not set to map
                    gui.Selection = null;
                    TypeUtils.CallPrivateMethod(gui.MainWindow.PropertyGrid, "GuiSelectionChanged",
                                                new[] {null, EventArgs.Empty});
                    
                    // Garbage collect and check layer is no longer in memory
                    GC.Collect(2, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();

                    Assert.IsFalse(layerRef.IsAlive, "not gc collected");
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        private static WeakReference GetWeakLayerRef(MapView mapview)
        {
            var layer = mapview.Map.GetAllVisibleLayers(false).OfType<UnstructuredBaseLayer>().First();
            return new WeakReference(layer);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void ReloadCentralMapAfterModelWithOutputSaved()
        {
            var mduPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"harlingen\har.mdu"));
            using (var gui = new DeltaShellGui())
            {
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Run();

                Action mainWindowShown = () =>
                {
                    var project = app.Project;
                    var model = new WaterFlowFMModel(mduPath);
                    project.RootFolder.Add(model);
                    gui.CommandHandler.OpenView(model, typeof (ProjectItemMapView));
                    var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    app.SaveProjectAs("test.dsproj");
                    gui.CommandHandler.OpenView(model, typeof (ValidationView));
                    gui.DocumentViews.ActiveView = mapView;

                    Assert.AreEqual(mapView, gui.DocumentViews.ActiveView);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.WorkInProgress)] // about 7 paths hold references the model after it is deleted
        public void ShowCentralMapForFMModelRemoveModelCheckMemoryLeaks()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                    {
                        var project = app.Project;

                        var weakRef = AddMduToProject(project, mduPath);

                        gui.CommandHandler.OpenView(project.RootFolder.Items[0], typeof (ProjectItemMapView));
                        gui.Selection = project.RootFolder.Items[0];
                        gui.CommandHandler.DeleteCurrentProjectItem();
                        gui.SelectedProjectItem = null;

                        Application.DoEvents(); // give invokes time to process
                        Thread.Sleep(500);
                        gui.MainWindow.ProjectExplorer.TreeView.WaitUntilAllEventsAreProcessed();
                        Application.DoEvents();

                        GC.Collect(2, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();

                        //MessageBox.Show("Done");

                        Assert.IsFalse(weakRef.IsAlive, "model removed from memory");
                    };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void ImportHarlingenShowVelocityOutput()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = () =>
                {
                    var project = app.Project;
                    var model = new WaterFlowFMModel(mduPath);
                    project.RootFolder.Add(model);
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));
                    var view = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    var outputGroupLayer =
                        view.MapView.Map.GetAllLayers(true).OfType<GroupLayer>().FirstOrDefault(l=>l.Name=="Output (map)");
                    Assert.IsNotNull(outputGroupLayer);
                    var flowLinkCoverageLayer =
                        outputGroupLayer.GetAllLayers(false).OfType<UnstructuredGridFlowLinkCoverageLayer>().First();
                    flowLinkCoverageLayer.Visible = true;
                    view.MapView.MapControl.Refresh();
                    gui.DocumentViews.Remove(gui.DocumentViews.ActiveView); // close view
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void ImportHarlingenRunShowVelocityOutput()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = () =>
                {
                    var project = app.Project;
                    var model = new WaterFlowFMModel(mduPath);
                    ActivityRunner.RunActivity(model);
                    project.RootFolder.Add(model);
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));
                    var view = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    var outputGroupLayer =
                        view.MapView.Map.GetAllLayers(true).OfType<GroupLayer>().FirstOrDefault(l => l.Name == "Output (map)");
                    Assert.IsNotNull(outputGroupLayer);
                    var flowLinkCoverageLayer =
                        outputGroupLayer.GetAllLayers(false).OfType<UnstructuredGridFlowLinkCoverageLayer>().First();
                    flowLinkCoverageLayer.Visible = true;
                    view.MapView.MapControl.Refresh();
                    gui.DocumentViews.Remove(gui.DocumentViews.ActiveView); // close view
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        private static WeakReference AddMduToProject(Project project, string mduPath)
        {
            var model = new WaterFlowFMModel(mduPath);
            var weakRef = new WeakReference(model);
            project.RootFolder.Add(model);
            return weakRef;
        }
    }
}
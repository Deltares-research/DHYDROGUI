using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.IntegrationTestUtils.Builders;
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

            using (var gui = CreateGui())
            {
                gui.Run();

                gui.MainWindow.Show();

                Project project = gui.Application.ProjectService.CreateProject();
                
                var model = new WaterFlowFMModel(mduPath);

                project.RootFolder.Add(model);

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

            using (var gui = CreateGuiMinimal())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
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

            using (var gui = CreateGuiMinimal())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
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
        [Category(TestCategory.Integration)]
        public void ReloadCentralMapAfterModelWithOutputSaved()
        {
            var mduPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"harlingen\har.mdu"));
            
            var pluginsToAdd = new List<IPlugin>()
            {
                new CommonToolsGuiPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new FlowFMApplicationPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                gui.Run();

                Action mainWindowShown = () =>
                {
                    var projectService = gui.Application.ProjectService;
                    Project project = projectService.CreateProject();
                    var model = new WaterFlowFMModel(mduPath);
                    project.RootFolder.Add(model);
                    gui.CommandHandler.OpenView(model, typeof (ProjectItemMapView));
                    var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    projectService.SaveProjectAs("test.dsproj");
                    gui.CommandHandler.OpenView(model, typeof (ValidationView));
                    gui.DocumentViews.ActiveView = mapView;

                    Assert.AreEqual(mapView, gui.DocumentViews.ActiveView);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }
        
        private static IGui CreateGuiMinimal()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                //apps : FM+Wave
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                
                //guis : FM+Wave
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
    }
}
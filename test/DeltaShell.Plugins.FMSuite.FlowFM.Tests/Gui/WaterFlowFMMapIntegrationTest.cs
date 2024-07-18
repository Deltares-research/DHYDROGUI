using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
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
    [TestFixture]
    public class WaterFlowFMMapIntegrationTest
    {
        private static IGui CreateRunningGui()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
                new ProjectExplorerGuiPlugin(),
            };
            IGui gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();

            gui.Run();

            return gui;
        }
        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.VerySlow)]
        public void TestRunningSmallModelWithManyTimeSteps()
        {
            string mduPath = TestHelper.GetTestFilePath(@"smallModelWithManyTimeSteps\r01.mdu");

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                gui.MainWindow.Show();

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduPath);

                project.RootFolder.Add(model);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                gui.Application.ActivityRunner.Enqueue(model);

                while (gui.Application.IsActivityRunningOrWaiting(model))
                {
                    Application.DoEvents();
                }

                stopwatch.Stop();
                Assert.Less(stopwatch.ElapsedMilliseconds, 53000);
                Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned), "The model run did not finish successfully.");
            }
        }

        [Test]
        [Category(NghsTestCategory.PerformanceDotTrace)]
        public void RunFMModelWithGUIVisible_ShouldBeWithinExecutionTime()
        {
            string mduPath = TestHelper.GetTestFilePath(@"smallModelWithManyTimeSteps\r01.mdu");

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                gui.MainWindow.Show();

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduPath);

                project.RootFolder.Add(model);

                TimerMethod_RunFMModelWithGUIVisible(gui, model);

                Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned), "The model run did not finish successfully.");
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void ShowCentralMapForFMModel()
        {
            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    project.RootFolder.Add(model);

                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView)); //open central map
                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    gui.DocumentViews.Remove(gui.DocumentViews.ActiveView); // close view
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void OpenCloseCentralMapForFMModelCheckEventLeaks()
        {
            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    project.RootFolder.Add(model);

                    // check subscribers
                    int eventsBefore = TestReferenceHelper.FindEventSubscriptions(model.SpatialData.Bathymetry, true);

                    // open & close central map
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView)); //open central map
                    Assert.IsNotNull(gui.DocumentViews.ActiveView);
                    gui.CommandHandler.RemoveAllViewsForItem(model); // close central map
                    Assert.IsNull(gui.DocumentViews.ActiveView);

                    // check event subscribers
                    int eventsAfter = TestReferenceHelper.FindEventSubscriptions(model.SpatialData.Bathymetry, true);

                    Assert.AreEqual(eventsBefore, eventsAfter, "#events bathymetry coverage");
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.Slow)]
        public void LoadHarlingenShowVelocityOutput()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = () =>
                {
                    var model = new WaterFlowFMModel();
                    model.LoadFromMdu(mduPath);

                    project.RootFolder.Add(model);
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));
                    ProjectItemMapView view = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    GroupLayer outputGroupLayer =
                        view.MapView.Map.GetAllLayers(true).OfType<GroupLayer>().FirstOrDefault(l => l.Name == "Output (map)");
                    Assert.IsNotNull(outputGroupLayer);
                    UnstructuredGridFlowLinkCoverageLayer flowLinkCoverageLayer =
                        outputGroupLayer.GetAllLayers(false).OfType<UnstructuredGridFlowLinkCoverageLayer>().First();
                    flowLinkCoverageLayer.Visible = true;
                    view.MapView.MapControl.Refresh();
                    gui.DocumentViews.Remove(gui.DocumentViews.ActiveView); // close view
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.Slow)]
        public void ImportHarlingenRunShowVelocityOutput()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = () =>
                {
                    var model = new WaterFlowFMModel();
                    model.ImportFromMdu(mduPath);

                    ActivityRunner.RunActivity(model);
                    project.RootFolder.Add(model);
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));
                    ProjectItemMapView view = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    GroupLayer outputGroupLayer =
                        view.MapView.Map.GetAllLayers(true).OfType<GroupLayer>().FirstOrDefault(l => l.Name == "Output (map)");
                    Assert.IsNotNull(outputGroupLayer);
                    UnstructuredGridFlowLinkCoverageLayer flowLinkCoverageLayer =
                        outputGroupLayer.GetAllLayers(false).OfType<UnstructuredGridFlowLinkCoverageLayer>().First();
                    flowLinkCoverageLayer.Visible = true;
                    view.MapView.MapControl.Refresh();
                    gui.DocumentViews.Remove(gui.DocumentViews.ActiveView); // close view
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        /// <summary>
        /// Method to test by dot Trace. Should be public for setting thresholds.
        /// </summary>
        /// <param name="gui"> DeltaShell application. </param>
        /// <param name="model"> The model which should be run. </param>
        public static void TimerMethod_RunFMModelWithGUIVisible(IGui gui, IActivity model)
        {
            gui.Application.ActivityRunner.Enqueue(model);

            while (gui.Application.IsActivityRunningOrWaiting(model))
            {
                Application.DoEvents();
            }
        }

        private static string GetBendProfPath()
        {
            return TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
        }
    }
}
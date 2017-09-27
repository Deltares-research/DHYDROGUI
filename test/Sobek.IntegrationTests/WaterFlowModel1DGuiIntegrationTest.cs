// 1. uncomment #define PROFILE_MEMORY
// 2. start profiler
// 3. take 1st memory snapshot after message box START is shown
// 4. click OK
// 5. take 2nd memory snapshot after message box FINISH is shown
// 6. analyze differences
// TIP: run all tests in a row and observe total memory use in Process Explorer - it should look like a horizontal saw (without climbing up)

//#define PROFILE_MEMORY

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap;
using SharpTestsEx;
using Application = System.Windows.Forms.Application;
using Control = System.Windows.Controls.Control;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.MemoryLeak)]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.Slow)]
    public class WaterFlowModel1DGuiIntegrationTest
    {
        private DeltaShellGui gui;
        private IApplication app;

        private long totalMemoryBeforeAllTests;

        private long totalMemoryBeforeTest;

        private long expectedMaxMemoryLeak;

        private struct TestMemoryLeakInfo
        {
            public string TestName { get; set; }

            public long ExpectedMaxMemoryLeak { get; set; }

            public long ActualMemoryLeak { get; set; }
        }

        private readonly List<TestMemoryLeakInfo> results = new List<TestMemoryLeakInfo>();

        private string testName;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();

            // avoid overhead when measuring memory leaks (run selected tests once so that a single reflection overhead will not be counted as a memory leak)
#if PROFILE_MEMORY
            PreventMemoryOverheads();
#else
            if(GuiTestHelper.IsBuildServer)
            {
                PreventMemoryOverheads();
            }
#endif

            FlushMemory();
            totalMemoryBeforeAllTests = GC.GetTotalMemory(true);
        }

        /// <summary>
        /// Performs typical actions which consume memory once (non-increasing).
        /// </summary>
        private void PreventMemoryOverheads()
        {
            try
            {
                InitializeGui();
                SavingAndImportingandSavingShouldNotThrowException();
                MovingADiffuseLateralShouldMaintainData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                DisposeGui();
            }

            try
            {
                InitializeGui();
                ImportInitialConditionWithOpenSideView(); // initializes python scripting
            }
            finally
            {
                DisposeGui();
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();

            FlushMemory();
            var totalMemoryAfterTest = GC.GetTotalMemory(true);
            Trace.Write(string.Format("Total memory leak after all tests: {0}.", (totalMemoryAfterTest - totalMemoryBeforeAllTests)));
            Trace.WriteLine("");
            Trace.WriteLine("TestName ExpectedMaxMemoryLeak ActualMemoryLeak");
            foreach (var info in results)
            {
                if (info.ActualMemoryLeak > info.ExpectedMaxMemoryLeak)
                {
                    Trace.WriteLine(info.TestName + " " + info.ExpectedMaxMemoryLeak + " " + info.ActualMemoryLeak + " ***");
                }
                else
                {
                    Trace.WriteLine(info.TestName + " " + info.ExpectedMaxMemoryLeak + " " + info.ActualMemoryLeak);
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            expectedMaxMemoryLeak = 0;

#if PROFILE_MEMORY
            System.Windows.Forms.MessageBox.Show(@"Collect memory using profiler (START).");
#endif

            FlushMemory();
            totalMemoryBeforeTest = GC.GetTotalMemory(true);

            InitializeGui();
        }

        [TearDown]
        public void TearDown()
        {
            DisposeGui();

            FlushMemory();
            var totalMemoryAfterTest = GC.GetTotalMemory(true);
            var totalMemoryLeakAfterTest = totalMemoryAfterTest - totalMemoryBeforeTest;
            Trace.Write(string.Format("Total memory leak after test: {0}.", totalMemoryLeakAfterTest));

            if (expectedMaxMemoryLeak == 0)
            {
                return;
            }

#if PROFILE_MEMORY
            System.Windows.Forms.MessageBox.Show(@"Collect memory using profiler (FINISH).");
#endif

            results.Add(new TestMemoryLeakInfo { TestName = testName, ActualMemoryLeak = totalMemoryLeakAfterTest, ExpectedMaxMemoryLeak = expectedMaxMemoryLeak });

            //totalMemoryLeakAfterTest
            //    .Should("memory leak detected").Be.LessThan(expectedMaxMemoryLeak);
        }

        private static void FlushMemory()
        {
            for(int i = 0; i < 10; i++)
            {
                Application.DoEvents(); //give threads waiting on invoke required time to finish
            }

            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }

        private void InitializeGui()
        {
            new RunningActivityLogAppender();
                //HACK: inside this constructor singleton magic happens, this should not be required

            gui = new DeltaShellGui();
            app = gui.Application;
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());
            gui.Run();
        }

        private void DisposeGui()
        {
            gui.Dispose();

            gui = null;
            app = null;
        }

        [Test]
        public void AddFlowModelAndSaveProject()
        {
            app.Project.RootFolder.Add(new WaterFlowModel1D());
            app.SaveProjectAs(TestHelper.GetCurrentMethodName());

            expectedMaxMemoryLeak = 570000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.Integration)]
        public void DeletingAModelWithNetCDFOutputAfterCreatingANewProjectShouldWork_Tools7636()
        {
            using (var model = new WaterFlowModel1D())
            {
                gui.CommandHandler.AddItemToProject(model);
                gui.Selection = model;
                gui.CommandHandler.DeleteCurrentProjectItem();
            }

            gui.Application.CreateNewProject();

            using (var model2 = new WaterFlowModel1D())
            {
                gui.CommandHandler.AddItemToProject(model2);
                gui.Selection = model2;
                gui.CommandHandler.DeleteCurrentProjectItem();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SavingAndImportingandSavingShouldNotThrowException()
        {
            // add demo model to project
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            gui.Application.Project.RootFolder.Add(model);
            
            //add cross section trapezium

            var pump = new Pump("pump") { Geometry = new Point(20, 0) };
            var weir = new Weir("weir") { Geometry = new Point(30, 0) };
            var bridge = new Bridge("brug") { Geometry = new Point(40, 0) };


            var branch = model.Network.Branches[0];
            NetworkHelper.AddBranchFeatureToBranch(pump, branch, 20.0);
            NetworkHelper.AddBranchFeatureToBranch(weir, branch, 30.0);
            NetworkHelper.AddBranchFeatureToBranch(bridge, branch, 40.0);
            var cs = CrossSection.CreateDefault(CrossSectionType.Standard, branch, 75.0);
            NetworkHelper.AddBranchFeatureToBranch(cs,branch,75.0);
            
            // save project
            const string path = "ModelWithCrossSection.dsproj";
            
            app.SaveProjectAs(path);
            
            // import saved project into this project
            var projectImporter = new ProjectImporter
                {
                    TargetDataDirectory = gui.Application.ProjectDataDirectory,
                    HybridProjectRepository = gui.Application.HybridProjectRepository
                };
            var importedItems = (IEnumerable<IProjectItem>) projectImporter.ImportItem(path);
            gui.Application.Project.RootFolder.Add(importedItems.First()); //only take flow model
            
            // save again (we want no crash)
            app.SaveProject();
        }

        [Test]
        public void OpeningInitialConditionsCoverageViewShouldNotClearOutputCoverages()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            gui.Application.Project.RootFolder.Add(model);

            ActivityRunner.RunActivity(model);

            model.OutputFunctions.First().Components[0].Values.Count.Should("Model ran: should have output coverages").Be.GreaterThan(0);
            
            var mainWindow = (Window)gui.MainWindow;

            Action onShown = delegate
            {
                gui.Selection = model;
                var view =
                    gui.DocumentViewsResolver.CreateViewForData(model.InitialConditions,
                    info => info.ViewType == typeof (CoverageTableView)) as CoverageTableView;

                Assert.NotNull(view);

                gui.DocumentViews.Add(view);

                model.OutputFunctions.First().Components[0].Values.Count.Should(
                    "Model ran and view opened: should still have output coverages").Be.GreaterThan(0);
            };
            WpfTestHelper.ShowModal(mainWindow, onShown);

            expectedMaxMemoryLeak = 1000000;
            testName = TestHelper.GetCurrentMethodName();
        }
        
        [Test]
        public void OpeningBoundaryConditionsCoverageViewShouldNotClearOutputCoverages()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            gui.Application.Project.RootFolder.Add(model);
            gui.SelectedProjectItem = model.GetDataItemByValue(model.Network);

            ActivityRunner.RunActivity(model);

            model.OutputFunctions.First().Components[0].Values.Count.Should("Model ran: should have output coverages").Be.GreaterThan(0);

            var mainWindow = (Window)gui.MainWindow;

            WpfTestHelper.ShowModal(mainWindow, () =>
                {
                    gui.DocumentViewsResolver.OpenViewForData(model.BoundaryConditions[0], typeof(WaterFlowModel1DBoundaryNodeDataViewWpf));

                    model.OutputFunctions.First().Components[0].Values.Count.Should("Model ran and view opened: should still have output coverages").Be.GreaterThan(0);
                });
            mainWindow.Close();

            expectedMaxMemoryLeak = 300000;
            testName = TestHelper.GetCurrentMethodName();
        }
        
        [Test]
        [Ignore("Not able to reproduce issue using this test..not sure what I'm doing wrong, but it's taking too much time so stopping")]
        [Category((TestCategory.Jira))] //TOOLS-4363
        public void SavingProjectAfterOpeningNetworkViewAndDeletingModelShouldNotThrowException()
        {
            const string path = "SavingProjectAfterOpening.dsproj";

            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            gui.Application.Project.RootFolder.Add(model);

            var mainWindow = (Window)gui.MainWindow;

            WpfTestHelper.ShowModal(mainWindow, () =>
            {
                //open network view:
                gui.DocumentViewsResolver.OpenViewForData(model.Network, typeof(CoverageView));

                //now delete model:
                var projectItem = gui.Application.Project.RootFolder.Items.First(i => i.Name.Contains("flow"));
                gui.Application.Project.RootFolder.Items.Remove(projectItem);

                gui.Application.SaveProjectAs(path);
            });
        }

        [Test]
        [Category((TestCategory.Jira))]
        [Category(TestCategory.DataAccess)]
        public void ErrorInOutputCoveragesAfterLoadingModelWithItsOutputCleared_Tools7337()
        {
            const string path = "ErrorInOutputCoverages.dsproj";

            // add demo flow model to project
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            gui.Application.Project.RootFolder.Add(model);

            // need to show the MainWindow otherwise we get the following error when running activity:
            // Cannot set Owner property to a Window that has not been shown previously.
            gui.MainWindow.Show();

            // run flow model
            gui.Application.RunActivity(model);
            
            // change something in network to cause flow to clear output
            model.Network.Branches[0].BranchFeatures.Add(new LateralSource());
            
            // save project
            gui.Application.SaveProjectAs(path);

            // load project
            gui.Application.OpenProject(path);

            // load back
            var loadedProject = gui.Application.Project;
            var loadedFlow = (WaterFlowModel1D) loadedProject.RootFolder.Models.First();
            var loadedOutputWaterLevel = loadedFlow.OutputWaterLevel;

            // assert
            var locations = loadedOutputWaterLevel.Locations.Values.ToList();
            Assert.AreEqual(0, locations.Count, "#count");
        }

        [Test]
        public void OpeningCrossSectionViewOfTypeHeightFlowStorageWidthShouldNotClearOutputCoverages()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var csTypeFp1 = new CrossSectionSectionType { Name = "FloodPlain1" };
            var csTypeFp2 = new CrossSectionSectionType { Name = "FloodPlain2" };
            var tabulatedCrossSectionDefinition = GetTabulatedCrossSectionDefinition();

            var geometryBasedCS = model.Network.Branches[0].BranchFeatures.First(bf => bf is ICrossSection);
            model.Network.Branches[0].BranchFeatures.Remove(geometryBasedCS);
            model.Network.CrossSectionSectionTypes.Add(csTypeFp1);
            model.Network.CrossSectionSectionTypes.Add(csTypeFp2);

            const int offset = 75;
            var HFSWCrossSection = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(model.Network.Branches[0], tabulatedCrossSectionDefinition, offset);

            HFSWCrossSection.Name = "cs";

            gui.Application.Project.RootFolder.Add(model);

            ActivityRunner.RunActivity(model);

            model.OutputFunctions.Count().Should("Model ran: should have output coverages").Be.GreaterThan(0);

            var mainWindow = (Window)gui.MainWindow;

            WpfTestHelper.ShowModal(mainWindow, () =>
            {
                gui.DocumentViewsResolver.OpenViewForData(HFSWCrossSection);

                model.OutputFunctions.Count().Should("Model ran and view opened: should still have output coverages").Be.GreaterThan(0);
            });

            expectedMaxMemoryLeak = 1000000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void ShowGuiWithWaterFlowModel1DAndUndoRedoEnabled()
        {
            gui.UndoRedoManager.TrackChanges = true;

            WpfTestHelper.ShowModal((Control) gui.MainWindow);
            expectedMaxMemoryLeak = 890000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void OpenMapViewForInitialConditionsShouldCreateMapWithThreeLayers()
        {
            var model = new WaterFlowModel1D();
            gui.Application.Project.RootFolder.Add(model);

            var mainWindow = (Window)gui.MainWindow;

            Action onShown = delegate
            {
                gui.Selection = model;
                gui.DocumentViewsResolver.OpenViewForData(model.InitialConditions);
                var mapView = ((ProjectItemMapView)gui.DocumentViews.ActiveView).MapView;

                // network editor plugin adds network as a second layer (TODO: move network-related logic from HydroNetworkEditorMapTool into (HydroNetwork)CoverageView
                mapView.Map.Layers.Count.Should().Be.EqualTo(1);
            };
            WpfTestHelper.ShowModal(mainWindow, onShown);

            expectedMaxMemoryLeak = 1000000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void DeleteBranchFromTheNetworkWhichIsPartOfModelShouldRemoveViewsOfBranchFeatures()
        {
            var model = new WaterFlowModel1D();

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));
            var compositeBranchStructure = new CompositeBranchStructure { Geometry = new Point(50, 0) };
            var pump = new Pump("pump") { Geometry = new Point(50, 0) };
            var weir = new Weir("weir") { Geometry = new Point(50, 0) };
            var bridge = new Bridge("brug") { Geometry = new Point(50, 0) };

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, bridge);

            model.Network = network;
            gui.Application.Project.RootFolder.Add(model);
            var mainWindow = (Window)gui.MainWindow;

            gui.DocumentViews.Clear();

            WpfTestHelper.ShowModal(mainWindow, () =>
                {
                    gui.CommandHandler.OpenView(compositeBranchStructure);
                    network.Branches.Remove(network.Branches[0]);

                    gui.DocumentViews.Count.Should().Be.EqualTo(0);
                });

            expectedMaxMemoryLeak = 650000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void CopyPasteDemoModelInApplicationAndRun()
        {
            TestHelper.SetDeltaresLicenseToEnvironmentVariable();

            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            gui.Application.Project.RootFolder.Add(model);

            var clonedModel = (WaterFlowModel1D)model.Clone();
            gui.Application.Project.RootFolder.Add(clonedModel);
            
            RunModelAndWaitToFinish(clonedModel);

            Assert.AreEqual(ActivityStatus.Cleaned, clonedModel.Status);

            expectedMaxMemoryLeak = 83000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunReportShouldBeAvailableForCancelledRun()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            gui.Application.Project.RootFolder.Add(model);

            model.StopTime = model.StartTime.AddDays(3); //make sure model runs for a long time
            
            bool readyForCancel = false;
            const string someMessage = "some message";

            gui.Application.ActivityRunner.ActivityStatusChanged += (s, e) =>
                {
                    if (e.NewStatus == ActivityStatus.Executing)
                    {
                        //log4net isn't propery initialized, so we do this manually
                        RunningActivityLogAppender.Instance.DoAppend(
                            new LoggingEvent(new LoggingEventData { Message = someMessage, TimeStamp = DateTime.Now }));

                        readyForCancel = true;  //we completed one timestep
                    }
                };

            gui.Application.RunActivityInBackground(model);

            while (!readyForCancel)
            {
                Application.DoEvents(); //nasty!
                Thread.Sleep(50);
            }
            
            gui.Application.ActivityRunner.CancelAll();

            int maxAttempts = 200;

            while (gui.Application.ActivityRunner.IsRunning || model.Status == ActivityStatus.Cancelling)
            {
                if (maxAttempts-- <= 0)
                    Assert.Fail("Cancel failed: no point waiting any longer");

                Application.DoEvents(); //nasty!
                Thread.Sleep(50);
            }
            
            var documents = model.DataItems.Select(item => item.Value).OfType<TextDocument>();
            var runReport = documents.First(doc => doc.Name == "Run report");
            Assert.IsTrue(runReport.Content.Contains(someMessage));

            expectedMaxMemoryLeak = 60000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void CloneModelInProjectShouldBreakExternalLinks()
        {
            var network = new HydroNetwork();
            var networkDataItem = new DataItem { Value = network, ValueType = typeof(HydroNetwork) };

            var model = new WaterFlowModel1D();
     
            var project = gui.Application.Project;
            project.RootFolder.Add(networkDataItem);
            project.RootFolder.Add(model);

            var flowModelNetworkDataItem = model.GetDataItemByValue(model.Network);
            flowModelNetworkDataItem.LinkTo(networkDataItem);
            
            // clone
            var clonedModel = (WaterFlowModel1D)model.Clone();

            // asserts
            Assert.AreEqual(network, clonedModel.Network);
            Assert.AreEqual(networkDataItem, clonedModel.GetDataItemByValue(clonedModel.Network).LinkedTo);
            Assert.AreEqual(1, networkDataItem.LinkedBy.Count);
            Assert.AreEqual(flowModelNetworkDataItem, networkDataItem.LinkedBy[0]);

            expectedMaxMemoryLeak = 150000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void CopyPasteModelInProjectShouldNotBreakExternalLinks()
        {
            var network = new HydroNetwork();
            var networkDataItem = new DataItem { Value = network, ValueType = typeof(HydroNetwork) };

            var model = new WaterFlowModel1D();

            var project = gui.Application.Project;
            project.RootFolder.Add(networkDataItem);
            project.RootFolder.Add(model);

            var flowModelNetworkDataItem = model.GetDataItemByValue(model.Network);
            flowModelNetworkDataItem.LinkTo(networkDataItem);
            
            // action
            gui.CopyPasteHandler.Copy(model);
            gui.CopyPasteHandler.Paste(project, project.RootFolder);

            var flowModelCopy = (WaterFlowModel1D) project.RootFolder.Models.Skip(1).First();

            // assert
            Assert.AreEqual(network, flowModelCopy.Network);
            var flowModelCopyNetworkDataItem = flowModelCopy.GetDataItemByValue(flowModelCopy.Network);
            Assert.AreEqual(networkDataItem, flowModelCopyNetworkDataItem.LinkedTo);
            Assert.AreEqual(2, networkDataItem.LinkedBy.Count);
            Assert.AreEqual(flowModelNetworkDataItem, networkDataItem.LinkedBy[0]);
            Assert.AreEqual(flowModelCopyNetworkDataItem, networkDataItem.LinkedBy[1]);

            expectedMaxMemoryLeak = 930000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void RunCopyPasteRunDemoModelInApplication()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            gui.Application.Project.RootFolder.Add(model);

            //run the model
            RunModelAndWaitToFinish(model);

            //action! clone it
            Assert.AreEqual(model.Network, model.InitialConditions.Network);


            var clonedModel = (WaterFlowModel1D)model.Clone();
            gui.Application.Project.RootFolder.Add(clonedModel);

            Assert.AreEqual(clonedModel.Network, clonedModel.InitialConditions.Network);
            //run it again!
            RunModelAndWaitToFinish(clonedModel);

            Assert.AreEqual(ActivityStatus.Cleaned, clonedModel.Status);

            expectedMaxMemoryLeak = 1000000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void StartDeltaShellWithTimeDependendNetworkCoverage()
        {
            StartDeltaShellWithCoverage(GetBigTimeDependendNetworkCoverage());

            expectedMaxMemoryLeak = 290000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void StartDeltaShellWithBigTimeDependendNetworkCoverage()
        {
            StartDeltaShellWithCoverage(GetTimeDependendNetworkCoverage());

            expectedMaxMemoryLeak = 290000;
            testName = TestHelper.GetCurrentMethodName();
        }

        private void StartDeltaShellWithCoverage(INetworkCoverage coverage)
        {
            var project = app.Project;

            // create a network coverage 
            project.RootFolder.Add(new Map());
            project.RootFolder.Add(coverage);

            // show gui main window
            var mainWindow = (Window)gui.MainWindow;

            WpfTestHelper.ShowModal(mainWindow);
        }

        private static INetworkCoverage GetBigTimeDependendNetworkCoverage()
        {
            //create a 'grid' network of 100 by 100
            const int rows = 100;
            const int cols = 100;

            IList<Point> points = new List<Point>();
            const int nodeCount = rows * cols;
            for (int i = 0; i < nodeCount; i++)
            {
                int x = (i % cols) * 10;
                int y = (i / cols) * 10;
                //'invert' the odd rows
                if (y % 2 == 1)
                {
                    x = (cols * 10) - x;
                }
                points.Add(new Point(x, y));
            }
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(points.ToArray());

            var networkCoverage = new NetworkCoverage("test", true) {Network = network};
            //5 locations at 10,20,30,40,50
            networkCoverage.Locations.FixedSize = nodeCount - 1;
            networkCoverage.SetLocations(Enumerable.Range(0, nodeCount - 1)
                                             .Select(i => (INetworkLocation)new NetworkLocation(network.Branches[i], 5)));
            //100 timesteps set values
            DateTime startTime = DateTime.Now;
            foreach (int i in Enumerable.Range(1, 100))
            {
                networkCoverage.AddValuesForTime(Enumerable.Range(0, nodeCount - 1).Select(j => (double)(j * i)), startTime.AddDays(i));
            }
            return networkCoverage;
        }
        private static INetworkCoverage GetTimeDependendNetworkCoverage()
        {
            //create a network with one horizontal branch
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var networkCoverage = new NetworkCoverage("test", true) {Network = network};
            //5 locations at 10,20,30,40,50
            networkCoverage.Locations.FixedSize = 5;
            networkCoverage.Locations.SetValues(new[] { 1, 2, 3, 4, 5 }
                .Select(i => new NetworkLocation(network.Branches[0], i * 10)));
            //5 timesteps set values
            foreach (int i in Enumerable.Range(1, 5))
            {
                networkCoverage.AddValuesForTime(new[] { 10.0, 2.0, 3.0, 4.0, 5.0 }.Select(j => j * i), new DateTime(2000, 1, i));
            }
            return networkCoverage;
        }


        private void RunModelAndWaitToFinish(IModel model)
        {
            // need to show the MainWindow otherwise we get the following error when running activity:
            // Cannot set Owner property to a Window that has not been shown previously.
            gui.MainWindow.Show();

            gui.Application.RunActivityInBackground(model);
            while (gui.Application.ActivityRunner.IsRunning)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
        }

        [Test]
        public void RemovingModelWhichUsesNetworkFromProjectShouldNotRemoveNetworkViewContext()
        {
            WpfTestHelper.ShowModal((Control) gui.MainWindow, () => {

            var project = app.Project;

            // add network
            var networkItem = new DataItem(new HydroNetwork());
            project.RootFolder.Add(networkItem);

            // add model and link it's network to networkItem
            var model = new WaterFlowModel1D();
            project.RootFolder.Add(model);

            var modelNetworkItem = model.DataItems.First(i => i.Value is IHydroNetwork);

            modelNetworkItem.LinkTo(networkItem);

            // open view for network
            gui.DocumentViewsResolver.OpenViewForData(modelNetworkItem.Value, typeof(ProjectItemMapView));
            
            // gui context is added
            var networkEditorViewContext = gui.ViewContextManager.GetViewContext(typeof(ProjectItemMapView), networkItem.Value);

            // now remove model
            project.RootFolder.Items.Remove(model);

            var networkEditorViewContextAfterRemoveModel = gui.ViewContextManager.GetViewContext(typeof(ProjectItemMapView), networkItem.Value);

            networkEditorViewContextAfterRemoveModel
                .Should("network editor view context is the same after remove model").Be.EqualTo(
                    networkEditorViewContext);

            expectedMaxMemoryLeak = 850000;
            testName = TestHelper.GetCurrentMethodName();
            });
        }

        private static CrossSectionDefinition GetTabulatedCrossSectionDefinition()
        {
            var csTypeFpMain = new CrossSectionSectionType { Name = "Main" };
            var tabulatedCrossSection = new CrossSectionDefinitionZW("tabulatedCS");
            tabulatedCrossSection.SetWithHfswData(new[]
                                                      {
                                                          new HeightFlowStorageWidth(0.0, 10.0, 9.0),
                                                          new HeightFlowStorageWidth(1.0, 110.0, 109.0),
                                                          new HeightFlowStorageWidth(2.0, 210.0, 209.0)
                                                      });


            tabulatedCrossSection.Sections.Add(new CrossSectionSection { SectionType = csTypeFpMain, MinY = 0, MaxY = 109 });
            return tabulatedCrossSection;
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void Remove500CrossSectionsWithGuiIsFast()
        {
            LogHelper.ConfigureLogging(Level.Fatal);

            var modelImporter = new SobekWaterFlowModel1DImporter();

            var modelPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\JAMM2010.sbk\40\DEFTOP.1");
            var importedModel = (ICompositeActivity)modelImporter.ImportItem(modelPath);

            app.Project.RootFolder.Items.Add(importedModel as IModel);

            var flowModel = importedModel.Activities.OfType<WaterFlowModel1D>().First();

            // THIS IS EXTREMELY SLOW!
            TestHelper.AssertIsFasterThan(2500, () =>
                flowModel.Network.Branches.ForEach(br => br.BranchFeatures.RemoveAllWhere(bf => bf is ICrossSection)));

            //doesn't call BeginEdit/EndEdit, which would stop RTC from reacting on each individual remove: but this is
            //similar to how the tables in HydroNetworkEditor currently work (they call Begin/EndEdit, but for each 
            //individual item, so that doesn't help much), so unfortunately this test is realistic for that situation.

            expectedMaxMemoryLeak = 300000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ImportCSVCrossSectionsToEmptyMaasModelIsFast()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();

            var modelPath =
                TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"ReModels\JAMM2010.sbk\40\DEFTOP.1");
            var importedModel = (ICompositeActivity)modelImporter.ImportItem(modelPath);

            app.Project.RootFolder.Items.Add(importedModel as IModel);

            var flowModel = importedModel.Activities.OfType<WaterFlowModel1D>().First();

            //remove all cross sections
            flowModel.Network.Branches.ForEach(br => br.BranchFeatures.RemoveAllWhere(bf => bf is ICrossSection));

            var csvFileImporter = new CrossSectionYZFromCsvFileImporter();

            var path = TestHelper.GetTestFilePath("maas_crosssections_yz.csv");

            LogHelper.SetLoggingLevel(Level.Off);
            TestHelper.AssertIsFasterThan(1825, () => csvFileImporter.ImportItem(path, flowModel.Network)); //include reading now

            Assert.AreEqual(812, flowModel.Network.CrossSections.Count());

            expectedMaxMemoryLeak = 260000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void ImportIntoNetworkWithOpenHydroNetworkEditor()
        {
            WpfTestHelper.ShowModal((Control) gui.MainWindow, (() =>
                {
                    Application.DoEvents();

                    var waterFlowModel1D = new WaterFlowModel1D();
                    gui.Application.Project.RootFolder.Add(waterFlowModel1D);
                    var network = waterFlowModel1D.Network;

                    gui.CommandHandler.OpenView(network, typeof(ProjectItemMapView));

                    var modelPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                            @"ReModels\JAMM2010.sbk\40\DEFTOP.1");

                    var toNetworkImported = new SobekNetworkToNetworkImporter { TargetObject = network };
                    toNetworkImported.ImportItem(modelPath);
                }));

            expectedMaxMemoryLeak = 260000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.WorkInProgress)]
        public void ImportIntoNetworkShouldNotSlowDownBecauseOfTreeView()
        {
            WpfTestHelper.ShowModal((Control) gui.MainWindow, (() =>
            {
                Application.DoEvents();

                var waterFlowModel1D = new WaterFlowModel1D();
                gui.Application.Project.RootFolder.Add(waterFlowModel1D);
                var network = waterFlowModel1D.Network;

                var modelPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                        @"ReModels\JAMM2010.sbk\40\DEFTOP.1");

                // warm-up
                var importer = new SobekNetworkImporter();
                importer.ImportItem(modelPath);

                // without tree view events
                TreeViewController.SkipEvents = true;

                var toNetworkImported = new SobekNetworkToNetworkImporter { TargetObject = network };
                gui.CommandHandler.OpenView(network, typeof(ProjectItemMapView));

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                toNetworkImported.ImportItem(modelPath);
                stopwatch.Stop();

                var withoutTreeViewEvents = stopwatch.ElapsedMilliseconds;

                // with tree view events
                TreeViewController.SkipEvents = false;

                waterFlowModel1D = new WaterFlowModel1D();
                gui.Application.Project.RootFolder.Add(waterFlowModel1D);
                network = waterFlowModel1D.Network;

                toNetworkImported = new SobekNetworkToNetworkImporter { TargetObject = network };
                gui.CommandHandler.OpenView(network, typeof(ProjectItemMapView));

                stopwatch.Reset();
                stopwatch.Start();
                toNetworkImported.ImportItem(modelPath);
                stopwatch.Stop();

                var importWithTreeViewEvents = stopwatch.ElapsedMilliseconds;

                Console.WriteLine(@"Without tree view event handler: " + withoutTreeViewEvents);
                Console.WriteLine(@"With tree view event handler: " + importWithTreeViewEvents);

                var ratio = importWithTreeViewEvents / (double)withoutTreeViewEvents;

                Console.WriteLine(ratio + @" times slower");

                ratio.Should("slow down factor because of tree view event handler").Be.LessThan(1.15); // 15%
            }));
        }
        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportSloterplasSobekIntoNetworkExportToSobekFilebased()
        {
            WpfTestHelper.ShowModal((Control)gui.MainWindow, (() =>
            {
                Application.DoEvents();

                var modelPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly,@"ExpSBI.lit\1\NETWORK.TP");

                var modelImporter = new SobekWaterFlowModel1DImporter { TargetItem = new WaterFlowModel1D() };
                var model = (WaterFlowModel1D)modelImporter.ImportItem(modelPath);
                gui.Application.Project.RootFolder.Add(model);
                var network = model.Network;
                
                var toNetworkImported = new SobekNetworkToNetworkImporter { TargetObject = network };
                gui.CommandHandler.OpenView(network, typeof(ProjectItemMapView));

                toNetworkImported.ImportItem(modelPath);
                gui.CommandHandler.OpenView(network, typeof(ProjectItemMapView));
                
                var modelNetworkItem = model.DataItems.First(i => i.Value is IHydroNetwork);

                // open view for network
                gui.DocumentViewsResolver.OpenViewForData(modelNetworkItem, typeof(ProjectItemMapView));
                
                // give delayed event handler some time
                Thread.Sleep(100);
                Application.DoEvents();
                Thread.Sleep(100);

                // add cross sections which were not imported correctly so that our model is valid
                var branch = network.Channels.FirstOrDefault(channel => channel.Name == "2");
                Assert.NotNull(branch);
                var crossSection = CrossSection.CreateDefault(CrossSectionType.ZW, branch, branch.Length / 2);
                branch.BranchFeatures.Add(crossSection);
                
                branch = network.Channels.FirstOrDefault(channel => channel.Name == "CH120");
                Assert.NotNull(branch);
                crossSection = CrossSection.CreateDefault(CrossSectionType.ZW, branch, branch.Length / 2);
                branch.BranchFeatures.Add(crossSection);

                branch = network.Channels.FirstOrDefault(channel => channel.Name == "CH410");
                Assert.NotNull(branch);
                crossSection = CrossSection.CreateDefault(CrossSectionType.ZW, branch, branch.Length / 2);
                branch.BranchFeatures.Add(crossSection);

                branch = network.Channels.FirstOrDefault(channel => channel.Name == "CH479");
                Assert.NotNull(branch);
                crossSection = CrossSection.CreateDefault(CrossSectionType.ZW, branch, branch.Length / 2);
                branch.BranchFeatures.Add(crossSection);

                var validation = model.Validate();
                Assert.AreEqual(0, validation.ErrorCount);

                var exporter = new WaterFlowModel1DExporter();

                string filepath = TestHelper.GetTestDataPath(typeof(WaterFlowModel1DGuiIntegrationTest).Assembly, "BridgeExport_SOBEK3-54.md1d"); 
                TestHelper.AssertIsFasterThan(5000, () => exporter.Export(model, filepath));

                Assert.IsTrue(File.Exists(filepath));
                
            }));
        }
        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.WorkInProgress)]
        public void ImportIntoNetworkWithOpenHydroNetworkEditorShouldNotSlowDown()
        {
            WpfTestHelper.ShowModal((Control) gui.MainWindow, (() =>
            {
                Application.DoEvents();

                TreeViewController.SkipEvents = true;

                var waterFlowModel1D = new WaterFlowModel1D();
                gui.Application.Project.RootFolder.Add(waterFlowModel1D);
                var network = waterFlowModel1D.Network;

                var modelPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                        @"ReModels\JAMM2010.sbk\40\DEFTOP.1");

                // warm-up
                var importer = new SobekNetworkImporter();
                importer.ImportItem(modelPath);

                // without view
                var toNetworkImported = new SobekNetworkToNetworkImporter { TargetObject = network };

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                toNetworkImported.ImportItem(modelPath);
                stopwatch.Stop();

                var importWithoutNetworkEditor = stopwatch.ElapsedMilliseconds;

                // with view
                waterFlowModel1D = new WaterFlowModel1D();
                gui.Application.Project.RootFolder.Add(waterFlowModel1D);
                network = waterFlowModel1D.Network;

                toNetworkImported = new SobekNetworkToNetworkImporter { TargetObject = network };
                gui.CommandHandler.OpenView(network, typeof(ProjectItemMapView));

                stopwatch.Reset(); 
                stopwatch.Start();
                toNetworkImported.ImportItem(modelPath);
                stopwatch.Stop();

                var importWithNetworkEditor = stopwatch.ElapsedMilliseconds;

                Console.WriteLine(@"Without network editor: " + importWithoutNetworkEditor);
                Console.WriteLine(@"With network editor: " + importWithNetworkEditor);

                var ratio = importWithNetworkEditor / (double)importWithoutNetworkEditor;

                Console.WriteLine(ratio + @" times slower");
                
                ratio.Should("slow down factor because of opened HydroNetworkEditor").Be.LessThan(1.05); // 5%
            }));
        }

        [Test]
        public void ImportInitialConditionWithOpenSideView()
        {
            WpfTestHelper.ShowModal((Control) gui.MainWindow, (() =>
                {
                    //reconstruction of 5520. Crash when importing initial conditions with open side view.
                    var modelPath =
                        TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly,@"ReModels\JAMM2010.sbk\40\DEFTOP.1");
                    var importer = new SobekHydroModelImporter(false);
                        
                    var model = (HydroModel)importer.ImportItem(modelPath);
                    
                    gui.Application.Project.RootFolder.Add(model);

                    var waterFlowModel = model.Models.OfType<WaterFlowModel1D>().First();

                    // some strange result, probably bug in reader, sometimes 820, sometimes 756                    
//                    Assert.AreEqual(820, waterFlowModel.InitialConditions.Components[0].Values.Count);
//                    Assert.AreEqual(756, waterFlowModel.InitialConditions.Components[0].Values.Count);

                    var route = new Route {Network = waterFlowModel.Network};
                    var start = new NetworkLocation(waterFlowModel.Network.Branches.First(), 0);
                    var end = new NetworkLocation(waterFlowModel.Network.Branches.Last(), 40);

                    route.Locations.AddValues(new[]{start,end});
                    
                    gui.CommandHandler.OpenView(route, typeof(NetworkSideView));

                    //reimport
                    var icImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(modelPath, waterFlowModel, new IPartialSobekImporter[] {  new SobekInitialConditionsImporter() });
                    
                    //import in activity (single thread mode)
                    gui.Application.RunActivity(new SimpleActivity(icImporter.Import));
                }));

            expectedMaxMemoryLeak = 700000;
            testName = TestHelper.GetCurrentMethodName();
        }

        public class SimpleActivity : Activity
        {
            private readonly Action action;

            public SimpleActivity(Action action)
            {
                this.action = action;
            }

            protected override void OnInitialize()
            {
            }

            protected override void OnExecute()
            {
                action();
                Status = ActivityStatus.Done;
            }

            protected override void OnCancel()
            {
            }

            protected override void OnCleanUp()
            {
            }

            protected override void OnFinish()
            {
            }
        }
        
        [Test]
        public void CopyPasteModelWithOutputInNetcdf()
        {
            // since gui is created in setup - dispose it first
            DisposeGui();

            //using custom gui here because we need netcdf and don't want it in other tests
            using( gui = new DeltaShellGui())
            {
                gui.Application.Plugins.Add(new NetCdfApplicationPlugin());
                gui.Application.Plugins.Add(new NHibernateDaoApplicationPlugin());
                gui.Application.Plugins.Add(new CommonToolsApplicationPlugin());
                gui.Application.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Application.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                gui.Application.Plugins.Add(new SharpMapGisApplicationPlugin());
                gui.Application.Plugins.Add(new SobekImportApplicationPlugin());
                gui.Application.Plugins.ForEach(p => p.Application = gui.Application);

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());

                gui.Run();

                var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                var project = gui.Application.Project;
                project.RootFolder.Add(model);

                RunModelAndWaitToFinish(model);

                //copy paste the model
                gui.CopyPasteHandler.Copy(model);
                gui.CopyPasteHandler.Paste(project, project.RootFolder);

                var copy = project.GetAllItemsRecursive().OfType<WaterFlowModel1D>().First(
                                                                                           m => m.Name.StartsWith("Copy of"));

                Assert.AreNotSame(model.OutputDepth.Components[0].Unit, copy.OutputDepth.Components[0].Unit);
                Assert.AreEqual(model.OutputDepth.Components[0].Unit.Name, copy.OutputDepth.Components[0].Unit.Name);

                //check the output is there 
                Assert.AreEqual(11, copy.OutputWaterLevel.Time.Values.Count);
                Assert.AreEqual(27, copy.OutputWaterLevel.Locations.Values.Count);

                //change the ORIGINAL
                model.StartTime = new DateTime(2010, 1, 1);

                //check the copy was NOT affected 
                Assert.AreEqual(11, copy.OutputWaterLevel.Time.Values.Count);
                Assert.AreEqual(27, copy.OutputWaterLevel.Locations.Values.Count);
                
            }
            expectedMaxMemoryLeak = 600000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        [Category(TestCategory.Jira)]
        public void OpenStructureViewWhileBoundaryGroupDataViewOpenShouldNotCrash_Tools6939()
        {
            var model = new WaterFlowModel1D();
            gui.Application.Project.RootFolder.Add(model);
            model.Network = HydroNetworkHelper.GetSnakeHydroNetwork(new[] {new Point(0, 0), new Point(100, 0)});
            var network = model.Network;

            var weir = new Weir {Geometry = new Point(15, 0), Chainage = 15};
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir, network.Branches[0]);

            var mainWindow = (Window)gui.MainWindow;

            WpfTestHelper.ShowModal(mainWindow, () =>
                {
                    gui.CommandHandler.OpenView(model.BoundaryConditions);
                    gui.Selection = weir;
                });

            expectedMaxMemoryLeak = 950000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        [Category(TestCategory.Jira)]
        public void OpenStructureViewWhileLateralGroupDataViewOpenShouldNotCrash_Tools6939()
        {
            var model = new WaterFlowModel1D();
            gui.Application.Project.RootFolder.Add(model);
            model.Network = HydroNetworkHelper.GetSnakeHydroNetwork(new[] { new Point(0, 0), new Point(100, 0) });
            var network = model.Network;

            var weir = new Weir { Geometry = new Point(15, 0), Chainage = 15 };
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir, network.Branches[0]);

            var mainWindow = (Window)gui.MainWindow;

            Action onShown = delegate
            {
                gui.CommandHandler.OpenView(model.LateralSourceData);
                gui.Selection = weir;
            };
            WpfTestHelper.ShowModal(mainWindow, onShown);

            expectedMaxMemoryLeak = 270000;
            testName = TestHelper.GetCurrentMethodName();
        }
        
        [Test]
        public void MovingALateralShouldMaintainData()
        {
            var project = app.Project;

            // add data
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            
            project.RootFolder.Add(model);
            var network = model.Network;
            var branch = network.Branches[0];
            var lateral = AddLateralToBranch(branch, new Point(15, 0));

            // show gui main window
            var mainWindow = (Window)gui.MainWindow;

            WpfTestHelper.ShowModal(mainWindow, () =>
                {
                    network = model.Network;
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));
                    var networkEditor = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

                    //define data in lateral source data
                    var flowLateralData = model.LateralSourceData.First(ld => ld.Feature == lateral);
                    flowLateralData.Data[new DateTime(2000, 1, 1)] = 5.0;

                    MoveFeature(networkEditor, lateral, new Coordinate(25, 0));

                    //re-retrieve:
                    flowLateralData = model.LateralSourceData.First(ld => ld.Feature == lateral);
                    Assert.AreEqual(5, flowLateralData.Data[new DateTime(2000, 1, 1)]);
                });
            mainWindow.Close();
            expectedMaxMemoryLeak = 960000;
            testName = TestHelper.GetCurrentMethodName();
        }

        [Test]
        public void MovingADiffuseLateralShouldMaintainData()
        {
            var project = app.Project;

            // add data
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            project.RootFolder.Add(model);
            var network = model.Network;
            var branch = network.Branches[0];
            var lateral = AddLateralToBranch(branch, new Point(15, 0));
            lateral.Length = 30;

            // show gui main window
            var mainWindow = (Window)gui.MainWindow;

            WpfTestHelper.ShowModal(mainWindow, () =>
                {
                    network = model.Network;
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));
                    var networkEditor = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

                    //define data in lateral source data
                    var flowLateralData = model.LateralSourceData.First(ld => ld.Feature == lateral);
                    flowLateralData.Data[new DateTime(2000, 1, 1)] = 5.0;

                    MoveFeature(networkEditor, lateral, new Coordinate(25, 0));

                    //re-retrieve:
                    flowLateralData = model.LateralSourceData.First(ld => ld.Feature == lateral);
                    Assert.AreEqual(5, flowLateralData.Data[new DateTime(2000, 1, 1)]);
                });
            expectedMaxMemoryLeak = 1400000;
            testName = TestHelper.GetCurrentMethodName();
        }

        private static LateralSource AddLateralToBranch(IBranch branch, Point location)
        {
            var lateral = new LateralSource {Branch = branch, Geometry = location};
            NetworkHelper.UpdateBranchFeatureChainageFromGeometry(lateral);
            branch.BranchFeatures.Add(lateral);
            return lateral;
        }

        private static void MoveFeature(ProjectItemMapView networkEditor, IFeature feature, Coordinate targetCoordinate)
        {
            networkEditor.MapView.MapControl.SelectTool.Select(feature);
            var moveTool = networkEditor.MapView.MapControl.MoveTool;
            moveTool.OnMouseDown(feature.Geometry.Coordinate, new MouseEventArgs(MouseButtons.Left, 1, -1, -1, -1));
            moveTool.OnMouseMove(targetCoordinate, new MouseEventArgs(MouseButtons.Left, 1, -1, -1, -1));
            moveTool.OnMouseUp(targetCoordinate, new MouseEventArgs(MouseButtons.Left, 1, -1, -1, -1));
        }
    }
}
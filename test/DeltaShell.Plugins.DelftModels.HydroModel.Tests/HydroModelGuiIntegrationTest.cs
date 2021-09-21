using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.Gui.Forms.MainWindow;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class HydroModelGuiIntegrationTest
    {
        private DeltaShellGui gui;
        private IApplication app;

        [SetUp]
        public void SetUp()
        {
            InitializeGui();
        }

        [TearDown]
        public void TearDown()
        {
            DisposeGui();
        }

        private void InitializeGui()
        {
            //new RunningActivityLogAppender();
            //HACK: inside this constructor singleton magic happens, this should not be required

            gui = new DeltaShellGui();
            app = gui.Application;
            
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new SobekImportApplicationPlugin());

            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new HydroModelGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());
            gui.Plugins.Add(new RainfallRunoffGuiPlugin());
            gui.Plugins.Add(new FlowFMGuiPlugin());

            gui.Run();
        }

        private void DisposeGui()
        {
            gui.Dispose();

            gui = null;
            app = null;
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ExportHydroModelWithCatchmentsAndSave()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.SobekModels);

            var paved = new Catchment { CatchmentType = CatchmentType.Paved };
            var unpaved = new Catchment {CatchmentType = CatchmentType.Unpaved};
            var green = new Catchment { CatchmentType = CatchmentType.GreenHouse };
            var water = new Catchment { CatchmentType = CatchmentType.OpenWater };
            var sacr = new Catchment { CatchmentType = CatchmentType.Sacramento };
            var hbv = new Catchment { CatchmentType = CatchmentType.Hbv };

            hydroModel.Region.SubRegions.OfType<IDrainageBasin>()
                      .First()
                      .Catchments.AddRange(new[] {paved, unpaved, green, water, sacr, hbv});

            app.Project.RootFolder.Add(hydroModel);
            var filename = TestHelper.GetCurrentMethodName();

            app.ExportProjectItem(hydroModel, filename + "_1.dsproj", true);

            app.SaveProjectAs("_2.dsproj"); //bang, exception!
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category("ToCheck")]
        public void ShowFMModelRunCoupledToRTC()
        {
            WaterFlowFMModel flow;
            RealTimeControlModel rtc;
            var hydroModel = CreateFMRTCModel(out rtc, out flow, app);

            // wait until gui starts
            Action mainWindowShown = delegate
            {
                ActivityRunner.RunActivity(hydroModel);
                gui.Selection = flow;
                gui.CommandHandler.OpenViewForSelection(typeof(ProjectItemMapView));
                var view = gui.DocumentViews.ActiveView as ProjectItemMapView;
                var velocityLayer = view.MapView.Map.GetAllLayers(true).FirstOrDefault(l => l.Name.Contains("x-component"));
                Assert.IsNotNull(velocityLayer);
                velocityLayer.Visible = true;
                var timeSeriesNavigator = gui.ToolWindowViews.OfType<TimeSeriesNavigator>().First();
                for (var i = 0; i < 20; ++i)
                {
                    timeSeriesNavigator.SelectNextTime();
                }
                Console.WriteLine("1");
            };
            WpfTestHelper.ShowModal((MainWindow)gui.MainWindow, mainWindowShown);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Add2D3DIntegratedModelAddFMModelRemoveIntegratedModel()
        {
            var mainWindow = (MainWindow) gui.MainWindow;

            if (!gui.DocumentViewsResolver.DefaultViewTypes.ContainsKey(typeof(WaterFlowFMModel)))
                gui.DocumentViewsResolver.DefaultViewTypes.Add(typeof(WaterFlowFMModel), typeof(WaterFlowFMFileStructureView));

            Action mainWindowShown = delegate
            {
                var hydroModelBuilder = new HydroModelBuilder();
                using (var integratedModel2D3D = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels))
                {
                    using (var waterFlowFMModel = new WaterFlowFMModel())
                    {
                        gui.CommandHandler.AddItemToProject(integratedModel2D3D);
                        gui.Selection = integratedModel2D3D;
                        gui.CommandHandler.OpenViewForSelection();
                        gui.CommandHandler.AddItemToProject(waterFlowFMModel);
                        gui.Selection = waterFlowFMModel;
                        gui.CommandHandler.OpenViewForSelection();
                        gui.Application.Project.RootFolder.Items.Remove(integratedModel2D3D);
                        Assert.IsTrue(gui.Application.Project.RootFolder.GetAllModelsRecursive().SequenceEqual(new[] {waterFlowFMModel}));
                    }
                }
            };
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenAnIntegratedModelWithFMModelInItWhenOpeningGridInRGFGridAndClosingItThenItShouldNotThowAnException()
        {
            Action mainWindowShown = delegate
            {
                using (var integratedModel = new HydroModelBuilder().BuildModel(ModelGroup.FMWaveRtcModels))
                {
                    gui.CommandHandler.AddItemToProject(integratedModel);
                    gui.Selection = integratedModel;
                    gui.CommandHandler.OpenViewForSelection();
                    var waterFlowFMModel = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().First();
                    waterFlowFMModel.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(50, 50, 20, 20);
                    waterFlowFMModel.ReloadGrid();
                    gui.Selection = waterFlowFMModel.Grid;
                    PerformActionWithCancellationThread(60000, () => gui.CommandHandler.OpenViewForSelection());
                }
            };
            WpfTestHelper.ShowModal((MainWindow)gui.MainWindow, mainWindowShown);
        }

        private static void PerformActionWithCancellationThread(int timeout, Action action)
        {
            // Action waits for rgfgrid to close, we do this manually from another thread
            var cancellationThread = new Thread(() => CloseRgfGrid(timeout));
            cancellationThread.Start();

            // Invoke action
            action.Invoke();
        }

        private static void CloseRgfGrid(int maxTimeout)
        {
            Thread.Sleep(500); // Give action time to get started
            const int millisecondsToSleep = 100;

            // Get active rgfGrid processes (there should only be one)
            var rgfGridProcesses = Process.GetProcessesByName(RgfGridEditor.MfeAppProcessName);
            while (!rgfGridProcesses.Any())
            {
                Thread.Sleep(millisecondsToSleep);
                rgfGridProcesses = Process.GetProcessesByName(RgfGridEditor.MfeAppProcessName);
            }

            foreach (var process in rgfGridProcesses)
            {
                var totalTimeWaiting = 0;
                // attempt to close rgfGrid (may not be successful straight away)
                while (!process.CloseMainWindow())
                {
                    totalTimeWaiting += millisecondsToSleep;
                    Thread.Sleep(millisecondsToSleep);

                    if (totalTimeWaiting > maxTimeout)
                    {
                        return;
                    }
                }
            }
        }
        private static HydroModel CreateFMRTCModel(out RealTimeControlModel rtc, out WaterFlowFMModel flow, IApplication application)
        {
            flow = new WaterFlowFMModel { Name = "flow" };
            rtc = new RealTimeControlModel("rtc");

            var hydroModel = new HydroModel { Activities = { rtc, flow } };
            application.Project.RootFolder.Add(hydroModel);

            hydroModel.WorkingDirectoryPathFunc = ()=> Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()));
            
            hydroModel.StopTime = hydroModel.StartTime.AddHours(0.5);
            hydroModel.TimeStep = new TimeSpan(0, 0, 1, 0);

            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(20, 20, 10.0, 10.0); //200 m x 200 
            flow.Grid = grid;
            flow.ReloadGrid();
            flow.ModelDefinition.GetModelProperty("MapOutputDeltaT").Value = new TimeSpan(0, 0, 1, 0);
            flow.ModelDefinition.GetModelProperty("HisOutputDeltaT").Value = new TimeSpan(0, 0, 1, 0);
            flow.ModelDefinition.GetModelProperty("RstOutputDeltaT").Value = new TimeSpan(0, 0, 1, 0);

            flow.Boundaries.Add(new Feature2D
            {
                Name = "bnd1",
                Geometry =
                    new LineString(new[] { new Coordinate(-5, 0), new Coordinate(-5, 100), new Coordinate(-5, 200) })
            });
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.TimeSeries) { Feature = flow.Boundaries.First() };
            flowBoundaryCondition.AddPoint(0);
            var timeSeries = flowBoundaryCondition.PointData[0];
            timeSeries.Arguments[0].SetValues(new[] { flow.StartTime, flow.StopTime });
            timeSeries.Components[0].SetValues(new[] { 100.0, 100.0 });
            flow.BoundaryConditionSets[0].BoundaryConditions.Add(flowBoundaryCondition);

            flow.Area.FixedWeirs.Add(new FixedWeir
            {
                Name = "fxw",
                Geometry = new LineString(new[] { new Coordinate(100, 0), new Coordinate(100, 200) })
            });
            
            flow.FixedWeirsProperties[0].DataColumns[0].ValueList[0] = 10.0;
            flow.FixedWeirsProperties[0].DataColumns[0].ValueList[1] = 10.0;

            flow.Area.Weirs.Add(new Weir2D("weir")
            {
                Geometry = new LineString(new[] { new Coordinate(99, 90), new Coordinate(99, 110) }),
                CrestLevel = 2.5
            });
            flow.Area.ObservationPoints.Add(new GroupableFeature2DPoint { Name = "obs", Geometry = new Point(50, 50) });
            
            var controlGroup = RealTimeControlModelHelper.CreateGroupRelativeTimeRule();
            rtc.ControlGroups.Add(controlGroup);
            var relativeTimeRule = ((RelativeTimeRule)controlGroup.Rules[0]);
            relativeTimeRule.FromValue = false;
            relativeTimeRule.Function[0.0] = 2.5;
            relativeTimeRule.Function[100.0] = 0.0;
            relativeTimeRule.Function[200.0] = -2.0;
            relativeTimeRule.Function[500.0] = -2.0;
            relativeTimeRule.Interpolation = InterpolationType.Linear;
            ((StandardCondition)controlGroup.Conditions[0]).Operation = Operation.Greater;
            controlGroup.Conditions[0].Value = 2.5;

            var outputDataItem = flow.GetChildDataItems(flow.Area.ObservationPoints[0]).First();
            rtc.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem);

            var inputDataItem = flow.GetChildDataItems(flow.Area.Weirs[0]).First();
            inputDataItem.LinkTo(rtc.GetDataItemByValue(controlGroup.Outputs[0]));

            return hydroModel;
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void FmModelShouldBeReplacedWhenImportedInIntegratedModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            /* create a integrated model */
            var hydroModel = HydroModel.BuildModel(ModelGroup.FMWaveRtcModels);
            var project = app.Project;

            /* add it to you project */
            project.RootFolder.Add(hydroModel);

            // wait until gui starts
            Action mainWindowShown = delegate
            {
                /* get the water flow fm model */
                var waterFlowFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.NotNull(waterFlowFmModel);
                Assert.IsTrue(waterFlowFmModel.Name.StartsWith("FlowFM"));

                var fmImporter = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                Assert.IsNotNull(fmImporter);
                fmImporter.ImportItem(mduPath, waterFlowFmModel);

                var targetFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(targetFmModel);
                Assert.IsTrue(targetFmModel.Name.StartsWith("har"));
            };
            WpfTestHelper.ShowModal((MainWindow)gui.MainWindow, mainWindowShown);
        }
    }
}
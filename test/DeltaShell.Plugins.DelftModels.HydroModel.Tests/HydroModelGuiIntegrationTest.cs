using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Gui.Forms.MainWindow;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMapTestUtils;
using ComboBox = System.Windows.Controls.ComboBox;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
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
            new RunningActivityLogAppender();
            //HACK: inside this constructor singleton magic happens, this should not be required

            gui = new DeltaShellGui();
            app = gui.Application;
            
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());
            app.Plugins.Add(new WaterQualityModelApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new WaveApplicationPlugin());

            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new HydroModelGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new RainfallRunoffGuiPlugin());
            gui.Plugins.Add(new FlowFMGuiPlugin());
            gui.Plugins.Add(new WaveGuiPlugin());

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

            hydroModel.Region.SubRegions.OfType<DrainageBasin>()
                      .First()
                      .Catchments.AddRange(new[] {paved, unpaved, green, water, sacr, hbv});

            app.Project.RootFolder.Add(hydroModel);
            var filename = TestHelper.GetCurrentMethodName();

            app.ExportProjectItem(hydroModel, filename + "_1.dsproj", true);

            app.SaveProjectAs("_2.dsproj"); //bang, exception!
        }

        /// <summary>
        /// TOOLS-22846 dictates as issue that the combobox is not filled after a route layer is added to network
        /// this test will verify that the combo box could get updated when in a
        /// sobek model the computational grid layer is activated.
        /// </summary>
        [Test]
        [Category(TestCategory.Slow)]
        public void EmptyCoverageDropdownBoxGetsUpdatedAfterCoverageGridSelection()
        {
            /* create a integrated model */
            var hydroModel = HydroModel.BuildModel(ModelGroup.SobekModels);
            var project = app.Project;

            /* add it to you project */
            project.RootFolder.Add(hydroModel);

            var mainWindow = (MainWindow)gui.MainWindow;

            // wait until gui starts
            mainWindow.Loaded += delegate
            {
                /* get the water flow 1d model */
                var model = hydroModel.Activities.OfType<WaterFlowModel1D>().First() as IModel;
                Assert.NotNull(model);
                
                /* get the computational grid of the waterflow 1d model */
                var discretization = model.DataItems.Select(di => di.Value).OfType<IDiscretization>().First();
                
                /* get the network of the waterflow 1d model */
                var network = model.DataItems.Select(di => di.Value).OfType<IHydroNetwork>().First();

                /* open the view for the integrated model (so the 'Map' tab is enabled which contains the coverage combobox) */
                gui.CommandHandler.OpenView(hydroModel, typeof(ProjectItemMapView));

                /* get the coverages combo box from te ribbon */
                var ribbon = (Fluent.Ribbon)TypeUtils.GetField(mainWindow, "MainWindowRibbon");
                var tab = ribbon.Tabs.First(t => t.Header.Equals("Map"));
                var group = tab.Groups.First(g => g.Name.Equals("NetworkCoverage"));
                var wrapPanel = group.Items.OfType<WrapPanel>().First();
                var comboBox = wrapPanel.Children.OfType<ComboBox>().First(c => c.Name == "ComboBoxSelectNetworkCoverage");

                Assert.NotNull(comboBox);
                
                /* attach event to see how many times the selected item attibute is set */
                var count = 0;
                comboBox.SelectionChanged += (s, e) => { count++; };
                
                /* check if combobox is still empty! */
                Assert.IsFalse(comboBox.HasItems);

                /* add simple branch */
                var node1 = new HydroNode() {Geometry = new Point(0, 0)};
                var node2 = new HydroNode() {Geometry = new Point(1000, 0)};

                var channel = new Channel(node1, node2)
                {
                    Geometry = new LineString(new[] {node1.Geometry.Coordinate, node2.Geometry.Coordinate})
                };
                network.Nodes.AddRange(new []{node1, node2});
                network.Branches.Add(channel);

                /* make computational grid layer visible */
                var projectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().First();
                projectItemMapView.EnsureVisible(projectItemMapView.MapView.GetLayerForData(discretization));
                mainWindow.ValidateItems();

                /* check if the computational grid is added to the combobox as ONLY one */
                Assert.AreEqual(1, count);
                Assert.IsTrue(comboBox.HasItems);
                Assert.AreEqual(1, comboBox.Items.Count);
                var layer = comboBox.Items[0] as Layer;
                Assert.IsNotNull(layer);
                Assert.AreEqual("Computational Grid", layer.Name);

                /* validate that the computational grid is the selected item */
                var selected = comboBox.SelectedItem as Layer;
                Assert.IsNotNull(selected);
                Assert.AreEqual("Computational Grid", selected.Name);
                
            };
            WpfTestHelper.ShowModal(mainWindow);
        }

        /// <summary>
        /// TOOLS-22846 dictates as issue that the combobox is not filled after a route layer is added to network
        /// this test will verify that the combo box could get updated when in a
        /// sobek model the computational grid layer is activated and then a route is added.
        /// </summary>
        [Test]
        [Category(TestCategory.Slow)]
        public void CoverageDropdownBoxWithComputationalGridSelectedGetsUpdatedAfterRouteAdd_22846()
        {
            // create a integrated model
            var hydroModel = HydroModel.BuildModel(ModelGroup.SobekModels);
            var project = app.Project;

            // add it to you project
            project.RootFolder.Add(hydroModel);

            var mainWindow = (MainWindow)gui.MainWindow;

            // wait until gui starts
            mainWindow.Loaded += delegate
            {
                // get the water flow 1d model
                var model = hydroModel.Activities.OfType<WaterFlowModel1D>().First() as IModel;
                Assert.NotNull(model);

                // get the computational grid of the waterflow 1d model
                var discretization = model.DataItems.Select(di => di.Value).OfType<IDiscretization>().First();
                
                // get the network of the waterflow 1d model
                var network = model.DataItems.Select(di => di.Value).OfType<IHydroNetwork>().First();

                // open the view for the integrated model (so the 'Map' tab is enabled which contains the coverage combobox)
                gui.CommandHandler.OpenView(hydroModel, typeof(ProjectItemMapView));

                // get the coverages combo box from te ribbon */
                var ribbon = (Fluent.Ribbon)TypeUtils.GetField(mainWindow, "MainWindowRibbon");
                var tab = ribbon.Tabs.First(t => t.Header.Equals("Map"));
                var group = tab.Groups.First(g => g.Name.Equals("NetworkCoverage"));
                var wrapPanel = group.Items.OfType<WrapPanel>().First();
                var comboBox = wrapPanel.Children.OfType<ComboBox>().First(c => c.Name == "ComboBoxSelectNetworkCoverage");

                Assert.NotNull(comboBox);

                // attach event to see how many times the selected item attibute is set
                var count = 0;
                comboBox.SelectionChanged += (s, e) => { count++; };

                // check if combobox is still empty!
                Assert.IsFalse(comboBox.HasItems);

                // add simple branch
                var node1 = new HydroNode() { Geometry = new Point(0, 0) };
                var node2 = new HydroNode() { Geometry = new Point(1000, 0) };

                var channel = new Channel(node1, node2)
                {
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };
                network.Nodes.AddRange(new[] { node1, node2 });
                network.Branches.Add(channel);

                // make computational grid layer visible
                var projectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().First();
                projectItemMapView.EnsureVisible(projectItemMapView.MapView.GetLayerForData(discretization));
                mainWindow.ValidateItems();

                // check if the computational grid is added to the combobox as ONLY one
                Assert.AreEqual(1, count);
                Assert.IsTrue(comboBox.HasItems);
                Assert.AreEqual(1, comboBox.Items.Count);
                var layer = comboBox.Items[0] as Layer;
                Assert.IsNotNull(layer);
                Assert.AreEqual("Computational Grid", layer.Name);

                // add a route to the network
                new AddNewNetworkRouteCommand().Execute();

                // check if the route (route_1) is added to the combobox, this is selected and that componational grid is also in the combobox list
                Assert.AreEqual(3, count); // why 3x???
                Assert.IsTrue(comboBox.HasItems);
                Assert.AreEqual(2, comboBox.Items.Count);
                var routeLayer = comboBox.Items[0] as Layer;
                Assert.IsNotNull(routeLayer);
                Assert.AreEqual("route_1", routeLayer.Name);
                var cgLayer = comboBox.Items[1] as Layer;
                Assert.IsNotNull(cgLayer);
                Assert.AreEqual("Computational Grid", cgLayer.Name);

                // validate that the just added route is the selected item
                var selected = comboBox.SelectedItem as Layer;
                Assert.IsNotNull(selected);
                Assert.AreEqual("route_1", selected.Name);
                
            };
            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ShowFMModelRunCoupledToRTC()
        {
            WaterFlowFMModel flow;
            RealTimeControlModel rtc;
            var hydroModel = CreateFMRTCModel(out rtc, out flow, app);
            var mainWindow = (MainWindow)gui.MainWindow;

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
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        private IEnumerable<IFileExporter> GetApplicationFileExportersForDimr()
        {
            return app.Plugins.SelectMany(p => p.GetFileExporters()).Plus(new Iterative1D2DCouplerExporter());
        }

        [Test]
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
        public void GivenAnIntegratedModelWithFMModelInItWhenOpeningGridInRGFGridAndClosingItThenItShouldNotThowAnException()
        {
            var mainWindow = (MainWindow)gui.MainWindow;

            Action mainWindowShown = delegate
            {
                var hydroModelBuilder = new HydroModelBuilder();
                using (var integratedModel = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels))
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
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
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

            hydroModel.ExplicitWorkingDirectory = Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()));
            
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
            
            flow.FixedWeirsProperties.ElementAt(0).DataColumns[0].ValueList[0] = 10.0;
            flow.FixedWeirsProperties.ElementAt(0).DataColumns[0].ValueList[1] = 10.0;

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
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string modelName = "har";
                var mduPath = Path.Combine(tempDir, $"{modelName}.mdu");
                var fmModel = new WaterFlowFMModel(mduPath);
                fmModel.ExportTo(mduPath, false, false, false);

                /* create a integrated model */
                var hydroModel = HydroModel.BuildModel(ModelGroup.FMWaveRtcModels);
                var project = app.Project;

                /* add it to you project */
                project.RootFolder.Add(hydroModel);

                var mainWindow = (MainWindow)gui.MainWindow;

                // wait until gui starts
                mainWindow.Loaded += delegate
                {
                    /* get the water flow fm model */
                    var waterFlowFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.NotNull(waterFlowFmModel);
                    Assert.That(waterFlowFmModel.Name, Is.StringContaining("FlowFM"));

                    var fmImporter = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(fmImporter);
                    fmImporter.ImportItem(mduPath, waterFlowFmModel);

                    var targetFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetFmModel);
                    Assert.That(targetFmModel.Name, Is.StringContaining(modelName));
                };
                WpfTestHelper.ShowModal(mainWindow);
            });
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void WaveModelShouldBeReplacedWhenImportedInIntegratedModel()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"waveFlowFM\wave\te0.mdw");
            mdwPath = TestHelper.CreateLocalCopy(mdwPath);

            /* create a integrated model */
            var hydroModel = HydroModel.BuildModel(ModelGroup.FMWaveRtcModels);
            var project = app.Project;

            /* add it to you project */
            project.RootFolder.Add(hydroModel);

            var mainWindow = (MainWindow)gui.MainWindow;
                
            // wait until gui starts
            Action mainWindowShown = delegate
            {
               /* get the wave model */
                var waveModel = hydroModel.Activities.OfType<WaveModel>().FirstOrDefault();
                Assert.NotNull(waveModel);
                Assert.That(waveModel.Name, Is.StringContaining("Waves"));

                var waveImporter = app.FileImporters.OfType<WaveModelFileImporter>().FirstOrDefault();
                Assert.IsNotNull(waveImporter);
                waveImporter.ImportItem(mdwPath, waveModel);

                var targetWaveModel = hydroModel.Activities.OfType<WaveModel>().FirstOrDefault();
                Assert.IsNotNull(targetWaveModel);
                Assert.That(targetWaveModel.Name, Is.StringContaining("te0"));
            };
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);


        }
    }
}
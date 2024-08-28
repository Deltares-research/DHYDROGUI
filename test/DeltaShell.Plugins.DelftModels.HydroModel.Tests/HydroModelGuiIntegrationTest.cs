using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Gui.Forms.MainWindow;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Wpf)]
    public class HydroModelGuiIntegrationTest
    {
        private IGui gui;
        private IApplication app;
        private Project project;

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

        [Test]
        [Category(TestCategory.Slow)]
        public void ShowFmModelRunCoupledToRtc()
        {
            HydroModel hydroModel = CreateFmRtcModel(out WaterFlowFMModel flow);
            var mainWindow = (MainWindow) gui.MainWindow;

            // wait until gui starts
            void MainWindowShown()
            {
                ActivityRunner.RunActivity(hydroModel);
                Assert.AreNotEqual(ActivityStatus.Failed, hydroModel.Status);
                gui.Selection = flow;
                gui.CommandHandler.OpenViewForSelection(typeof(ProjectItemMapView));
                var view = gui.DocumentViews.ActiveView as ProjectItemMapView;
                ILayer velocityLayer = view.MapView.Map.GetAllLayers(true).FirstOrDefault(l => l.Name.Contains("x-component"));
                Assert.IsNotNull(velocityLayer);
                velocityLayer.Visible = true;
                TimeSeriesNavigator timeSeriesNavigator = gui.ToolWindowViews.OfType<TimeSeriesNavigator>().First();
                for (var i = 0; i < 20; ++i)
                {
                    timeSeriesNavigator.SelectNextTime();
                }

                Console.WriteLine("1");
            }

            WpfTestHelper.ShowModal(mainWindow, MainWindowShown);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Add2D3DIntegratedModelAddFmModelRemoveIntegratedModel()
        {
            var mainWindow = (MainWindow) gui.MainWindow;

            if (!gui.DocumentViewsResolver.DefaultViewTypes.ContainsKey(typeof(WaterFlowFMModel)))
            {
                gui.DocumentViewsResolver.DefaultViewTypes.Add(typeof(WaterFlowFMModel), typeof(WaterFlowFMFileStructureView));
            }

            void MainWindowShown()
            {
                var hydroModelBuilder = new HydroModelBuilder();
                using (HydroModel integratedModel2D3D = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels))
                {
                    using (var waterFlowFmModel = new WaterFlowFMModel())
                    {
                        gui.CommandHandler.AddItemToProject(integratedModel2D3D);
                        gui.Selection = integratedModel2D3D;
                        gui.CommandHandler.OpenViewForSelection();
                        gui.CommandHandler.AddItemToProject(waterFlowFmModel);
                        gui.Selection = waterFlowFmModel;
                        gui.CommandHandler.OpenViewForSelection();
                        project.RootFolder.Items.Remove(integratedModel2D3D);
                        Assert.IsTrue(project.RootFolder.GetAllModelsRecursive()
                                             .SequenceEqual(new[]
                                             {
                                                 waterFlowFmModel
                                             }));
                    }
                }
            }

            WpfTestHelper.ShowModal(mainWindow, MainWindowShown);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void GivenAnIntegratedModelWithFmModelInItWhenOpeningGridInRgfGridAndClosingItThenItShouldNotThrowAnException()
        {
            var mainWindow = (MainWindow) gui.MainWindow;

            void MainWindowShown()
            {
                var hydroModelBuilder = new HydroModelBuilder();
                using (HydroModel integratedModel = hydroModelBuilder.BuildModel(ModelGroup.FMWaveRtcModels))
                {
                    gui.CommandHandler.AddItemToProject(integratedModel);
                    gui.Selection = integratedModel;
                    gui.CommandHandler.OpenViewForSelection();
                    WaterFlowFMModel waterFlowFmModel = project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>().First();
                    waterFlowFmModel.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(50, 50, 20, 20);
                    waterFlowFmModel.ReloadGrid();
                    gui.Selection = waterFlowFmModel.Grid;
                    PerformActionWithCancellationThread(60000, () => gui.CommandHandler.OpenViewForSelection());
                }
            }

            WpfTestHelper.ShowModal(mainWindow, MainWindowShown);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void FmModelShouldBeReplacedWhenImportedInIntegratedModel()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string modelName = "har";
                string mduPath = Path.Combine(tempDir, $"{modelName}.mdu");

                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(mduPath);

                fmModel.ExportTo(mduPath, false, false, false);

                /* create a integrated model */
                var hydroModel = HydroModel.BuildModel(ModelGroup.FMWaveRtcModels);

                /* add it to you project */
                project.RootFolder.Add(hydroModel);

                var mainWindow = (MainWindow) gui.MainWindow;

                // wait until gui starts
                void MainWindowShown()
                {
                    /* get the water flow fm model */
                    WaterFlowFMModel waterFlowFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.NotNull(waterFlowFmModel);
                    Assert.That(waterFlowFmModel.Name, Does.Contain("FlowFM"));

                    WaterFlowFMFileImporter fmImporter = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(fmImporter);
                    fmImporter.ImportItem(mduPath, waterFlowFmModel);

                    WaterFlowFMModel targetFmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetFmModel);
                    Assert.That(targetFmModel.Name, Does.Contain(modelName));
                }

                WpfTestHelper.ShowModal(mainWindow, MainWindowShown);
            });
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void WaveModelShouldBeReplacedWhenImportedInIntegratedModel()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"waveFlowFM\wave\te0.mdw");
            mdwPath = TestHelper.CreateLocalCopy(mdwPath);

            /* create a integrated model */
            var hydroModel = HydroModel.BuildModel(ModelGroup.FMWaveRtcModels);

            /* add it to you project */
            project.RootFolder.Add(hydroModel);

            var mainWindow = (MainWindow) gui.MainWindow;

            // wait until gui starts
            void MainWindowShown()
            {
                /* get the wave model */
                WaveModel waveModel = hydroModel.Activities.OfType<WaveModel>().FirstOrDefault();
                Assert.NotNull(waveModel);
                Assert.That(waveModel.Name, Does.Contain("Waves"));

                WaveModelFileImporter waveImporter = app.FileImporters.OfType<WaveModelFileImporter>().FirstOrDefault();
                Assert.IsNotNull(waveImporter);
                waveImporter.ImportItem(mdwPath, waveModel);

                WaveModel targetWaveModel = hydroModel.Activities.OfType<WaveModel>().FirstOrDefault();
                Assert.IsNotNull(targetWaveModel);
                Assert.That(targetWaveModel.Name, Does.Contain("te0"));
            }

            WpfTestHelper.ShowModal(mainWindow, MainWindowShown);
        }

        private void InitializeGui()
        {
            new RunningActivityLogAppender();
            //HACK: inside this constructor singleton magic happens, this should not be required

            gui = new DHYDROGuiBuilder().WithHydroModel().WithRealTimeControl().WithWaterQuality().WithFlowFM().WithWaves().Build();
            app = gui.Application;
            gui.Run();
            project = gui.Application.ProjectService.CreateProject();
        }

        private void DisposeGui()
        {
            gui.Dispose();

            gui = null;
            app = null;
        }

        private static void PerformActionWithCancellationThread(int timeout, Action action)
        {
            // Action waits for rgf grid to close, we do this manually from another thread
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
            Process[] rgfGridProcesses = Process.GetProcessesByName(RgfGridEditor.MfeAppProcessName);
            while (!rgfGridProcesses.Any())
            {
                Thread.Sleep(millisecondsToSleep);
                rgfGridProcesses = Process.GetProcessesByName(RgfGridEditor.MfeAppProcessName);
            }

            foreach (Process process in rgfGridProcesses)
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

        private HydroModel CreateFmRtcModel(out WaterFlowFMModel flow)
        {
            flow = new WaterFlowFMModel {Name = "flow"};
            var rtc = new RealTimeControlModel("rtc");

            var hydroModel = new HydroModel
            {
                Activities =
                {
                    rtc,
                    flow
                }
            };
            project.RootFolder.Add(hydroModel);

            hydroModel.StopTime = hydroModel.StartTime.AddHours(0.5);
            hydroModel.TimeStep = new TimeSpan(0, 0, 1, 0);

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(20, 20, 10.0, 10.0); //200 m x 200 
            flow.Grid = grid;
            flow.ReloadGrid();
            flow.ModelDefinition.GetModelProperty("MapOutputDeltaT").Value = new TimeSpan(0, 0, 1, 0);
            flow.ModelDefinition.GetModelProperty("HisOutputDeltaT").Value = new TimeSpan(0, 0, 1, 0);
            flow.ModelDefinition.GetModelProperty("RstOutputDeltaT").Value = new TimeSpan(0, 0, 1, 0);

            flow.Boundaries.Add(new Feature2D
            {
                Name = "bnd1",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(-5, 0),
                        new Coordinate(-5, 100),
                        new Coordinate(-5, 200)
                    })
            });
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                                                                  BoundaryConditionDataType.TimeSeries) {Feature = flow.Boundaries.First()};
            flowBoundaryCondition.AddPoint(0);
            IFunction timeSeries = flowBoundaryCondition.PointData[0];
            timeSeries.Arguments[0].SetValues(new[]
            {
                flow.StartTime,
                flow.StopTime
            });
            timeSeries.Components[0].SetValues(new[]
            {
                100.0,
                100.0
            });
            flow.BoundaryConditionSets[0].BoundaryConditions.Add(flowBoundaryCondition);

            flow.Area.FixedWeirs.Add(new FixedWeir
            {
                Name = "fxw",
                Geometry = new LineString(new[]
                {
                    new Coordinate(100, 0),
                    new Coordinate(100, 200)
                })
            });

            flow.FixedWeirsProperties.ElementAt(0).DataColumns[0].ValueList[0] = 10.0;
            flow.FixedWeirsProperties.ElementAt(0).DataColumns[0].ValueList[1] = 10.0;

            flow.Area.Structures.Add(new Structure()
            {
                Name = "weir",
                Geometry = new LineString(new[]
                {
                    new Coordinate(99, 90),
                    new Coordinate(99, 110)
                }),
                CrestLevel = 2.5,
                CrestWidth = 1
            });
            flow.Area.ObservationPoints.Add(new GroupableFeature2DPoint
            {
                Name = "obs",
                Geometry = new Point(50, 50)
            });

            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupRelativeTimeRule();
            rtc.ControlGroups.Add(controlGroup);
            var relativeTimeRule = (RelativeTimeRule) controlGroup.Rules[0];
            relativeTimeRule.FromValue = false;
            relativeTimeRule.Function[0.0] = 2.5;
            relativeTimeRule.Function[100.0] = 0.0;
            relativeTimeRule.Function[200.0] = -2.0;
            relativeTimeRule.Function[500.0] = -2.0;
            relativeTimeRule.Interpolation = InterpolationType.Linear;
            ((StandardCondition) controlGroup.Conditions[0]).Operation = Operation.Greater;
            controlGroup.Conditions[0].Value = 2.5;

            IDataItem outputDataItem = flow.GetChildDataItems(flow.Area.ObservationPoints[0]).First();
            rtc.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem);

            IDataItem inputDataItem = flow.GetChildDataItems(flow.Area.Structures[0]).First();
            inputDataItem.LinkTo(rtc.GetDataItemByValue(controlGroup.Outputs[0]));

            return hydroModel;
        }
    }
}
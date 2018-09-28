using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Core;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;


namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class RealTimeControlModelRestartIntegrationTest : NHibernateIntegrationTestBase
    {
        [Test]
        [Ignore("No such things as RTC outputs with DIMR (so far)")]
        public void OpenProjectAndVerify()
        {
            var projectRepository = factory.CreateNew();
            var legacyPath = TestHelper.GetTestFilePath(@"RtcFlow1DRestart\RtcWithPidFlow1DRestart.dsproj");
            var localLegacyPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
            var project = projectRepository.Open(localLegacyPath);
            var hydroModel = (HydroModel)project.RootFolder.Models.First();
            hydroModel.ExplicitWorkingDirectory = Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()));

            var fullRunStartTime = hydroModel.StartTime;
            var fullRunStopTime = hydroModel.StopTime;
            var rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().First();
            var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();

            flowModel.OutputSettings.GetEngineParameter(QuantityType.FiniteGridType,
               ElementSet.FiniteVolumeGridOnGridPoints).AggregationOptions = (int)FiniteVolumeDiscretizationType.None;

            var originalDirectory = Environment.CurrentDirectory;
            
            ActivityRunner.RunActivity(hydroModel);
            Environment.CurrentDirectory = originalDirectory;

            var fullRunRtcCrestLevel = rtcModel.OutputFeatureCoverages.First().Components[0].Values.OfType<double>().ToArray();

            // run again for first half of time, and write restart
            var timeSpan = fullRunStopTime - fullRunStartTime;
            hydroModel.StopTime = fullRunStartTime.AddHours(timeSpan.TotalHours/2);
            rtcModel.WriteRestart = true;
            flowModel.WriteRestart = true;
            ActivityRunner.RunActivity(hydroModel);
            var halfWayStateRtc = (FileBasedRestartState)rtcModel.GetRestartOutputStates().Last().Clone();
            var halfWayStateFlow = (FileBasedRestartState)flowModel.GetRestartOutputStates().Last().Clone();

            var firstHalfRtcCrestLevel = rtcModel.OutputFeatureCoverages.First().Components[0].Values.OfType<double>().ToArray();

            // run for second half of time, using restart from previous run
            hydroModel.StartTime = fullRunStartTime.AddHours(timeSpan.TotalHours/2);
            hydroModel.StopTime = fullRunStopTime;
            rtcModel.UseRestart = true;
            flowModel.UseRestart = true;
            rtcModel.RestartInput = halfWayStateRtc;
            flowModel.RestartInput = halfWayStateFlow;
            ActivityRunner.RunActivity(hydroModel);

            var secondHalfRtcCrestLevel = rtcModel.OutputFeatureCoverages.First().Components[0].Values.OfType<double>().ToArray();

            var crestLevelCombined = firstHalfRtcCrestLevel.Concat(secondHalfRtcCrestLevel).ToArray();

            Assert.IsTrue(fullRunRtcCrestLevel.Length > 0);
            Assert.IsTrue(fullRunRtcCrestLevel.Length == crestLevelCombined.Length); 
            for (var i = 0; i < fullRunRtcCrestLevel.Length; i++)
            {
                Assert.AreEqual(fullRunRtcCrestLevel[i], crestLevelCombined[i], 0.0001);
            }
        }

        [TestCase(24, 24, 4, 0, TestName = "GivenAnIntegratedModelWithStateSavesFor1DAndRTCMatchingTheRunPeriodWhenThisModelRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun")]
        [TestCase(24, 16, 4, 4, TestName = "GivenAnIntegratedModelWithStateSavesFor1DAndRTCMatchingASubsetOfTheRunPeriodWhenThisRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun")]
        public void GivenAnIntegratedModelWithStateSavesFor1DAndRTCWhenThisModelIsRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun(int runLengthInHours, int runLengthSaveStateInHours, int intervalSaveStateInHours, int offsetSaveStateInHours)
        {
            const string projectName = "TestRTC1D.dsproj";

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Get project files in temp folder.
                var projectPath = Path.Combine(tempDir, projectName);

                // Set up application.
                using (var app = new DeltaShellApplication())
                {
                    app.IsProjectCreatedInTemporaryDirectory = true;
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new NetCdfApplicationPlugin());
                    app.Plugins.Add(new RealTimeControlApplicationPlugin());
                    app.Plugins.Add(new HydroModelApplicationPlugin());
                    app.Plugins.Add(new NetCdfApplicationPlugin());

                    app.Run();

                    app.SaveProjectAs(projectPath);

                    // Given
                    var hydroModel = createSimpleHydroModel(projectPath + "_data");
                    var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
                    var rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().First();

                    app.Project.RootFolder.Add(hydroModel);

                    var fullRunStartTime = DateTime.Today;
                    var fullRunStopTime = fullRunStartTime.AddHours(runLengthInHours);

                    // Set up time ranges on HydroModel
                    hydroModel.StartTime = fullRunStartTime;
                    hydroModel.StopTime = fullRunStopTime;
                    hydroModel.TimeStep = TimeSpan.FromHours(1);

                    // Set up time ranges on WaterFlowModel1D.
                    flowModel.WriteRestart = true;
                    flowModel.UseRestart = false;
                    flowModel.StartTime = fullRunStartTime;
                    flowModel.StopTime = fullRunStopTime;
                    flowModel.TimeStep = TimeSpan.FromHours(1);

                    flowModel.UseSaveStateTimeRange = true;
                    flowModel.SaveStateStartTime = flowModel.StartTime.AddHours(offsetSaveStateInHours);
                    flowModel.SaveStateStopTime = flowModel.SaveStateStartTime.AddHours(runLengthSaveStateInHours);
                    flowModel.SaveStateTimeStep = TimeSpan.FromHours(intervalSaveStateInHours);

                    // Set up time ranges on RealTimeControlModel.
                    rtcModel.WriteRestart = true;
                    rtcModel.UseRestart = false;
                    rtcModel.StartTime = fullRunStartTime;
                    rtcModel.StopTime = fullRunStopTime;

                    rtcModel.UseSaveStateTimeRange = true;
                    rtcModel.SaveStateStartTime = rtcModel.StartTime.AddHours(offsetSaveStateInHours);
                    rtcModel.SaveStateStopTime = rtcModel.SaveStateStartTime.AddHours(runLengthSaveStateInHours);
                    rtcModel.SaveStateTimeStep = TimeSpan.FromHours(intervalSaveStateInHours);

                    // When
                    //   Run initial model to generate restart values.
                    ActivityRunner.RunActivity(hydroModel);
                    Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                    // Obtain output values generated by non-restarted run.
                    var rtcCrestLevelFullRun = rtcModel.OutputFeatureCoverages.First().Components[0].Values.OfType<double>().ToList();

                    flowModel.WriteRestart = false;
                    rtcModel.WriteRestart = false;

                    app.SaveProject();
                    app.CloseProject();

                    // Calculate number of restart states to evaluate.
                    var nRestartStates = (runLengthSaveStateInHours / intervalSaveStateInHours);
                    var lastRestartStateOverlapsWithStop =
                        fullRunStopTime.Equals(
                            fullRunStartTime.AddHours(offsetSaveStateInHours + nRestartStates * intervalSaveStateInHours));

                    if (lastRestartStateOverlapsWithStop)
                        nRestartStates -= 1;

                    // Do restarts for each of the restart files.
                    for (var i = 0; i < nRestartStates; i++)
                    {
                        // Open projects and obtain models.
                        app.OpenProject(projectPath);

                        hydroModel = (HydroModel) app.Project.RootFolder.Models.First();
                        rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().First();
                        flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();

                        // Set up restart.
                        var restartStateFlowModel =
                            (FileBasedRestartState) flowModel.GetRestartOutputStates().ElementAt(i).Clone();

                        flowModel.StartTime = restartStateFlowModel.SimulationTime;
                        flowModel.StopTime = fullRunStopTime;
                        flowModel.UseRestart = true;
                        flowModel.RestartInput = restartStateFlowModel;

                        var restartStateRtcModel =
                            (FileBasedRestartState) rtcModel.GetRestartOutputStates().ElementAt(i).Clone();

                        rtcModel.StartTime = restartStateRtcModel.SimulationTime;
                        rtcModel.StopTime = fullRunStopTime;
                        rtcModel.UseRestart = true;
                        rtcModel.RestartInput = restartStateRtcModel;

                        // When
                        // Run with restart.
                        ActivityRunner.RunActivity(hydroModel);
                        Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                        // Obtain data to compare to.
                        var rtcCrestLevelRestart =
                            rtcModel.OutputFeatureCoverages.First().Components[0].Values.OfType<double>().ToArray();
                        var rtcCrestLevelFullRunSubset = rtcCrestLevelFullRun
                            .Skip(rtcCrestLevelFullRun.Count - rtcCrestLevelRestart.Length)
                            .ToArray();

                        for (var indexCrestLevel = 0; indexCrestLevel < rtcCrestLevelRestart.Length; indexCrestLevel++)
                        {
                            // Then
                            Assert.AreEqual(rtcCrestLevelRestart[indexCrestLevel],
                                            rtcCrestLevelFullRunSubset[indexCrestLevel],
                                            0.0001);
                        }
                        app.CloseProject();
                    }
                }
            });
        }

        private IEnumerable<IFileExporter> GetFactoryFileExportersForDimr()
        {
            return factory.SessionProvider.ConfigurationProvider.Plugins.OfType<ApplicationPlugin>().SelectMany(p => p.GetFileExporters()).Plus(new Iterative1D2DCouplerExporter());
        }

        private static HydroModel createSimpleHydroModel(string projectDataPath)
        {
            // Set up hydro model
            var hydroModel = new HydroModel() {Name = "Integrated Model"};

            var flowModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            hydroModel.Region.SubRegions.Add(flowModel.Network);

            var rtcModel = new RealTimeControlModel("Real-Time Control");

            // Add models to hydromodel
            hydroModel.Activities.Add(flowModel);
            hydroModel.Activities.Add(rtcModel);

            // Set up flow model
            flowModel.BoundaryConditions[0].DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            flowModel.BoundaryConditions[0].Flow = 100.0;
            flowModel.BoundaryConditions[1].DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            flowModel.BoundaryConditions[1].WaterLevel = 0.0;
            flowModel.BoundaryConditions[2].DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            flowModel.BoundaryConditions[2].WaterLevel = 0.0;
            flowModel.OutputTimeStep = new TimeSpan(0, 0, 8, 0);

            flowModel.InitialConditionsType = InitialConditionsType.WaterLevel;
            flowModel.DefaultInitialWaterLevel = 0.0;
            flowModel.InitialFlow.DefaultValue = 0.0;
            flowModel.InitialConditions.DefaultValue = 0.0;

            // Set up rtc model
            var controlGroup = RealTimeControlModelHelper.CreateGroupPidRule(false);
            rtcModel.ControlGroups.Add(controlGroup);

            // Set up Observation Point -> RTC rule connection
            var observationPoint = ObservationPoint.CreateDefault(flowModel.Network.Branches[0]);
            observationPoint.Chainage = 20.0;
            flowModel.Network.Branches[0].BranchFeatures.Add(observationPoint);

            var dataItemsForObservationPoint = flowModel.GetChildDataItems(observationPoint).Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            Assert.That(dataItemsForObservationPoint.Count(), Is.Not.EqualTo(0));
            
            var outputDataItem = dataItemsForObservationPoint.First();
            rtcModel.GetDataItemByValue(rtcModel.ControlGroups[0].Inputs[0]).LinkTo(outputDataItem);

            // Set up RTC rule -> Water Crest Level connection
            var weir = new Weir
            {
                OffsetY = 0,
                FlowDirection = FlowDirection.Both,
                WeirFormula = new SimpleWeirFormula
                {
                    LateralContraction = 0.9,
                    DischargeCoefficient = 1.0
                }
            };

            var branch = flowModel.Network.Branches[0];

            var compositeStructure = new CompositeBranchStructure
            {
                Chainage  = 80.0,
                Geometry = new Point(branch.Geometry.Coordinates[0])
            };

            NetworkHelper.AddBranchFeatureToBranch(compositeStructure, branch, compositeStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeStructure, weir);

            var dataItemsForWeir = flowModel.GetChildDataItems(weir).Where(di => (di.Role & DataItemRole.Input) > 0);
            Assert.That(dataItemsForWeir.Count(), Is.Not.EqualTo(0));

            var outputWeirDataItem = dataItemsForWeir.First();
            outputWeirDataItem.LinkTo(rtcModel.GetDataItemByValue(rtcModel.ControlGroups[0].Outputs[0]));

            return hydroModel;
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());

            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }
    }
}
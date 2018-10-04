using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
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
    [Category(TestCategory.Integration)]
    [Category(TestCategory.VerySlow)]
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

        [TestCase(24, 24, 4, 0, TestName = "GivenAnIntegratedModelWithAPidRuleWithStateSavesFor1DAndRTCMatchingTheRunPeriodWhenThisModelRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun")]
        [TestCase(24, 16, 4, 4, TestName = "GivenAnIntegratedModelWithAPidRuleWithStateSavesFor1DAndRTCMatchingASubsetOfTheRunPeriodWhenThisRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun")]
        public void GivenAnIntegratedModelWithAPidRuleWithStateSavesFor1DAndRTCWhenThisModelIsRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun(int runLengthInHours, int runLengthSaveStateInHours, int intervalSaveStateInHours, int offsetSaveStateInHours)
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

                    // DeltaShell Plugins
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());    // Storing WaterFlowModel1D in NHibernate
                    app.Plugins.Add(new CommonToolsApplicationPlugin());      // Common Utilities
                    app.Plugins.Add(new NetCdfApplicationPlugin());           // Handling NetCF files - output data is stored in *.nc
                    app.Plugins.Add(new SharpMapGisApplicationPlugin()); 

                    // NGHS Plugins
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());    // Storing Networks in Integrated Model | WaterFlowModel1D
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin()); // WaterFlowModel1D
                    app.Plugins.Add(new RealTimeControlApplicationPlugin());  // RTC Model
                    app.Plugins.Add(new HydroModelApplicationPlugin());       // Integrated Model

                    app.Run();

                    app.SaveProjectAs(projectPath);
                    
                    // Given
                    //   An IntegratedModel with state saves for 1D and RTC

                    // Construct HydroModel through the HydroModelBulder
                    var builder = new HydroModelBuilder();
                    var hydroModel = builder.BuildModel(ModelGroup.SobekModels);
                    
                    // Remove Rainfall Runoff, as it is not used in this test
                    hydroModel.Activities.RemoveAllWhere(a => a is RainfallRunoffModel);

                    var fullRunStartTime = DateTime.Today;
                    var fullRunStopTime = fullRunStartTime.AddHours(runLengthInHours);
                    var timeStep = TimeSpan.FromHours(1);

                    hydroModel.StartTime = fullRunStartTime;
                    hydroModel.StopTime = fullRunStopTime;
                    hydroModel.TimeStep = timeStep;

                    // Setup Flow1D 
                    var flowModel = hydroModel.Activities.GetActivitiesOfType<WaterFlowModel1D>().FirstOrDefault();
                    Assert.NotNull(flowModel);

                    ConfigureFlowModel1D(flowModel, 
                                         fullRunStartTime, 
                                         fullRunStopTime, 
                                         timeStep);

                    // Setup RTC 
                    var rtcModel = hydroModel.Activities.GetActivitiesOfType<RealTimeControlModel>().FirstOrDefault();
                    Assert.NotNull(rtcModel);

                    ConfigureRTCModelWithPidRule(rtcModel, flowModel, 
                                                 fullRunStartTime, 
                                                 fullRunStopTime,
                                                 timeStep);

                    app.Project.RootFolder.Add(hydroModel);

                    // Set up restart time ranges on WaterFlowModel1D.
                    flowModel.WriteRestart = true;
                    flowModel.UseRestart = false;
                    flowModel.UseSaveStateTimeRange = true;
                    flowModel.SaveStateStartTime = flowModel.StartTime.AddHours(offsetSaveStateInHours);
                    flowModel.SaveStateStopTime = flowModel.SaveStateStartTime.AddHours(runLengthSaveStateInHours);
                    flowModel.SaveStateTimeStep = TimeSpan.FromHours(intervalSaveStateInHours);

                    // Set up restart time ranges on RealTimeControlModel.
                    rtcModel.WriteRestart = true;
                    rtcModel.UseRestart = false;

                    rtcModel.UseSaveStateTimeRange = true;
                    rtcModel.SaveStateStartTime = rtcModel.StartTime.AddHours(offsetSaveStateInHours);
                    rtcModel.SaveStateStopTime = rtcModel.SaveStateStartTime.AddHours(runLengthSaveStateInHours);
                    rtcModel.SaveStateTimeStep = TimeSpan.FromHours(intervalSaveStateInHours);

                    // When
                    //   Run initial model to generate restart values.
                    ActivityRunner.RunActivity(hydroModel);
                    Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                    // Obtain output values generated by non-restarted run.
                    var crestLevelOutput =
                        flowModel.OutputSettings.EngineParameters.FirstOrDefault(
                            ep => ep.QuantityType == QuantityType.CrestLevel && ep.ElementSet == ElementSet.Structures);
                    Assert.NotNull(crestLevelOutput);

                    var crestLevelFullRun = flowModel.OutputFunctions.FirstOrDefault(f => f.Name == crestLevelOutput.Name)?.Components[0].Values.OfType<double>().ToList();
                    Assert.NotNull(crestLevelFullRun);

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

                        hydroModel = (HydroModel) app.Project.RootFolder.Models.FirstOrDefault();
                        Assert.NotNull(hydroModel);
                        rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().FirstOrDefault();
                        Assert.NotNull(rtcModel);
                        flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().FirstOrDefault();
                        Assert.NotNull(flowModel);

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
                        var crestLevelRestart = flowModel.OutputFunctions.FirstOrDefault(f => f.Name == crestLevelOutput.Name)?.Components[0].Values.OfType<double>().ToArray();
                        Assert.NotNull(crestLevelRestart);

                        var crestLevelFullRunSubset = crestLevelFullRun
                            .Skip(crestLevelFullRun.Count - crestLevelRestart.Length)
                            .ToArray();

                        for (var indexCrestLevel = 0; indexCrestLevel < crestLevelRestart.Length; indexCrestLevel++)
                        {
                            // Then
                            Assert.That(crestLevelRestart[indexCrestLevel],
                                Is.EqualTo(crestLevelFullRunSubset[indexCrestLevel]).Within(0.0001));
                        }
                        app.CloseProject();
                    }
                }
            });
        }

        [TestCase(24, 24, 4, 0, TestName = "GivenAnIntegratedModelWithATimeRuleWithStateSavesFor1DAndRTCMatchingTheRunPeriodWhenThisModelRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun")]
        [TestCase(24, 16, 4, 4, TestName = "GivenAnIntegratedModelWithATimeRuleWithStateSavesFor1DAndRTCMatchingASubsetOfTheRunPeriodWhenThisRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun")]
        public void GivenAnIntegratedModelWithATimeRuleWithStateSavesFor1DAndRTCWhenThisModelIsRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun(int runLengthInHours, int runLengthSaveStateInHours, int intervalSaveStateInHours, int offsetSaveStateInHours)
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

                    // DeltaShell Plugins
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());    // Storing WaterFlowModel1D in NHibernate
                    app.Plugins.Add(new CommonToolsApplicationPlugin());      // Common Utilities
                    app.Plugins.Add(new NetCdfApplicationPlugin());           // Handling NetCF files - output data is stored in *.nc
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());

                    // NGHS Plugins
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());    // Storing Networks in Integrated Model | WaterFlowModel1D
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin()); // WaterFlowModel1D
                    app.Plugins.Add(new RealTimeControlApplicationPlugin());  // RTC Model
                    app.Plugins.Add(new HydroModelApplicationPlugin());       // Integrated Model

                    app.Run();

                    app.SaveProjectAs(projectPath);

                    // Given
                    //   An IntegratedModel with state saves for 1D and RTC

                    // Construct HydroModel through the HydroModelBulder
                    var builder = new HydroModelBuilder();
                    var hydroModel = builder.BuildModel(ModelGroup.SobekModels);

                    // Remove Rainfall Runoff, as it is not used in this test
                    hydroModel.Activities.RemoveAllWhere(a => a is RainfallRunoffModel);

                    var fullRunStartTime = DateTime.Today;
                    var fullRunStopTime = fullRunStartTime.AddHours(runLengthInHours);
                    var timeStep = TimeSpan.FromHours(1);

                    hydroModel.StartTime = fullRunStartTime;
                    hydroModel.StopTime = fullRunStopTime;
                    hydroModel.TimeStep = timeStep;

                    // Setup Flow1D 
                    var flowModel = hydroModel.Activities.GetActivitiesOfType<WaterFlowModel1D>().FirstOrDefault();
                    Assert.NotNull(flowModel);

                    ConfigureFlowModel1D(flowModel,
                                         fullRunStartTime,
                                         fullRunStopTime,
                                         timeStep);

                    // Setup RTC 
                    var rtcModel = hydroModel.Activities.GetActivitiesOfType<RealTimeControlModel>().FirstOrDefault();
                    Assert.NotNull(rtcModel);

                    ConfigureRTCModelWithTimeSeriesRule(rtcModel, flowModel,
                                                        fullRunStartTime,
                                                        fullRunStopTime,
                                                        timeStep);

                    app.Project.RootFolder.Add(hydroModel);

                    // Set up restart time ranges on WaterFlowModel1D.
                    flowModel.WriteRestart = true;
                    flowModel.UseRestart = false;

                    flowModel.UseSaveStateTimeRange = true;
                    flowModel.SaveStateStartTime = flowModel.StartTime.AddHours(offsetSaveStateInHours);
                    flowModel.SaveStateStopTime = flowModel.SaveStateStartTime.AddHours(runLengthSaveStateInHours);
                    flowModel.SaveStateTimeStep = TimeSpan.FromHours(intervalSaveStateInHours);

                    // Set up restart time ranges on RealTimeControlModel.
                    rtcModel.WriteRestart = true;
                    rtcModel.UseRestart = false;

                    rtcModel.UseSaveStateTimeRange = true;
                    rtcModel.SaveStateStartTime = rtcModel.StartTime.AddHours(offsetSaveStateInHours);
                    rtcModel.SaveStateStopTime = rtcModel.SaveStateStartTime.AddHours(runLengthSaveStateInHours);
                    rtcModel.SaveStateTimeStep = TimeSpan.FromHours(intervalSaveStateInHours);

                    // When
                    //   Run initial model to generate restart values.
                    ActivityRunner.RunActivity(hydroModel);
                    Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                    // Obtain output values generated by non-restarted run.
                    var crestLevelOutput =
                        flowModel.OutputSettings.EngineParameters.FirstOrDefault(
                            ep => ep.QuantityType == QuantityType.CrestLevel && ep.ElementSet == ElementSet.Structures);
                    Assert.NotNull(crestLevelOutput);

                    var crestLevelFullRun = flowModel.OutputFunctions.FirstOrDefault(f => f.Name == crestLevelOutput.Name)?.Components[0].Values.OfType<double>().ToList();
                    Assert.NotNull(crestLevelFullRun);

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

                        hydroModel = (HydroModel)app.Project.RootFolder.Models.FirstOrDefault();
                        Assert.NotNull(hydroModel);
                        rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().FirstOrDefault();
                        Assert.NotNull(rtcModel);
                        flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().FirstOrDefault();
                        Assert.NotNull(flowModel);

                        // Set up restart.
                        var restartStateFlowModel =
                            (FileBasedRestartState)flowModel.GetRestartOutputStates().ElementAt(i).Clone();

                        flowModel.StartTime = restartStateFlowModel.SimulationTime;
                        flowModel.StopTime = fullRunStopTime;
                        flowModel.UseRestart = true;
                        flowModel.RestartInput = restartStateFlowModel;

                        var restartStateRtcModel =
                            (FileBasedRestartState)rtcModel.GetRestartOutputStates().ElementAt(i).Clone();

                        rtcModel.StartTime = restartStateRtcModel.SimulationTime;
                        rtcModel.StopTime = fullRunStopTime;
                        rtcModel.UseRestart = true;
                        rtcModel.RestartInput = restartStateRtcModel;

                        // When
                        // Run with restart.
                        ActivityRunner.RunActivity(hydroModel);
                        Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                        // Obtain data to compare to.
                        var crestLevelRestart = flowModel.OutputFunctions.FirstOrDefault(f => f.Name == crestLevelOutput.Name)?.Components[0].Values.OfType<double>().ToArray();
                        Assert.NotNull(crestLevelRestart);

                        var crestLevelFullRunSubset = crestLevelFullRun
                            .Skip(crestLevelFullRun.Count - crestLevelRestart.Length)
                            .ToArray();

                        for (var indexCrestLevel = 0; indexCrestLevel < crestLevelRestart.Length; indexCrestLevel++)
                        {
                            // Then
                            Assert.That(crestLevelRestart[indexCrestLevel],
                                Is.EqualTo(crestLevelFullRunSubset[indexCrestLevel]).Within(0.0001));
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

        /// <summary>
        /// Configure a FlowModel1D by:
        ///   * Setting start and stop time, and timeStep.
        ///   * Configuring the model as a DemoModel.
        ///   * Adding boundary conditions.
        ///   * Adding an ObservationPoint to the first branch of the network.
        ///   * Adding a Weir to the first branch of the network.
        ///   * Enable CrestLevel output.
        /// </summary>
        private static void ConfigureFlowModel1D(WaterFlowModel1D flowModel,
                                                 DateTime startTime,
                                                 DateTime stopTime,
                                                 TimeSpan timeStep)
        {
            // Setup DemoModel
            var network = new HydroNetwork();
            flowModel.Network = network;
            WaterFlowModel1DDemoModelTestHelper.ConfigureModelAsDemoModel(flowModel);

            // Setup timings
            flowModel.StartTime = startTime;
            flowModel.StopTime = stopTime;
            flowModel.TimeStep = timeStep;

            // Add Boundary Conditions
            flowModel.BoundaryConditions[0].DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            flowModel.BoundaryConditions[0].Flow = 100.0;
            flowModel.BoundaryConditions[1].DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            flowModel.BoundaryConditions[1].WaterLevel = 0.0;
            flowModel.BoundaryConditions[2].DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            flowModel.BoundaryConditions[2].WaterLevel = 0.0;
            flowModel.OutputTimeStep = new TimeSpan(0, 0, 8, 0);

            // Add ObservationPoint
            var observationPoint = ObservationPoint.CreateDefault(flowModel.Network.Branches[0]);
            observationPoint.Chainage = 20.0;
            flowModel.Network.Branches[0].BranchFeatures.Add(observationPoint);

            // Add Weir
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

            // Enable Crest Level output
            var crestLevelOutput =
                flowModel.OutputSettings.EngineParameters.FirstOrDefault(
                    ep => ep.QuantityType == QuantityType.CrestLevel && ep.ElementSet == ElementSet.Structures);
            Assert.IsNotNull(crestLevelOutput);
            crestLevelOutput.AggregationOptions = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.AggregationOptions.Current;
        }

        /// <summary>
        /// Configure an RTC Model by:
        ///   * Setting start and stop time, and timeStep.
        ///   * adding a PID rule to the ControlGroups of the RTC Model.
        ///   * Link WaterLevel output to PID rule input.
        ///   * Link PID rule output to Water Crest Level input.
        /// </summary>
        private static void ConfigureRTCModelWithPidRule(RealTimeControlModel rtcModel, 
                                                         WaterFlowModel1D flowModel, 
                                                         DateTime startTime, 
                                                         DateTime stopTime,
                                                         TimeSpan timeStep)
        {
            // Set timings
            rtcModel.StartTime = startTime;
            rtcModel.StopTime = stopTime;
            rtcModel.TimeStep = timeStep;

            var controlGroup = RealTimeControlModelHelper.CreateGroupPidRule(false);
            rtcModel.ControlGroups.Add(controlGroup);

            // Set up observation point -> rtcModel
            var observationPoint = flowModel.Network.Branches[0].BranchFeatures.FirstOrDefault(o => o is ObservationPoint);
            Assert.NotNull(observationPoint);

            var dataItemsForObservationPoint = flowModel.GetChildDataItems(observationPoint).Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output).ToList();
            Assert.That(dataItemsForObservationPoint.Count(), Is.Not.EqualTo(0));

            var outputDataItem = dataItemsForObservationPoint.FirstOrDefault();
            Assert.NotNull(outputDataItem);
            rtcModel.GetDataItemByValue(rtcModel.ControlGroups[0].Inputs[0]).LinkTo(outputDataItem);

            // Set up rtcModel -> flowModel Weir
            var weir = flowModel.Network.Branches[0].BranchFeatures.FirstOrDefault(o => o is Weir);
            Assert.NotNull(weir);

            var dataItemsForWeir = flowModel.GetChildDataItems(weir).Where(di => (di.Role & DataItemRole.Input) > 0).ToList();
            Assert.That(dataItemsForWeir.Count(), Is.Not.EqualTo(0));

            var outputWeirDataItem = dataItemsForWeir.FirstOrDefault();
            Assert.NotNull(outputWeirDataItem);
            outputWeirDataItem.LinkTo(rtcModel.GetDataItemByValue(rtcModel.ControlGroups[0].Outputs[0]));
        }

        /// <summary>
        /// Configure an RTC Model by:
        ///   * Setting start and stop time, and timeStep.
        ///   * adding a TimeSeries rule to the ControlGroups of the RTC Model.
        ///   * Link the TimeSeries rule output to Water Crest Level input.
        ///   * Adding a simple function to the TimeSeries rule.
        /// </summary>
        private static void ConfigureRTCModelWithTimeSeriesRule(RealTimeControlModel rtcModel,
                                                                WaterFlowModel1D flowModel,
                                                                DateTime startTime,
                                                                DateTime stopTime,
                                                                TimeSpan timeStep)
        {
            // Set timings
            rtcModel.StartTime = startTime;
            rtcModel.StopTime = stopTime;
            rtcModel.TimeStep = timeStep;

            var controlGroup = RealTimeControlTestHelper.CreateGroupRuleWithoutConditionWithoutInput(new TimeRule());
            rtcModel.ControlGroups.Add(controlGroup);

            // Set up rtcModel -> flowModel Weir
            var weir = flowModel.Network.Branches[0].BranchFeatures.FirstOrDefault(o => o is Weir);
            Assert.NotNull(weir);

            var dataItemsForWeir = flowModel.GetChildDataItems(weir).Where(di => (di.Role & DataItemRole.Input) > 0).ToList();
            Assert.That(dataItemsForWeir.Count(), Is.Not.EqualTo(0));

            // Construct graph for TimeSeries rule
            const double inputValue = 5.0;
            var inputWeirDataItem = dataItemsForWeir.FirstOrDefault();
            Assert.NotNull(inputWeirDataItem);
            inputWeirDataItem.Value = inputValue * inputValue;
            inputWeirDataItem.LinkTo(rtcModel.GetDataItemByValue(controlGroup.Outputs[0]));

            ((TimeRule) controlGroup.Rules[0]).InterpolationOptionsTime = InterpolationType.Linear;

            var nSteps = Math.Ceiling(((stopTime - startTime).TotalHours / timeStep.TotalHours));
            for (var i = 0; i <= nSteps; i++)
            {
                var currentTime = startTime.AddHours(timeStep.TotalHours * i);
                var currentValue = inputValue * (1 - (i / nSteps));
                var currentValueSq = currentValue * currentValue;

                ((TimeRule) controlGroup.Rules[0]).TimeSeries[currentTime] = currentValueSq;
            }
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
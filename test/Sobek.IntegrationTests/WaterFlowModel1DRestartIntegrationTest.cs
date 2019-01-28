using System;
using System.IO;
using System.Linq;
using System.Windows;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class WaterFlowModel1DRestartIntegrationTest
    {
        [Test]
        public void CopyRestartStateCreatesCopyOfRestartFile()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                
                gui.Run();

                var mainWindow = (Window)gui.MainWindow;
                WpfTestHelper.ShowModal(mainWindow, () =>
                {
                    var project = gui.Application.Project;
                    var flowModel = CreateSimpleFlowModel();

                    project.RootFolder.Add(flowModel);

                    // run flow & write restart
                    flowModel.WriteRestart = true;

                    var report = flowModel.Validate();

                    ActivityRunner.RunActivity(flowModel);
                    Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);

                    // save project
                    app.SaveProjectAs("s1.dsproj");

                    // we assume .zip files are only written for state files, and that statefiles are in zips
                    var projectDataDirectory = app.ProjectDataDirectory;

                    Assert.AreEqual(10, Directory.GetFiles(projectDataDirectory).Count(f => f.EndsWith(".zip")));

                    // make copy & add as input (mimic what happens on 'use as initial state')
                    flowModel.RestartInput = (FileBasedRestartState) flowModel.GetRestartOutputStates().Last().Clone();

                    // copy of state added, so now there's 2 files
                    Assert.AreEqual(11, Directory.GetFiles(projectDataDirectory).Count(f => f.EndsWith(".zip")));

                    app.CloseProject();

                    //last state delete again: we closed the project without saving
                    Assert.AreEqual(10, Directory.GetFiles(projectDataDirectory).Count(f => f.EndsWith(".zip")));
                });
            }
        }

        [Test]
        public void ExportAndImportRestartStateCreatesCopyOfRestartFile()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                
                gui.Run();

                var mainWindow = (Window)gui.MainWindow;
                WpfTestHelper.ShowModal(mainWindow, () => 
                {
                    var project = gui.Application.Project;
                    var flowModel = CreateSimpleFlowModel();

                    project.RootFolder.Add(flowModel);

                    // run flow & write restart
                    flowModel.WriteRestart = true;
                    
                    ActivityRunner.RunActivity(flowModel);
                    Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);
                    
                    // export state
                    var exporter = new FileBasedRestartStateExporter();
                    var tempFile = Path.GetTempFileName();
                    File.Delete(tempFile);
                    tempFile += ".zip";
                    exporter.Export(flowModel.GetRestartOutputStates().First(), tempFile);

                    // we assume .zip files are only written for state files, and that statefiles are in zips
                    // get number of zips
                    Assert.AreEqual(10, Directory.GetFiles(app.ProjectDataDirectory).Count(f => f.EndsWith(".zip")));

                    // import state (overwrite)
                    var importer = new FileBasedRestartStateImporter();
                    flowModel.RestartInput = (FileBasedRestartState)importer.ImportItem(tempFile);
                    
                    // ds gives it a place in the data dir
                    Assert.AreEqual(11, Directory.GetFiles(app.ProjectDataDirectory).Count(f => f.EndsWith(".zip")));

                    // save project
                    app.SaveProjectAs("s1.dsproj");

                    // stil two
                    Assert.AreEqual(11, Directory.GetFiles(app.ProjectDataDirectory).Count(f => f.EndsWith(".zip")));
                    
                    app.CloseProject();
                });
            }
        }

        [Test]
        public void ExceptionPastingRestartStateInProjectWithNewNameTools9039()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                
                gui.Run();

                var mainWindow = (Window)gui.MainWindow;
                Action shownAction = () =>
                {
                    var project = gui.Application.Project;
                    var flowModel = CreateSimpleFlowModel();

                    project.RootFolder.Add(flowModel);

                    // run flow & write restart
                    flowModel.WriteRestart = true;
                    flowModel.StopTime = flowModel.StartTime.AddHours(24.0);
                    ActivityRunner.RunActivity(flowModel);
                    Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);

                    // copy & paste state to project
                    // make copy & add as input (mimic what happens on 'use as initial state')
                    var clonedState = flowModel.GetRestartOutputStates().Last().Clone();
                    project.RootFolder.Add(clonedState);

                    // delete flow
                    project.RootFolder.Items.Remove(flowModel);

                    // save project 
                    app.SaveProjectAs("adf1.dsproj");
                    project = app.Project;

                    // retrieve the state from the project
                    var stateProjectItem = project.RootFolder.Items.OfType<DataItem>().First(di => di.Value == clonedState);

                    // copy state1 (without paste)
                    gui.CopyPasteHandler.Copy(stateProjectItem);

                    // delete state1
                    project.RootFolder.Items.Remove(stateProjectItem);

                    // save project with new name
                    app.SaveProjectAs("adf2.dsproj");
                    project = app.Project;

                    // paste state1 (this doesn't actually do anything anymore: copy is cleared on save)
                    gui.CopyPasteHandler.Paste(project, project.RootFolder);
                };

                WpfTestHelper.ShowModal(mainWindow, shownAction);
            }
        }

        [Test]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]
        public void CompareFullRunWithRestartedRunInGuiWithSaveLoad()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                app.Run();
                app.SaveProjectAs("s2.dsproj");//to initialize repositories..stupid i know!
                
                var project = app.Project;
                var flowModel = CreateSimpleFlowModel();
                
                var samplePoint = flowModel.Network.CrossSections.First();

                project.RootFolder.Add(flowModel);

                // run full calculation
                flowModel.WriteRestart = false;
                flowModel.StopTime = flowModel.StartTime.AddHours(24.0);
                ActivityRunner.RunActivity(flowModel);
                Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);

                var waterLevelValuesFullRun = flowModel.OutputWaterLevel.GetTimeSeries(samplePoint).Components[0].Values.OfType<double>().ToList();

                // do one half run, and write restart
                flowModel.WriteRestart = true;
                flowModel.StopTime = flowModel.StartTime.AddHours(12.0);
                ActivityRunner.RunActivity(flowModel);
                Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);
                var halfWayState = (FileBasedRestartState)flowModel.GetRestartOutputStates().Last().Clone();
                flowModel.RestartInput = halfWayState;

                // save & load
                app.SaveProject();
                app.CloseProject();
                app.OpenProject("s2.dsproj");

                flowModel = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().First();
                samplePoint = flowModel.Network.CrossSections.First();

                // restart from 2nd half of run, and compare results
                flowModel.StartTime = flowModel.StopTime;
                flowModel.StopTime = flowModel.StartTime.AddHours(12.0);
                flowModel.UseRestart = true;
                ActivityRunner.RunActivity(flowModel);
                Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);

                // assert results are the same
                var waterLevelRestartSecondHalf = flowModel.OutputWaterLevel.GetTimeSeries(samplePoint).Components[0].Values.OfType<double>().ToList();
                var waterLevelFullRunSecondHalf = waterLevelValuesFullRun.Skip(waterLevelValuesFullRun.Count - waterLevelRestartSecondHalf.Count).ToArray();

                for (int i = 0; i < waterLevelFullRunSecondHalf.Length; i++)
                {
                    Assert.AreEqual(waterLevelFullRunSecondHalf[i], waterLevelRestartSecondHalf[i], 1E-4, i.ToString() + ": ");
                }
            }

            /*using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                
                gui.Run();

                var mainWindow = (Window)gui.MainWindow;
                WpfTestHelper.ShowModal(mainWindow, () =>
                    {
                        
                        
                        
                    }

                );
            }*/
        }

        [Test]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]
        public void CompareFullRunWithRestartedRun()
        {
            var flowModel = CreateSimpleFlowModel();
            var samplePoint = flowModel.Network.CrossSections.First();

            // do one full run
            flowModel.WriteRestart = false;
            flowModel.StopTime = flowModel.StartTime.AddHours(24.0);
            WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(flowModel);
            ActivityRunner.RunActivity(flowModel);
            Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);
            
            var waterLevelValuesFullRun = flowModel.OutputWaterLevel.GetTimeSeries(samplePoint).Components[0].Values.OfType<double>().ToList();

            // do one half run, and write restart
            flowModel.WriteRestart = true;
            flowModel.StopTime = flowModel.StartTime.AddHours(12.0);
            ActivityRunner.RunActivity(flowModel);
            Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);
            var halfWayState = (FileBasedRestartState) flowModel.GetRestartOutputStates().Last().Clone();

            // restart from 2nd half of run, and compare results
            flowModel.StartTime = flowModel.StopTime;
            flowModel.StopTime = flowModel.StartTime.AddHours(12.0);
            flowModel.UseRestart = true;
            flowModel.RestartInput = halfWayState;
            ActivityRunner.RunActivity(flowModel);
            Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);
            
            // assert results are the same
            var waterLevelRestartSecondHalf = flowModel.OutputWaterLevel.GetTimeSeries(samplePoint).Components[0].Values.OfType<double>().ToList();
            var waterLevelFullRunSecondHalf = waterLevelValuesFullRun.Skip(waterLevelValuesFullRun.Count - waterLevelRestartSecondHalf.Count).ToArray(); 
            
            for (int i = 0; i < waterLevelFullRunSecondHalf.Length; i++)
            {
                Assert.AreEqual(waterLevelFullRunSecondHalf[i], waterLevelRestartSecondHalf[i], 1E-4, i.ToString()+": ");
            }
        }

        [Test]
        public void WriteMultipleRestartsDuringRun()
        {
            var flowModel = CreateSimpleFlowModel();

            // do one full run
            flowModel.WriteRestart = true;
            flowModel.StopTime = flowModel.StartTime.AddHours(24.0);
            flowModel.OutputTimeStep = new TimeSpan(0,1,0,0);

            flowModel.SaveStateStartTime = flowModel.StartTime;
            flowModel.SaveStateStopTime = flowModel.StopTime;
            flowModel.SaveStateTimeStep = flowModel.OutputTimeStep;
            
            ActivityRunner.RunActivity(flowModel);
            if (flowModel.Status == ActivityStatus.Failed)
            {
                var report = flowModel.Validate();
                var modeldataValidationReport = report.SubReports.FirstOrDefault(r => r.Category == "Model Data");
                var validationReport = modeldataValidationReport.SubReports.FirstOrDefault(r => r.Category == "Dimr intermediate restart files");
                Assert.That(validationReport, Is.Not.Null);
                Assert.That(validationReport.Issues.Count(), Is.EqualTo(1));
                Assert.That(validationReport.Issues.First().Message,
                    Is.StringContaining("Currently, Flow 1D models cannot create intermediate restart files.At the moment, a single restart file may only be written for the final time-step after a complete run."));
                return;
            }
            Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);

            Assert.AreEqual(24, flowModel.GetRestartOutputStates().Count());
        }

        [TestCase(24, 24, 4, 0, TestName = "GivenASimpleFlowModelWithAStateSaveMatchingTheRunPeriodWhenThisModelRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun")]
        [TestCase(24, 16, 4, 4, TestName = "GivenASimpleFlowModelWithAStateSaveMatchingASubsetOfTheRunPeriodWhenThisRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun")]
        public void GivenASimpleFlowModelWhenThisModelIsRunAndRerunThenTheResultsOfTheRerunAreEqualToTheInitialRun(int runLengthInHours, int runLengthSaveStateInHours, int intervalSaveStateInHours, int offsetSaveStateInHours)
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string projectName = "restartTestProject.dsproj";
                var projectPath = Path.Combine(tempDir, projectName);

                using (var app = new DeltaShellApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new NetCdfApplicationPlugin());

                    app.Run();

                    app.SaveProjectAs(projectPath);

                    var flowModel = CreateSimpleFlowModel();
                    var samplePoint = flowModel.Network.CrossSections.First();
                    app.Project.RootFolder.Add(flowModel);

                    // When
                    // Run full calculation with generating restart times.
                    flowModel.WriteRestart = true;
                    flowModel.StopTime = flowModel.StartTime.AddHours(runLengthInHours);

                    flowModel.SaveStateStartTime = flowModel.StartTime.AddHours(offsetSaveStateInHours);
                    flowModel.SaveStateStopTime = flowModel.SaveStateStartTime.AddHours(runLengthSaveStateInHours);
                    flowModel.SaveStateTimeStep = TimeSpan.FromHours(intervalSaveStateInHours);

                    ActivityRunner.RunActivity(flowModel);
                    Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);

                    // Get results for the waterLevels after this run to compare with partial runs.
                    var waterLevelValuesFullRun = flowModel.OutputWaterLevel.GetTimeSeries(samplePoint).Components[0]
                        .Values.OfType<double>().ToList();

                    // Remove writeRestart for subsequent runs.
                    flowModel.WriteRestart = false;

                    var fullRunStartTime = flowModel.StartTime;
                    var fullRunStopTime = flowModel.StopTime;

                    // Save project and close project.
                    app.SaveProject();
                    app.CloseProject();

                    // Calculate number of restart states to evaluate.
                    var nRestartStates = (runLengthSaveStateInHours / intervalSaveStateInHours);
                    var lastRestartStateOverlapsWithStop =
                        fullRunStopTime.Equals(fullRunStartTime.AddHours(offsetSaveStateInHours + nRestartStates * intervalSaveStateInHours));

                    if (lastRestartStateOverlapsWithStop)
                        nRestartStates -= 1;

                    // Do restarts for each of the restart files.
                    for (var i = 0; i < nRestartStates; i++)
                    {
                        app.OpenProject(projectPath);
                        flowModel = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().First();
                        samplePoint = flowModel.Network.CrossSections.First();

                        // Set up restart details of next run.
                        var restartState =
                            (FileBasedRestartState) flowModel.GetRestartOutputStates().ElementAt(i).Clone();

                        flowModel.StartTime = restartState.SimulationTime;
                        flowModel.StopTime = fullRunStopTime;
                        flowModel.UseRestart = true;

                        flowModel.RestartInput = restartState;

                        // Run with restart.
                        ActivityRunner.RunActivity(flowModel);
                        Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);

                        // Obtain the data to compare with.
                        var waterLevelRestart = flowModel.OutputWaterLevel.GetTimeSeries(samplePoint).Components[0]
                            .Values.OfType<double>().ToArray();
                        var waterLevelFullRunSubSetRestart = waterLevelValuesFullRun
                            .Skip(waterLevelValuesFullRun.Count - waterLevelRestart.Length).ToArray();

                        for (var indexWaterLevel = 0;
                            indexWaterLevel < waterLevelFullRunSubSetRestart.Length;
                            indexWaterLevel++)
                        {
                        // Then
                        Assert.That(waterLevelRestart[indexWaterLevel],
                            Is.EqualTo(waterLevelFullRunSubSetRestart[indexWaterLevel]).Within(0.0001));
                        }

                        app.CloseProject();
                    }
                }
            });
        }

        private static WaterFlowModel1D CreateSimpleFlowModel()
        {
            var flowModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            flowModel.BoundaryConditions[0].DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            flowModel.BoundaryConditions[0].Flow = 300.0;
            flowModel.BoundaryConditions[2].DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            flowModel.BoundaryConditions[2].Flow = -100.0;
            return flowModel;
        }
    }
}
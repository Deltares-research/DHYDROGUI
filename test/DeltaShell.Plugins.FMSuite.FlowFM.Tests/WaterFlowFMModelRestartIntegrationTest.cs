using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
﻿using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
﻿using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
﻿using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class WaterFlowFMModelRestartIntegrationTest
    {
        [Test]
        [Category("Quarantine")]
        public void WriteRestart()
        {
            var model = LoadBendProfModelWithWriteRestart();

            ActivityRunner.RunActivity(model);

            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            Assert.AreEqual(1, model.GetRestartOutputStates().Count());
        }

        [Test]
        public void WriteRestartMultipleSaveLoad()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                var path = "restart.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = LoadBendProfModelWithWriteRestart();

                app.Project.RootFolder.Add(model);
                
                model.SaveStateStartTime = model.StartTime;
                model.SaveStateStopTime = model.StopTime;
                model.SaveStateTimeStep = new TimeSpan(0, 1, 0);

                ActivityRunner.RunActivity(model);
                model.RestartInput = (FileBasedRestartState) model.GetRestartOutputStates().First().Clone();
                
                var countBefore = model.GetRestartOutputStates().Count();
                var nameBefore = model.GetRestartOutputStates().First().Name;

                var newPath = "/new/restartNew.dsproj";

                app.SaveProjectAs(newPath);
                app.CloseProject();
                app.OpenProject(newPath);

                var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

                Assert.AreEqual(countBefore, retrievedModel.GetRestartOutputStates().Count());
                Assert.AreEqual(nameBefore, retrievedModel.GetRestartOutputStates().First().Name);
                Assert.IsNotNull(retrievedModel.RestartInput);
                Assert.IsFalse(retrievedModel.RestartInput.IsEmpty);
            }
        }

        [Test]
        public void WriteOutputRestartOnlySaveLoad()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                var path = "restart.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = LoadBendProfModelWithWriteRestart();

                app.Project.RootFolder.Add(model);

                model.SaveStateStartTime = model.StartTime;
                model.SaveStateStopTime = model.StopTime;
                model.SaveStateTimeStep = new TimeSpan(0, 1, 0);

                ActivityRunner.RunActivity(model);

                var newPath = "new/restartNew.dsproj";

                app.SaveProjectAs(newPath);
                app.CloseProject();
                app.OpenProject(newPath);

                var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel.RestartInput);
                Assert.IsTrue(retrievedModel.RestartInput.IsEmpty);
            }
        }

        [Test]
        [Category("Quarantine")]
        public void LoadModelWithRestartOptionsSetCheckThem()
        {
            var mduPath = TestHelper.GetTestFilePath(@"restart\bendprof.mdu");
            var model = new WaterFlowFMModel(TestHelper.CreateLocalCopy(mduPath));

            Assert.AreEqual(true, model.WriteRestart);
            Assert.AreEqual(true, model.UseSaveStateTimeRange);
            Assert.AreEqual(new DateTime(1992, 8, 31, 0, 2, 0), model.SaveStateStartTime);
            Assert.AreEqual(new DateTime(1992, 8, 31, 0, 6, 0), model.SaveStateStopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0), model.SaveStateTimeStep);

            ActivityRunner.RunActivity(model);

            Assert.AreEqual(6, model.GetRestartOutputStates().Count()); //5 + always last timestep
        }

        [Test]
        [Category("Quarantine")]
        public void WriteRestartMultipleTimes()
        {
            var model = LoadBendProfModelWithWriteRestart();
            model.StopTime = model.StartTime.AddMinutes(15);

            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = new TimeSpan(0, 1, 0);

            ActivityRunner.RunActivity(model);

            Assert.AreEqual(15, model.GetRestartOutputStates().Count());
        }

        [Test]
        [Category("Quarantine")]
        public void WriteAndUseRestart()
        {
            // make sure the test is clean:
            var restartFilePath = "input\\bendprof_19920831_000200_rst.nc";
            FileUtils.DeleteIfExists(restartFilePath);
            
            var model = LoadBendProfModelWithWriteRestart();
            ActivityRunner.RunActivity(model); //generates an output state

            // set output state as input state
            model.RestartInput = (FileBasedRestartState) model.GetRestartOutputStates().First().Clone();
            model.UseRestart = true;

            model.Initialize();

            restartFilePath = Path.Combine(model.WorkingDirectory, Path.GetFileName(restartFilePath));

            Assert.AreEqual(ActivityStatus.Initialized, model.Status);
            Assert.IsTrue(File.Exists(restartFilePath), "restart file exists");
            Assert.AreEqual(Path.GetFileName(restartFilePath),
                            model.ModelDefinition.GetModelProperty(KnownProperties.RestartFile).GetValueAsString(),
                            "model definition is adjusted");
        }

        [Test]
        [Category("Quarantine")]
        public void WriteExportImportRestart()
        {
            // make sure the test is clean:
            const string restartFilePath = "input\\bendprof_19920831_000200_rst.nc";
            FileUtils.DeleteIfExists(restartFilePath);

            var model = LoadBendProfModelWithWriteRestart();
            ActivityRunner.RunActivity(model); //generates an output state

            // set output state as input state
            model.RestartInput = (FileBasedRestartState) model.GetRestartOutputStates().First().Clone();
            model.UseRestart = true;

            model.Initialize();

            var exporter = new WaterFlowFMFileExporter();
            Directory.CreateDirectory("restartmodel");
            exporter.Export(model, "restartmodel/bendprof.mdu");

            var importer = new WaterFlowFMFileImporter();
            var importedModel = (WaterFlowFMModel) importer.ImportItem("restartmodel/bendprof.mdu");

            Assert.IsTrue(importedModel.UseRestart);
            Assert.AreEqual(Path.GetFileName(importedModel.RestartInput.Path), "state_bendprof_1992-08-31_00-02-00.zip");
        }

        [Test]
        [Category("Quarantine")]
        public void CompareFullRunWithRestartedRun()
        {
            var measureLocation = new Coordinate(100, 100);
            var model = LoadBendProfModelWithWriteRestart();

            // first half:
            model.StopTime = model.StartTime.AddMinutes(2); //6 sec timestep
            ActivityRunner.RunActivity(model); //generates an output state
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            var waterLevelResultsFirstHalf = GetWaterLevelValuesAtPoint(model, measureLocation);
            var restartHalfway = (FileBasedRestartState)model.GetRestartOutputStates().Last().Clone();
            
            // full run:
            model.WriteRestart = false;
            model.StopTime = model.StartTime.AddMinutes(4); //6 sec timestep
            ActivityRunner.RunActivity(model); //generates an output state
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            var fullRunTimes = model.OutputWaterLevel.Time.Values.ToList();
            var waterLevelResultsFullRun = GetWaterLevelValuesAtPoint(model, measureLocation);
            
            // restarted run (from halfway):
            model.RestartInput = restartHalfway;
            model.UseRestart = true;
            model.StartTime = model.StartTime.AddMinutes(2);
            model.StopTime = model.StartTime.AddMinutes(2); //6 sec timestep
            ActivityRunner.RunActivity(model); //generates an output state
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            var waterLevelResultsSecondHalf = GetWaterLevelValuesAtPoint(model, measureLocation); //first time step overlaps with last timestep of first run
            
            // log to console:
            const string format = "{0,-25}{1,-20}{2,-20}{3,-20}{4,-20}";
            var indexOf2ndRun = waterLevelResultsFirstHalf.Count - 1;
            for (int i = 0; i < fullRunTimes.Count; i++)
            {
                if (i == 0)
                    Console.WriteLine(Environment.NewLine + format + Environment.NewLine, "(t)", "Full", "First", "Second", "Diff");

                var currentTime = fullRunTimes[i];
                var fullRunLevel = waterLevelResultsFullRun[i];
                var firstHalfLevel = i <= indexOf2ndRun ? waterLevelResultsFirstHalf[i].ToString() : "";
                var secondHalfLevel = i >= indexOf2ndRun ? waterLevelResultsSecondHalf[i - indexOf2ndRun].ToString() : "";

                var diff = waterLevelResultsFullRun[i] - (i >= indexOf2ndRun
                                                              ? waterLevelResultsSecondHalf[i - indexOf2ndRun]
                                                              : waterLevelResultsFirstHalf[i]);

                Console.WriteLine(format, currentTime.ToString(), fullRunLevel, firstHalfLevel, secondHalfLevel, diff);
            }

            var combinedRunResults = waterLevelResultsFirstHalf.Concat(waterLevelResultsSecondHalf.Skip(1)).ToList();
            
            // asserts:
            Assert.AreEqual(waterLevelResultsFullRun.Count, combinedRunResults.Count);
            for (int i = 0; i < waterLevelResultsFullRun.Count; i++)
                Assert.AreEqual(waterLevelResultsFullRun[i], combinedRunResults[i], 0.05, "index:" + i); //5cm..
        }

        private static IList<double> GetWaterLevelValuesAtPoint(WaterFlowFMModel model, Coordinate measureLocation)
        {
            var result = new List<double>();
            if (model == null || model.OutputWaterLevel == null || model.OutputWaterLevel.Time == null) return result;
            
            foreach (var time in model.OutputWaterLevel.Time.Values)
                result.Add((double) model.OutputWaterLevel.Evaluate(measureLocation, time));

            return result;
        }

        private static WaterFlowFMModel LoadBendProfModelWithWriteRestart()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            model.WriteRestart = true;
            model.StopTime = model.StartTime.AddMinutes(2); //6 sec timestep
            model.OutputTimeStep = new TimeSpan(0, 0, 15);
            model.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = model.TimeStep;
            model.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = model.TimeStep;

            return model;
        }
    }
}
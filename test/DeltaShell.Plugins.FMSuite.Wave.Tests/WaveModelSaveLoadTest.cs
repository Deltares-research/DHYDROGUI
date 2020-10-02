using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class WaveModelSaveLoadTest
    {
        [Test]
        public void SaveLoadEmptyWaveModel()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();

                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(model.ModelDefinition.Properties.Count, retrievedModel.ModelDefinition.Properties.Count);
            }
        }

        [Test]
        public void SaveLoadCoordinateSystem()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var path = "coords.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);

                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel);
                Assert.IsNotNull(retrievedModel.CoordinateSystem);
                Assert.AreEqual(model.CoordinateSystem.WKT, retrievedModel.CoordinateSystem.WKT);
            }
        }

        [Test]
        public void SaveLoadImportedWaveModel()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                string mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");

                var model = new WaveModel(mdwFilePath);
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(model.ModelDefinition.Properties.Count, retrievedModel.ModelDefinition.Properties.Count);
                Assert.AreEqual(model.ModelDefinition.BoundaryContainer.Boundaries.Count,
                                retrievedModel.ModelDefinition.BoundaryContainer.Boundaries.Count);
            }
        }

        [Test]
        public void SaveLoadImportedWaveModelTwiceWithoutEventLeaks()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var path = "mdw.dsproj";
                app.SaveProjectAs(path);

                string mdwFilePath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary/obw.mdw");

                var model = new WaveModel(mdwFilePath);
                app.Project.RootFolder.Add(model);

                int subscriptionsBefore = TestReferenceHelper.FindEventSubscriptions(model);

                app.SaveProjectAs(path);
                app.CloseProject();
                app.OpenProject(path);
                app.SaveProjectAs(path);
                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];

                Assert.AreEqual(retrievedModel.BoundaryContainer.Boundaries.Count, model.BoundaryContainer.Boundaries.Count);
                Assert.AreEqual(retrievedModel.Obstacles.Count, model.Obstacles.Count);
                Assert.AreEqual(retrievedModel.Obstacles.Count, model.Obstacles.Count);
                Assert.AreEqual(WaveDomainHelper.GetAllDomains(retrievedModel.OuterDomain).Count,
                                WaveDomainHelper.GetAllDomains(model.OuterDomain).Count);
                Assert.AreEqual(retrievedModel.Obstacles.Count, model.Obstacles.Count);

                int subscriptionsAfter = TestReferenceHelper.FindEventSubscriptions(model);
                int subscriptionsAfterRetrieved = TestReferenceHelper.FindEventSubscriptions(retrievedModel);

                Assert.AreEqual(subscriptionsBefore, subscriptionsAfterRetrieved, "event leak");
                Assert.LessOrEqual(subscriptionsAfter, subscriptionsBefore, "event leak");
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledStartTime()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                app.Project.RootFolder.Add(model);

                //Set parameter to desired value.
                DateTime newStartTime = DateTime.Today;
                if (model.StartTime == newStartTime)
                {
                    newStartTime = newStartTime.AddDays(1);
                }

                model.StartTime = newStartTime;
                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];
                //Check persistance
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newStartTime, retrievedModel.StartTime);
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledStopTime()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                app.Project.RootFolder.Add(model);

                //Set parameter to desired value.
                DateTime newStopTime = DateTime.Today;
                if (model.StopTime == newStopTime)
                {
                    newStopTime = newStopTime.AddDays(1);
                }

                model.StopTime = newStopTime;
                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];
                //Check persistance
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newStopTime, retrievedModel.StopTime);
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledTimeStep()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                app.Project.RootFolder.Add(model);

                //Set parameter to desired value.
                TimeSpan newTimeStep = TimeSpan.FromHours(1);
                if (model.TimeStep == newTimeStep)
                {
                    newTimeStep = newTimeStep.Add(TimeSpan.FromHours(1));
                }

                model.TimeStep = newTimeStep;
                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];
                //Check persistance
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newTimeStep, retrievedModel.TimeStep);
            }
        }

        [Test]
        public void ImportModelSaveAsConfirmFilesAreCopiedAlong()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var path = "mdw_grid.dsproj";
                var secondPath = "target_mdw_grid.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                string mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");
                var model = new WaveModel(mdwFilePath);

                app.Project.RootFolder.Add(model);

                // after this call, we should go into PFBIR.Initialize(..) to get filebased items form project ???
                app.SaveProjectAs(path);

                app.SaveProjectAs(secondPath);

                string targetDir = Path.Combine(secondPath + "_data", model.Name);
                Assert.IsTrue(File.Exists(Path.Combine(targetDir, "input", model.Name + ".mdw")));
            }
        }

        [Test]
        public void WaveAddDeleteDomainsSaveLoadTest()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                var projPath = "modelSaveLoadDomainsTest.dsproj";
                app.SaveProjectAs(projPath); // save to initialize file repository..

                var model = new WaveModel {Name = "domainSaveLoadTest"};
                app.Project.RootFolder.Add(model);

                var inner = new WaveDomainData("inner");
                model.AddSubDomain(model.OuterDomain, inner);
                model.AddSubDomain(inner, new WaveDomainData("innerior"));
                app.SaveProjectAs(projPath);

                model.DeleteSubDomain(model.OuterDomain, inner);

                app.SaveProjectAs(projPath);
                app.CloseProject();

                // open
                app.OpenProject(projPath);
                var loadedModel = app.Project.RootFolder.Items[0] as WaveModel;

                Assert.AreEqual(1, WaveDomainHelper.GetAllDomains(loadedModel.OuterDomain).Count);
                Assert.AreEqual(model.OuterDomain.Name, loadedModel.OuterDomain.Name);
            }
        }

        [Test]
        public void WaveBathymetryDefinitionsSaveLoadTest()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            {
                const string projPath = "bathySaveLoadTest.dsproj";
                app.SaveProjectAs(projPath); // save to initialize file repository..

                var model = new WaveModel {Name = "bathySaveLoadTest"};

                app.Project.RootFolder.Add(model);

                app.SaveProject();
                app.CloseProject();

                app.OpenProject(projPath);
                var loadedModel = app.Project.RootFolder.Items[0] as WaveModel;

                IEnumerable<IDataItem> bathymetries = loadedModel.DataItems.Where(d => d.Name == loadedModel.OuterDomain.Bathymetry.Name);
                Assert.AreEqual(1, bathymetries.Count()); // TOOLS-22877: with every save the bathymetry was added as a duplicate
                Assert.AreEqual(loadedModel.OuterDomain.Bathymetry, loadedModel.DataItems.FirstOrDefault(d => d.Name == loadedModel.OuterDomain.Bathymetry.Name).Value);
            }
        }

        [Test]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void SaveWaveModel_AfterClearingExistingOutput_ThenWaveOutputFileIsRemoved()
        {
            // Setup
            string testDataDirectory = TestHelper.GetTestFilePath("WaveModelSaveLoadTest");
            using (var tempDirectory = new TemporaryDirectory())
            {
                FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);
                string waveOutputFilePath = Path.Combine(tempDirectory.Path, "wavm-Waves.nc");
                string mdwFilePath = Path.Combine(tempDirectory.Path, "Waves.mdw");

                var waveModel = new WaveModel(mdwFilePath);

                // Simulate the result of clearing model output
                waveModel.WavmFunctionStores.Single().Path = waveOutputFilePath;
                waveModel.WavmFunctionStores.Single().Close();

                // Pre-condition
                Assert.That(File.Exists(waveOutputFilePath), Is.True);
                Assert.That(waveModel.WavmFunctionStores.Single().Functions, Is.Empty);

                string saveModelDir = tempDirectory.CreateDirectory("Waves");

                // Call
                waveModel.ModelSaveTo(Path.Combine(saveModelDir, "input", "Waves.mdw"), true);

                // Assert
                Assert.That(Path.Combine(saveModelDir, "output", "wavm-Waves.nc"), Does.Not.Exist);
            }
        }

        [Test]
        public void ModelSaveTo_MdwFilePathUnderRoot_ThrowsInvalidOperationException()
        {
            // Setup
            var model = new WaveModel();

            // Call
            void Call() => model.ModelSaveTo(@"c:\model.mdw", true);

            // Assert
            var e = Assert.Throws<InvalidOperationException>(Call);
            Assert.That(e.Message, Is.EqualTo("Model cannot be directly saved under the root."));
        }

        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void ModelSaveTo_WithOutput_SavesOutput(bool switchTo)
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string origOutputFile = temp.CopyTestDataFileToTempDirectory("output_wavm\\wavm-wave.nc");

                model.WavmFunctionStores.First().Path = origOutputFile;

                // Call
                model.ModelSaveTo(Path.Combine(temp.Path, "input", "Waves.mdw"), switchTo);

                // Assert
                string expectedOutputFile = Path.Combine(temp.Path, "output", "wavm-wave.nc");
                Assert.That(expectedOutputFile, Does.Exist);
                Assert.That(origOutputFile, Does.Exist);
                Assert.That(model.WavmFunctionStores.First().Path, Is.EqualTo(switchTo ? expectedOutputFile : origOutputFile));
            }
        }

        [Test]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void WaveOutputSaveLoadTest()
        {
            using (DeltaShellApplication app = GetRunningApplication())
            using (var tempDir = new TemporaryDirectory())
            {
                RunModel(tempDir, app.Project.RootFolder);

                string projPath = Path.Combine(tempDir.Path, "project.dsproj");

                app.SaveProjectAs(projPath);

                AssertFileStructure(projPath);

                app.CloseProject();

                app.OpenProject(projPath);

                using (var loadedModel = (WaveModel) app.Project.RootFolder.Items[0])
                {
                    WavmFileFunctionStore functionStore = loadedModel.WavmFunctionStores.First();
                    Assert.That(functionStore.Functions[0].Components[0].GetValues(), Is.Not.Empty);
                }

                app.CloseProject();
            }
        }

        private static DeltaShellApplication GetRunningApplication()
        {
            var app = new DeltaShellApplication {IsProjectCreatedInTemporaryDirectory = true};
            LoadRequiredPlugins(app);
            app.Run();

            return app;
        }

        private static void RunModel(TemporaryDirectory tempDir, Folder rootFolder)
        {
            string mdwDirPath = tempDir.CopyDirectoryToTempDirectory(TestHelper.GetTestFilePath(@"obw"));
            string mdwFilePath = Path.Combine(mdwDirPath, "obw.mdw");

            using (var model = new WaveModel(mdwFilePath))
            {
                rootFolder.Add(model);

                ActivityRunner.RunActivity(model);
            }
        }

        private static string AssertExists(string dir, string relPath)
        {
            string path = Path.Combine(dir, relPath);
            Assert.That(path, Does.Exist);

            return path;
        }

        private static void AssertFileStructure(string projPath)
        {
            string modelFolder = AssertExists(projPath + "_data", "obw");
            string inputFolder = AssertExists(modelFolder, "input");
            string outputFolder = AssertExists(modelFolder, "output");

            AssertExists(inputFolder, "coastw.grd");
            AssertExists(inputFolder, "coastw20.dep");
            AssertExists(inputFolder, "obw.mdw");
            AssertExists(inputFolder, "obw.obs");
            AssertExists(inputFolder, "obw.pol");
            AssertExists(inputFolder, "points.xy");
            AssertExists(outputFolder, "wavm-obw.nc");
        }

        private static void LoadRequiredPlugins(DeltaShellApplication app)
        {
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaveApplicationPlugin());
        }
    }
}
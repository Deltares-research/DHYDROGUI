using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.Data.NHibernate;
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
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string path = "mdw.dsproj";

                var model = new WaveModel();

                project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                projectService.CloseProject();
                project = projectService.OpenProject(path);

                var retrievedModel = (WaveModel)project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(model.ModelDefinition.Properties.Count, retrievedModel.ModelDefinition.Properties.Count);
            }
        }

        [Test]
        public void SaveLoadCoordinateSystem()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string path = "coords.dsproj";

                var model = new WaveModel();
                model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);

                project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                projectService.CloseProject();
                project = projectService.OpenProject(path);

                var retrievedModel = (WaveModel)project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel);
                Assert.IsNotNull(retrievedModel.CoordinateSystem);
                Assert.AreEqual(model.CoordinateSystem.WKT, retrievedModel.CoordinateSystem.WKT);
            }
        }

        [Test]
        public void SaveLoadImportedWaveModel()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string path = "mdw.dsproj";

                string mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");

                var model = new WaveModel(mdwFilePath);
                project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                projectService.CloseProject();
                project = projectService.OpenProject(path);

                var retrievedModel = (WaveModel)project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(model.ModelDefinition.Properties.Count, retrievedModel.ModelDefinition.Properties.Count);
                Assert.AreEqual(model.ModelDefinition.BoundaryContainer.Boundaries.Count,
                                retrievedModel.ModelDefinition.BoundaryContainer.Boundaries.Count);
            }
        }

        [Test]
        public void SaveLoadImportedWaveModelTwiceWithoutEventLeaks()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string path = "mdw.dsproj";

                string mdwFilePath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary/obw.mdw");

                var model = new WaveModel(mdwFilePath);
                project.RootFolder.Add(model);

                int subscriptionsBefore = TestReferenceHelper.FindEventSubscriptions(model);

                projectService.SaveProjectAs(path);
                projectService.CloseProject();
                projectService.OpenProject(path);
                projectService.SaveProjectAs(path);
                projectService.CloseProject();
                project = projectService.OpenProject(path);

                var retrievedModel = (WaveModel)project.RootFolder.Items[0];

                Assert.AreEqual(retrievedModel.BoundaryContainer.Boundaries.Count, model.BoundaryContainer.Boundaries.Count);
                Assert.AreEqual(retrievedModel.FeatureContainer.Obstacles.Count, model.FeatureContainer.Obstacles.Count);
                Assert.AreEqual(retrievedModel.FeatureContainer.Obstacles.Count, model.FeatureContainer.Obstacles.Count);
                Assert.AreEqual(WaveDomainHelper.GetAllDomains(retrievedModel.OuterDomain).Count,
                                WaveDomainHelper.GetAllDomains(model.OuterDomain).Count);
                Assert.AreEqual(retrievedModel.FeatureContainer.Obstacles.Count, model.FeatureContainer.Obstacles.Count);

                int subscriptionsAfter = TestReferenceHelper.FindEventSubscriptions(model);
                int subscriptionsAfterRetrieved = TestReferenceHelper.FindEventSubscriptions(retrievedModel);

                Assert.AreEqual(subscriptionsBefore, subscriptionsAfterRetrieved, "event leak");
                Assert.LessOrEqual(subscriptionsAfter, subscriptionsBefore, "event leak");
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledStartTime()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string path = "mdw.dsproj";

                var model = new WaveModel();
                project.RootFolder.Add(model);

                //Set parameter to desired value.
                DateTime newStartTime = DateTime.Today;
                if (model.StartTime == newStartTime)
                {
                    newStartTime = newStartTime.AddDays(1);
                }

                model.StartTime = newStartTime;
                projectService.SaveProjectAs(path);

                projectService.CloseProject();
                project = projectService.OpenProject(path);

                var retrievedModel = (WaveModel)project.RootFolder.Items[0];
                //Check persistance
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newStartTime, retrievedModel.StartTime);
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledStopTime()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string path = "mdw.dsproj";

                var model = new WaveModel();
                project.RootFolder.Add(model);

                //Set parameter to desired value.
                DateTime newStopTime = DateTime.Today;
                if (model.StopTime == newStopTime)
                {
                    newStopTime = newStopTime.AddDays(1);
                }

                model.StopTime = newStopTime;
                projectService.SaveProjectAs(path);

                projectService.CloseProject();
                project = projectService.OpenProject(path);

                var retrievedModel = (WaveModel)project.RootFolder.Items[0];

                //Check persistence
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newStopTime, retrievedModel.StopTime);
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledTimeStep()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string path = "mdw.dsproj";

                var model = new WaveModel();
                project.RootFolder.Add(model);

                //Set parameter to desired value.
                TimeSpan newTimeStep = TimeSpan.FromHours(1);
                if (model.TimeStep == newTimeStep)
                {
                    newTimeStep = newTimeStep.Add(TimeSpan.FromHours(1));
                }

                model.TimeStep = newTimeStep;
                projectService.SaveProjectAs(path);

                projectService.CloseProject();
                project = projectService.OpenProject(path);

                var retrievedModel = (WaveModel)project.RootFolder.Items[0];
                //Check persistence
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newTimeStep, retrievedModel.TimeStep);
            }
        }

        [Test]
        public void ImportModelSaveAsConfirmFilesAreCopiedAlong()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string path = "mdw_grid.dsproj";
                const string secondPath = "target_mdw_grid.dsproj";

                string mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");
                var model = new WaveModel(mdwFilePath);

                project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);
                projectService.SaveProjectAs(secondPath);

                string targetDir = Path.Combine(secondPath + "_data", model.Name);
                Assert.IsTrue(File.Exists(Path.Combine(targetDir, "input", model.Name + ".mdw")));
            }
        }

        [Test]
        public void WaveAddDeleteDomainsSaveLoadTest()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string projPath = "modelSaveLoadDomainsTest.dsproj";

                var model = new WaveModel {Name = "domainSaveLoadTest"};
                project.RootFolder.Add(model);

                var inner = new WaveDomainData("inner");
                model.AddSubDomain(model.OuterDomain, inner);
                model.AddSubDomain(inner, new WaveDomainData("innerior"));
                projectService.SaveProjectAs(projPath);

                model.DeleteSubDomain(model.OuterDomain, inner);

                projectService.SaveProjectAs(projPath);
                projectService.CloseProject();

                // open
                project = projectService.OpenProject(projPath);
                var loadedModel = project.RootFolder.Items[0] as WaveModel;

                Assert.AreEqual(1, WaveDomainHelper.GetAllDomains(loadedModel.OuterDomain).Count);
                Assert.AreEqual(model.OuterDomain.Name, loadedModel.OuterDomain.Name);
            }
        }

        [Test]
        public void WaveBathymetryDefinitionsSaveLoadTest()
        {
            using (var app = CreateRunningApplication())
            {
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                const string projPath = "bathySaveLoadTest.dsproj";

                var model = new WaveModel {Name = "bathySaveLoadTest"};

                project.RootFolder.Add(model);

                projectService.SaveProjectAs(projPath);
                projectService.CloseProject();

                project = projectService.OpenProject(projPath);
                var loadedModel = project.RootFolder.Items[0] as WaveModel;

                IEnumerable<IDataItem> bathymetries = loadedModel.DataItems.Where(d => d.Name == loadedModel.OuterDomain.Bathymetry.Name);
                Assert.AreEqual(1, bathymetries.Count()); // TOOLS-22877: with every save the bathymetry was added as a duplicate
                Assert.AreEqual(loadedModel.OuterDomain.Bathymetry, loadedModel.DataItems.FirstOrDefault(d => d.Name == loadedModel.OuterDomain.Bathymetry.Name).Value);
            }
        }

        [Test]
        public void SaveWaveModel_AfterClearingExistingOutput_ThenWaveOutputFileIsRemoved()
        {
            // Setup
            string testDataDirectory = TestHelper.GetTestFilePath("WaveModelSaveLoadTest");
            using (var tempDirectory = new TemporaryDirectory())
            {
                string localTestDataDirectory = tempDirectory.CopyDirectoryToTempDirectory(testDataDirectory);

                string mdwFilePath = Path.Combine(localTestDataDirectory, "input", "Waves.mdw");

                using (var waveModel = new WaveModel(mdwFilePath))
                {

                    // Pre-condition
                    Assert.That(waveModel.WaveOutputData.IsConnected, Is.True);
                    Assert.That(waveModel.WaveOutputData.WavmFileFunctionStores.Any(), Is.True);
                    Assert.That(waveModel.WaveOutputData.WavhFileFunctionStores.Any(), Is.True);
                    Assert.That(waveModel.WaveOutputData.DiagnosticFiles.Any(), Is.True);
                    Assert.That(waveModel.WaveOutputData.SpectraFiles.Any(), Is.True);
                    Assert.That(waveModel.WaveOutputData.SwanFiles.Any(), Is.True);

                    string saveModelDir = tempDirectory.CreateDirectory("NewSaveLocation");

                    // Call
                    waveModel.ClearOutput();
                    waveModel.ModelSaveTo(Path.Combine(saveModelDir, "input", "Waves.mdw"), true);

                    // Assert
                    Assert.That(new DirectoryInfo(Path.Combine(saveModelDir, "output")).EnumerateFileSystemInfos(), Is.Empty);
                }
            }
        }
        
        [Test]
        public void LoadWaveModel_WithOutput_ThenOutputIsConnected()
        {
            // Setup
            string testDataDirectory = TestHelper.GetTestFilePath("WaveModelSaveLoadTest");
            using (var tempDirectory = new TemporaryDirectory())
            {
                string localTestDataDirectory = tempDirectory.CopyDirectoryToTempDirectory(testDataDirectory);

                string mdwFilePath = Path.Combine(localTestDataDirectory, "input", "Waves.mdw");

                // Call
                using (var waveModel = new WaveModel(mdwFilePath))
                {

                    // Assert
                    string outputDirectory = Path.Combine(localTestDataDirectory, "output");
                    Assert.That(waveModel.WaveOutputData.IsConnected, Is.True);

                    Assert.That(waveModel.WaveOutputData.WavmFileFunctionStores.Any(), Is.True);
                    Assert.That(waveModel.WaveOutputData.WavmFileFunctionStores.Single().Path, Is.EqualTo(Path.Combine(outputDirectory, "wavm-Waves.nc")));

                    Assert.That(waveModel.WaveOutputData.WavhFileFunctionStores.Any(), Is.True);
                    Assert.That(waveModel.WaveOutputData.WavhFileFunctionStores.Single().Path, Is.EqualTo(Path.Combine(outputDirectory, "wavh-Waves.nc")));

                    Assert.That(waveModel.WaveOutputData.DiagnosticFiles.Any(), Is.True);
                    AssertReadOnlyTextFileData(waveModel.WaveOutputData.DiagnosticFiles, "swan_bat.log", "swn-diag.Waves");

                    Assert.That(waveModel.WaveOutputData.SpectraFiles.Any(), Is.True);
                    AssertReadOnlyTextFileData(waveModel.WaveOutputData.SpectraFiles, "Waves.sp1", "Waves.sp2");

                    Assert.That(waveModel.WaveOutputData.SwanFiles.Any(), Is.True);
                    AssertReadOnlyTextFileData(waveModel.WaveOutputData.SwanFiles, "INPUT_1_20201026_000000");
                }
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
        public void ModelSaveTo_WithOutput_SavesOutput(bool switchTo)
        {
            // Setup
            string testDataDirectory = TestHelper.GetTestFilePath("WaveModelSaveLoadTest");

            using (var temp = new TemporaryDirectory())
            {
                string localTestDataDirectory = temp.CopyDirectoryToTempDirectory(testDataDirectory);

                using (var model = new WaveModel(Path.Combine(localTestDataDirectory, "input", "Waves.mdw")))
                {
                    string saveDirectory = temp.CreateDirectory("goalDirectory");

                    // Call
                    model.ModelSaveTo(Path.Combine(saveDirectory, "input", "Waves.mdw"), switchTo);

                    // Assert
                    var expectedSavedWavmFile = new FileInfo(Path.Combine(saveDirectory, "output", "wavm-Waves.nc"));
                    // If we do not switch to the directory, we export the model.
                    // Output data is never copied upon exporting, as such we 
                    // expect the saved wavm file function store to only exist
                    // if we switch to the folder.
                    Assert.That(expectedSavedWavmFile.Exists, Is.EqualTo(switchTo));

                    var expectedSavedSwanFile = new FileInfo(Path.Combine(saveDirectory, "output", "INPUT_1_20201026_000000"));
                    Assert.That(expectedSavedSwanFile.Exists, Is.EqualTo(switchTo));

                    var expectedOriginalWavmFile = new FileInfo(Path.Combine(localTestDataDirectory, "output", "wavm-Waves.nc"));
                    Assert.That(expectedOriginalWavmFile.Exists, Is.True);

                    string expectedWavmPath = switchTo ? expectedSavedWavmFile.FullName : expectedOriginalWavmFile.FullName;
                    Assert.That(model.WaveOutputData.WavmFileFunctionStores.First().Path, Is.EqualTo(expectedWavmPath));
                }
            }
        }

        private static IApplication CreateRunningApplication()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new WaveApplicationPlugin(),
            };
            
            var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
            app.Run();

            return app;
        }

        private static void AssertReadOnlyTextFileData(IEnumerable<ReadOnlyTextFileData> files, params string[] expDocumentNames)
        {
            Assert.That(files, Has.Count.EqualTo(expDocumentNames.Length));
            Assert.That(files.Select(f => f.DocumentName), Is.EquivalentTo(expDocumentNames));
        }
    }
}
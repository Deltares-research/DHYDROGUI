using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class WaveModelSaveLoadTest
    {
        private static void LoadRequiredPlugins(DeltaShellApplication app)
        {
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaveApplicationPlugin());
        }

        [Test]
        public void SaveLoadEmptyWaveModel()
        {
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

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
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

                var path = "coords.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);

                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel)app.Project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel);
                Assert.IsNotNull(retrievedModel.CoordinateSystem);
                Assert.AreEqual(model.CoordinateSystem.WKT, retrievedModel.CoordinateSystem.WKT);
            }
        }

        [Test]
        public void SaveLoadImportedWaveModel()
        {
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");

                var model = new WaveModel(mdwFilePath);
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];

                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(model.ModelDefinition.Properties.Count, retrievedModel.ModelDefinition.Properties.Count);
                Assert.AreEqual(model.ModelDefinition.BoundaryConditions.Count,
                                retrievedModel.ModelDefinition.BoundaryConditions.Count);
            }
        }

        [Test]
        public void SaveLoadImportedWaveModelTwiceWithoutEventLeaks()
        {
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

                var path = "mdw.dsproj";
                app.SaveProjectAs(path);

                var mdwFilePath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary/obw.mdw");

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

                Assert.AreEqual(retrievedModel.BoundaryConditions.Count, model.BoundaryConditions.Count);
                Assert.AreEqual(retrievedModel.Boundaries.Count, model.Boundaries.Count);
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
        [TestCase(false)]
        [TestCase(true)]
        public void SaveLoadWaveModelPersistsIsCoupledToDFlowFM(bool isCoupled)
        {
            using (var app = new DeltaShellApplication())
            {
                //Plugins needed to save the project
                LoadRequiredPlugins(app);
                app.Run();

                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                app.Project.RootFolder.Add(model);

                //Set parameter to desired value.
                model.IsCoupledToFlow = isCoupled;
                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel)app.Project.RootFolder.Items[0];
                //Check persistance
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(isCoupled, retrievedModel.IsCoupledToFlow);
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledStartTime()
        {
            using (var app = new DeltaShellApplication())
            {
                //Plugins needed to save the project
                LoadRequiredPlugins(app);
                app.Run();

                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                app.Project.RootFolder.Add(model);

                //Set parameter to desired value.
                var newStartTime = DateTime.Today;
                if (model.StartTime == newStartTime)
                    newStartTime = newStartTime.AddDays(1);
                model.StartTime = newStartTime;
                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel)app.Project.RootFolder.Items[0];
                //Check persistance
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newStartTime, retrievedModel.StartTime);
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledStopTime()
        {
            using (var app = new DeltaShellApplication())
            {
                //Plugins needed to save the project
                LoadRequiredPlugins(app);
                app.Run();

                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                app.Project.RootFolder.Add(model);

                //Set parameter to desired value.
                var newStopTime = DateTime.Today;
                if (model.StopTime == newStopTime)
                    newStopTime = newStopTime.AddDays(1);
                model.StopTime = newStopTime;
                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel)app.Project.RootFolder.Items[0];
                //Check persistance
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newStopTime, retrievedModel.StopTime);
            }
        }

        [Test]
        public void SaveLoadWaveModelPersistsCoupledTimeStep()
        {
            using (var app = new DeltaShellApplication())
            {
                //Plugins needed to save the project
                LoadRequiredPlugins(app);
                app.Run();

                var path = "mdw.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel();
                app.Project.RootFolder.Add(model);

                //Set parameter to desired value.
                var newTimeStep = TimeSpan.FromHours(1);
                if (model.TimeStep == newTimeStep)
                    newTimeStep = newTimeStep.Add(TimeSpan.FromHours(1));
                model.TimeStep = newTimeStep;
                app.SaveProjectAs(path);

                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel)app.Project.RootFolder.Items[0];
                //Check persistance
                Assert.IsNotNull(retrievedModel);
                Assert.AreEqual(newTimeStep, retrievedModel.TimeStep);
            }
        }

        [Test]
        public void CreateFromScratchAddBoundarySaveAndReload()
        {
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

                string path = "modelSaveTest.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaveModel {Name = "modelSaveTest"};
                app.Project.RootFolder.Add(model);

                var line = new LineString(new [] {new Coordinate(15, 15), new Coordinate(20, 20)});
                model.Boundaries.Add(new Feature2D {Name = "bound1", Geometry = line});
                
                // save & reload
                app.SaveProject();
                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaveModel) app.Project.RootFolder.Items[0];
                Assert.AreEqual(1, retrievedModel.Boundaries.Count, "#boundaries");
                Assert.AreEqual(1, retrievedModel.BoundaryConditions.Count, "#bcs");
            }

        }

        [Test]
        public void ImportModelSaveAsConfirmFilesAreCopiedAlong()
        {
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

                string path = "mdw_grid.dsproj";
                string secondPath = "target_mdw_grid.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");
                var model = new WaveModel(mdwFilePath);

                app.Project.RootFolder.Add(model);


                // after this call, we should go into PFBIR.Initialize(..) to get filebased items form project ???
                app.SaveProjectAs(path);


                app.SaveProjectAs(secondPath);

                var targetDir = Path.Combine(secondPath + "_data", model.Name);
                Assert.IsTrue(File.Exists(Path.Combine(targetDir, model.Name + ".mdw")));
            }
        }

        [Test]
        public void WaveAddDeleteDomainsSaveLoadTest()
        {
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

                string projPath = "modelSaveLoadDomainsTest.dsproj";
                app.SaveProjectAs(projPath); // save to initialize file repository..

                var model = new WaveModel { Name = "domainSaveLoadTest" };
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
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

                const string projPath = "bathySaveLoadTest.dsproj";
                app.SaveProjectAs(projPath); // save to initialize file repository..

                var model = new WaveModel {Name = "bathySaveLoadTest"};

                app.Project.RootFolder.Add(model);

                app.SaveProject();
                app.CloseProject();

                app.OpenProject(projPath);
                var loadedModel = app.Project.RootFolder.Items[0] as WaveModel;

                var bathymetries = loadedModel.DataItems.Where(d => d.Name == loadedModel.OuterDomain.Bathymetry.Name);
                Assert.AreEqual(1, bathymetries.Count()); // TOOLS-22877: with every save the bathymetry was added as a duplicate
                Assert.AreEqual(loadedModel.OuterDomain.Bathymetry, loadedModel.DataItems.FirstOrDefault(d => d.Name == loadedModel.OuterDomain.Bathymetry.Name).Value);
            }
        }

        [Test]
        public void SaveWaveModel_AfterClearingExistingOutput_ThenWavmOutputFileIsRemoved()
        {
            // Setup
            using (var app = new DeltaShellApplication())
            {
                LoadRequiredPlugins(app);
                app.Run();

                string testDataDirectory = TestHelper.GetTestFilePath("WaveModelSaveLoadTest");
                using (var tempDirectory = new TemporaryDirectory())
                {
                    FileUtils.CopyDirectory(testDataDirectory, tempDirectory.Path);

                    string projectFilePath = Path.Combine(tempDirectory.Path, "WaveModelWithOutput.dsproj");
                    string wavmOutputFilePath = Path.Combine(tempDirectory.Path, "WaveModelWithOutput.dsproj_data", "Waves", "wavm-Waves.nc");

                    app.OpenProject(projectFilePath);
                    app.Project.RootFolder.Models.ForEach(m => m.ClearOutput());

                    // Pre-condition
                    Assert.That(File.Exists(wavmOutputFilePath), Is.True);

                    // Call
                    app.SaveProjectAs(projectFilePath);

                    // Assert
                    Assert.That(File.Exists(wavmOutputFilePath), Is.False);

                    app.CloseProject();
                }
            }
        }

        [Test]
        public void WaveOutputSaveLoadTest()
        {
            using (var app = new DeltaShellApplication() { IsProjectCreatedInTemporaryDirectory = true })
            {
                LoadRequiredPlugins(app);
                app.Run();


                const string projPath = "outputSaveLoadTest.dsproj";
                
                var path = TestHelper.GetTestFilePath(@"obw\obw.mdw");
                var localPath = TestHelper.CreateLocalCopy(path);

                var model = new WaveModel(localPath) { Name = "outputSaveLoadTest" };

                app.Project.RootFolder.Add(model);

                var report = model.Validate();

                ActivityRunner.RunActivity(model);

                app.SaveProjectAs(projPath);
                app.CloseProject();

                app.OpenProject(projPath);
                var loadedModel = app.Project.RootFolder.Items[0] as WaveModel;

                var functionStore = loadedModel.WavmFunctionStores.FirstOrDefault();

                Assert.IsNotNull(functionStore);

                Assert.IsTrue(functionStore.Functions.First().Components[0].GetValues().Count > 0);
            }
        }

    }
}
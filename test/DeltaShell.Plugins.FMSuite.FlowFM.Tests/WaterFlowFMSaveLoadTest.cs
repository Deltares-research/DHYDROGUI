using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class WaterFlowFMSaveLoadTest
    {
        private static string GetBendProfPath()
        {
            return TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
        }

        private static IApplication CreateRunningApplication()
        {
            IApplication app = new DHYDROApplicationBuilder().WithFlowFM().Build();

            app.Run();

            return app;
        }

        private static IProjectService GetProjectServiceWithProject(IApplication app)
        {
            IProjectService projectService = app.ProjectService;
            projectService.CreateProject();

            return projectService;
        }

        [Test]
        public void SaveLoadModelEmptyModel()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";

                var model = new WaterFlowFMModel();
                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                projectService.CloseProject();
                Project project = projectService.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];

                Assert.IsTrue(retrievedModel.NetFilePath.EndsWith("_net.nc"));
                Assert.AreEqual(0, retrievedModel.BoundaryConditions.Count());
            }
        }

        [Test]
        public void SaveLoadModelVerifyStartTimeIsSaved()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu_time.dsproj";

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                projectService.Project.RootFolder.Add(model);

                var newStartTime = new DateTime(2000, 1, 2, 11, 15, 5, 2); //time with milliseconds!
                model.StartTime = newStartTime;

                var dtUserTimeSpan = new TimeSpan(0, 1, 0, 1, 430);
                model.TimeStep = dtUserTimeSpan;

                projectService.SaveProjectAs(path);
                projectService.CloseProject();
                Project project = projectService.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];

                Assert.AreEqual(newStartTime, retrievedModel.StartTime);
                Assert.AreEqual(dtUserTimeSpan, retrievedModel.TimeStep);
            }
        }

        [Test]
        public void SaveLoadModelVerifyHeatFluxModelTypeIsSaved()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdutemp.dsproj";

                var model = new WaterFlowFMModel();
                projectService.Project.RootFolder.Add(model);

                model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("3");
                Assert.AreEqual(true, model.UseTemperature);

                projectService.SaveProjectAs(path);
                projectService.CloseProject();
                Project project = projectService.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];

                Assert.AreEqual(HeatFluxModelType.ExcessTemperature, retrievedModel.HeatFluxModelType);
                Assert.AreEqual(true, retrievedModel.UseTemperature);

                retrievedModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("0");
                Assert.AreEqual(false, retrievedModel.UseTemperature);

                projectService.SaveProjectAs(path);
                projectService.CloseProject();
                project = projectService.OpenProject(path);

                retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];

                Assert.AreEqual(HeatFluxModelType.None, retrievedModel.HeatFluxModelType);
                Assert.AreEqual(false, retrievedModel.UseTemperature);
            }
        }

        [Test]
        public void ExportImportModelVerifyHeatFluxModelTypeIsExported()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdutemp.dsproj";

                var model = new WaterFlowFMModel();
                projectService.Project.RootFolder.Add(model);

                model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("3");
                Assert.AreEqual(true, model.UseTemperature);

                model.ExportTo("tempexport1\\mdutemp1.mdu", false);

                var retrievedModel = new WaterFlowFMModel("tempexport1\\mdutemp1.mdu");
                projectService.Project.RootFolder.Add(retrievedModel);

                Assert.AreEqual(HeatFluxModelType.ExcessTemperature, retrievedModel.HeatFluxModelType);
                Assert.AreEqual(true, retrievedModel.UseTemperature);

                retrievedModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("0");
                Assert.AreEqual(false, retrievedModel.UseTemperature);

                retrievedModel.ExportTo("tempexport2\\mdutemp2.mdu", false);

                retrievedModel = new WaterFlowFMModel("tempexport2\\mdutemp2.mdu");

                Assert.AreEqual(HeatFluxModelType.None, retrievedModel.HeatFluxModelType);
                Assert.AreEqual(false, retrievedModel.UseTemperature);
            }
        }

        [Test]
        public void SaveLoadModelVerifyMduIsReloaded()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);
                projectService.CloseProject();
                Project project = projectService.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];

                Assert.IsTrue(retrievedModel.NetFilePath.EndsWith("bend1_net.nc"));
                Assert.AreEqual(2, retrievedModel.BoundaryConditions.Count());
            }
        }

        [Test]
        public void SaveLoadModelVerifyMduIsReloadedAndModelDoesNotLeakEventSubscriptions()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                projectService.Project.RootFolder.Add(model);

                int subscriptionsBefore = TestReferenceHelper.FindEventSubscriptions(model);

                projectService.SaveProjectAs(path);
                projectService.SaveProject();
                projectService.SaveProject();

                Project project = projectService.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Models.First();

                int subscriptionsAfter = TestReferenceHelper.FindEventSubscriptions(retrievedModel);

                Assert.AreEqual(subscriptionsBefore, subscriptionsAfter, "event leak!");
            }
        }
        
        [Test]
        public void ImportIntoProjectVerifyGridFileIsDirectlyCopiedToDeltaShellDataDir()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";
                projectService.SaveProjectAs(path);

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);

                projectService.Project.RootFolder.Add(model);

                var netFile = Path.GetFullPath(Path.Combine(Path.Combine(path + "_data", "bendprof", "input"), "bend1_net.nc"));
                Assert.IsTrue(File.Exists(netFile), "grid file should be in the data directory after import (for rgfgrid)");
            }
        }

        [Test]
        public void ImportIntoProjectVerifyFilesNotYetSupportedInUiButReferencedInMduAreCopiedAlong()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "copyalong.dsproj";
                projectService.SaveProjectAs(path);

                var mduPath = TestHelper.GetTestFilePath(@"copyalong\manholes_1d2d.mdu");
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);

                // upon adding to project, non-memory based stuff should be copied to the project temp directory
                projectService.Project.RootFolder.Add(model);
                var tempSaveDir = Path.GetFullPath(Path.Combine(path + "_data", "manholes_1d2d", "input"));

                // check various files are copied along (even though they aren't supported yet by the UI):
                var netFile = Path.Combine(tempSaveDir, "manholes_net.nc");
                Assert.IsTrue(File.Exists(netFile), "grid file should be in the data directory after import (for rgfgrid)");

                var manholeFile = Path.Combine(tempSaveDir, "manholes.dat");
                Assert.IsTrue(File.Exists(manholeFile), "manhole file should be copied along while not yet supported in UI");

                var profdefFile = Path.Combine(tempSaveDir, "manhls_profdef.txt");
                Assert.IsTrue(File.Exists(profdefFile), "prof def file should be copied along while not yet support in UI");

                var proflocFile = Path.Combine(tempSaveDir, "manhls_profloc.xyz");
                Assert.IsTrue(File.Exists(proflocFile), "prof loc file should be copied along while not yet support in UI");
            }
        }

        [Test]
        public void SaveAsLoadModelVerifyMduIsCopiedAlong()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";
                projectService.SaveProjectAs(path);

                const string path2 = "mdu_save_as.dsproj";

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);

                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                projectService.SaveProjectAs(path2);

                projectService.CloseProject();

                Project project = projectService.OpenProject(path2);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                Assert.AreEqual(
                    Path.GetFullPath(Path.Combine(Path.Combine(path2 + "_data", "bendprof","input"), "bend1_net.nc")),
                    retrievedModel.NetFilePath);
            }
        }
        
        [Test]
        public void SaveModelVerifyGridObjectIsNotReplaced()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu_grid.dsproj";
                projectService.SaveProjectAs(path);

                string mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                
                UnstructuredGrid gridBeforeSaving = model.Grid;

                projectService.Project.RootFolder.Add(model);

                projectService.SaveProject();

                Assert.AreSame(gridBeforeSaving, model.Grid);
            }
        }

        [Test]
        public void SaveAsLoadModelVerifyGridIsCopiedAlong()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu_grid.dsproj";
                projectService.SaveProjectAs(path);

                const string path2 = "mdu_save_as_grid.dsproj";

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);

                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                projectService.SaveProjectAs(path2);

                Assert.IsTrue(File.Exists("mdu_save_as_grid.dsproj_data\\bendprof\\input\\bend1_net.nc"), "grid file does not exist");
                var retrievedModel = (WaterFlowFMModel)projectService.Project.RootFolder.Items[0];
                Assert.AreEqual(451, retrievedModel.Grid.Vertices.Count);
            }
        }

        [Test]
        public void CreateModelFromScratchModifySaveAsAndReload()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu_obs.dsproj";

                var model = new WaterFlowFMModel();
                projectService.Project.RootFolder.Add(model);

                // add obs point
                model.Area.ObservationPoints.Add(new ObservationPoint2D { Name = "obs1", Geometry = new Point(15, 15) });

                // save & reload
                projectService.SaveProjectAs(path);
                projectService.CloseProject();
                Project project = projectService.OpenProject(path);

                // check obs point still exists
                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                Assert.AreEqual(1, retrievedModel.Area.ObservationPoints.Count, "#obs points");
            }
        }

        [Test]
        public void CreateModelFromScratchSaveModifySaveAndReload()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu_resave.dsproj";

                var model = new WaterFlowFMModel();
                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs(path); // save

                // add obs point
                model.Area.ObservationPoints.Add(new ObservationPoint2D { Name = "obs1", Geometry = new Point(15, 15) });

                // save & reload
                projectService.SaveProject(); //this only works if nhibernate is aware that something changed and actually does something
                projectService.CloseProject();
                Project project = projectService.OpenProject(path);

                // check obs point still exists
                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                Assert.AreEqual(1, retrievedModel.Area.ObservationPoints.Count, "#obs points");
            }
        }

        [Test]
        public void SaveLoadBathymetryDefinitions()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu_resave.dsproj";

                var model = new WaterFlowFMModel();
                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                projectService.CloseProject();

                projectService.OpenProject(path);
            }
        }

        [Test]
        public void CreateFromScratchAddBoundarySaveAndReload()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu_drt.dsproj";

                var model = new WaterFlowFMModel { Name = "mdu_drt" };
                projectService.Project.RootFolder.Add(model);

                var line = new LineString(new [] { new Coordinate(15, 15), new Coordinate(20, 20) });

                var boundary = new Feature2D { Name = "bound1", Geometry = line };
                model.Boundaries.Add(boundary);
                model.BoundaryConditionSets[0].BoundaryConditions.Add(
                    FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));
                model.BoundaryConditions.First().AddPoint(0);

                // save & reload
                projectService.SaveProjectAs(path);
                projectService.CloseProject();
                Project project = projectService.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                Assert.AreEqual(1, retrievedModel.Boundaries.Count, "#boundaries");
                Assert.AreEqual(1, retrievedModel.BoundaryConditions.Count(), "#bcs");
            }
        }

        [Test]
        public void ImportHarlingenRunSaveAsLoadCheckOutput()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                // import
                const string path = "har.dsproj";
                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduFilePath);
                projectService.Project.RootFolder.Add(model);
                // run
                ActivityRunner.RunActivity(model);
                // save
                projectService.SaveProjectAs(path);
                
                // close
                projectService.CloseProject();

                // reopen
                Project project = projectService.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                Assert.That(retrievedModel.OutputHisFileStore.Functions[0].Components[0].Values.Count > 0);
                Assert.That(retrievedModel.OutputMapFileStore.Functions[0].Components[0].Values.Count > 0);
            }
        }

        [Test]
        public void ImportHarlingenSaveRunCloseLoadCheckOutput()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                // import
                const string path = "har.dsproj";
                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduFilePath);
                projectService.Project.RootFolder.Add(model);

                ActivityRunner.RunActivity(model);
                
                // close
                projectService.SaveProjectAs(path);
                projectService.CloseProject();

                //reopen
                Project project = projectService.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                Assert.IsNotNull(retrievedModel.OutputMapFileStore);
            }
        }
        
        [Test]
        // Test related to issue FM1D2D-2077.
        public void ImportHarlingenRunSaveAsLoadCheckLogsDoesNotContainNetworkChangedClearingResults()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                // import
                const string path = "har.dsproj";
                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduFilePath);
                projectService.Project.RootFolder.Add(model);
                // run
                ActivityRunner.RunActivity(model);
                // save
                projectService.SaveProjectAs(path);
                
                // close
                projectService.CloseProject();

                // reopen
                IEnumerable<string> logMessages = TestHelper.GetAllRenderedMessages(() => _ = projectService.OpenProject(path));
                Assert.That(logMessages, Does.Not.Contain("Network has changed, clearing results."));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveLoadHarlingenModelWithOrganizedFileStructure()
        {
            using (var app = CreateRunningApplication())
            using (var sourceDir = new TemporaryDirectory())
            using (var saveDir = new TemporaryDirectory())
            {
                string testData = TestHelper.GetTestFilePath(@"harlingen\OrganizedModel");
                string modelDir = sourceDir.CopyDirectoryToTempDirectory(testData);
                string mduPath = Path.Combine(modelDir, @"har\computations\test\har.mdu");

                var model = new WaterFlowFMModel(mduPath);
                app.ProjectService.CreateProject();
                app.ProjectService.Project.RootFolder.Add(model);

                string projectPath = Path.Combine(saveDir.Path, "har.dsproj");

                app.ProjectService.SaveProjectAs(projectPath);
                app.ProjectService.CloseProject();
                app.ProjectService.OpenProject(projectPath);

                WaterFlowFMModel loadedModel = app.ProjectService.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(loadedModel);

                ValidationReport validationResult = loadedModel.Validate();
                string loadedMduFilePath = loadedModel.MduFilePath;
                string loadedNetFilePath = loadedModel.NetFilePath;
                string loadedExtFilePath = loadedModel.ExtFilePath;
                string loadedBndExtFilePath = loadedModel.BndExtFilePath;
                
                app.ProjectService.CloseProject();
                
                Assert.That(validationResult.ErrorCount, Is.Zero, "Model validation failed after loading the saved Harlingen model.");
                Assert.That(loadedMduFilePath, Is.EqualTo(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\har.mdu")));
                Assert.That(loadedNetFilePath, Is.EqualTo(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\fm_003_net.nc")));
                Assert.That(loadedExtFilePath, Is.EqualTo(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\001.ext")));
                Assert.That(loadedBndExtFilePath, Is.EqualTo(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\001_bnd.ext")));
                
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\network_bounds_d3d.pol"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\network_bounds_d3d_add.pol"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\001.ext"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\001_bnd.ext"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\071_01.pli"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\071_02.pli"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\071_03.pli"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\Discharge.bc"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\L1.pli"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\Salinity.bc"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\boundary_conditions\test\WaterLevel.bc"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\fm_003_net.nc"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\har.mdu"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\roughness-Channels.ini"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\roughness-Main.ini"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\roughness-Manning_0.01667.ini"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\roughness-Sewer.ini"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\computations\test\roughness-Strickler_15.0.ini"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\general\fourier_max.fou"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\geometry\cross_sections\har_crs_V2_crs.pli"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\geometry\output_locations\har_fine_V3_obs.xyn"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\geometry\fixedweir_fxw.pli"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\geometry\har_enc.pol"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\geometry\Harlingen_haven.ldb"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\geometry\thindam_thd.pli"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\initial_conditions\test\initialFields.ini"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\initial_conditions\test\bedlevel.xyz"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\initial_conditions\test\frictioncoefficient_friction.pol"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\initial_conditions\test\InitialWaterdepth.ini"), Does.Exist);
                Assert.That(Path.Combine(saveDir.Path, @"har.dsproj_data\har\input\initial_conditions\test\structures.ini"), Does.Exist);
                
                string ini = File.ReadAllText(loadedMduFilePath);
                
                IniData iniData = new IniParser().Parse(ini);
                IReadOnlyList<IniProperty> iniProperties = iniData.Sections.SelectMany(section => section.Properties).ToList();

                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "NetFile" && p.Value == "fm_003_net.nc"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "GridEnclosureFile" && p.Value == "../../geometry/har_enc.pol"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "LandBoundaryFile" && p.Value == "../../geometry/Harlingen_haven.ldb"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "ThinDamFile" && p.Value == "../../geometry/thindam_thd.pli"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "FixedWeirFile" && p.Value == "../../geometry/fixedweir_fxw.pli"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "IniFieldFile" && p.Value == "../../initial_conditions/test/initialFields.ini"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "StructureFile" && p.Value == "../../initial_conditions/test/structures.ini"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "FrictFile" && p.Value == "roughness-Channels.ini;roughness-Main.ini;roughness-Sewer.ini;roughness-Manning_0.01667.ini;roughness-Strickler_15.0.ini"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "ExtForceFile" && p.Value == "../../boundary_conditions/test/001.ext"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "ExtForceFileNew" && p.Value == "../../boundary_conditions/test/001_bnd.ext"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "ObsFile" && p.Value == "../../geometry/output_locations/har_fine_V3_obs.xyn"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "CrsFile" && p.Value == "../../geometry/cross_sections/har_crs_V2_crs.pli"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "FouFile" && p.Value == "../../general/fourier_max.fou"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "HisFile" && p.Value == "001_his.nc"));
                Assert.That(iniProperties, Has.One.Matches<IniProperty>(p => p.Key == "MapFile" && p.Value == "001_map.nc"));
            }
        }

        [Test]
        public void DeleteDataPointSaveLoadShouldNotKeepDataPoint()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";
                
                var model = new WaterFlowFMModel();

                projectService.Project.RootFolder.Add(model);

                var feature2D = new Feature2D
                {
                    Name = "bnd",
                    Geometry =
                        new LineString(new[]
                        {new Coordinate(0, 0), new Coordinate(0, 1), new Coordinate(1, 1), new Coordinate(1, 2)})
                };

                model.Boundaries.Add(feature2D);
                model.BoundaryConditionSets[0].BoundaryConditions.Add(
                    FlowBoundaryConditionFactory.CreateBoundaryCondition(feature2D));

                Assert.AreEqual(1, model.BoundaryConditionSets.Count);
                Assert.AreEqual(1, model.BoundaryConditions.Count());

                var waterLevelBoundaryCondition = model.BoundaryConditions.First();
                waterLevelBoundaryCondition.AddPoint(0);
                waterLevelBoundaryCondition.AddPoint(1);
                projectService.SaveProjectAs(path);
                projectService.CloseProject();

                Project project = projectService.OpenProject(path);
                var loadedModel = project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(loadedModel);
                Assert.AreEqual(1, loadedModel.BoundaryConditionSets.Count);
                Assert.AreEqual(1, loadedModel.BoundaryConditions.Count());
                Assert.AreEqual(new[] {0, 1}, loadedModel.BoundaryConditions.First().DataPointIndices);

                loadedModel.BoundaryConditions.First().RemovePoint(0);
                projectService.SaveProject();
                projectService.CloseProject();

                project = projectService.OpenProject(path);
                var secondLoadedModel = project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(secondLoadedModel);
                Assert.AreEqual(1, secondLoadedModel.BoundaryConditionSets.Count);
                Assert.AreEqual(1, secondLoadedModel.BoundaryConditions.Count());
                Assert.AreEqual(new[] {1}, secondLoadedModel.BoundaryConditions.First().DataPointIndices);
            }
        }

        [Test]
        public void SaveModelBuiltFromScratchWithWind()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";

                var model = new WaterFlowFMModel();

                projectService.Project.RootFolder.Add(model);

                model.WindFields.Add(SpiderWebWindField.Create(TestHelper.GetTestFilePath(@"windtest\wind.spw")));

                projectService.SaveProjectAs("windtest.dsproj");
                projectService.CloseProject();

                Project project = projectService.OpenProject("windtest.dsproj");
                var loadedModel = project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(loadedModel);
                Assert.AreEqual("wind.spw", Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().Path));
                Assert.IsTrue(File.Exists(@"windtest.dsproj_data\FlowFM\input\wind.spw"));
            }
        }

        [Test]
        public void SaveLoadSaveAsSaveShouldCopyWindFiles()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                var model = new WaterFlowFMModel();

                projectService.Project.RootFolder.Add(model);
                model.WindFields.Add(SpiderWebWindField.Create(TestHelper.GetTestFilePath(@"windtest\wind.spw")));

                projectService.SaveProjectAs("windtest.dsproj");
                projectService.CloseProject();

                Project project = projectService.OpenProject("windtest.dsproj");
                var loadedModel = project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(loadedModel);

                loadedModel.WindFields.RemoveAt(0);
                loadedModel.WindFields.Add(SpiderWebWindField.Create(TestHelper.GetTestFilePath(@"windtest\wind2.spw")));

                projectService.SaveProjectAs("windtest2.dsproj");
                projectService.CloseProject();

                project = projectService.OpenProject("windtest.dsproj");
                loadedModel = project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(loadedModel);
                Assert.AreEqual("wind.spw", Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().WindFilePath));
                Assert.IsTrue(File.Exists(@"windtest.dsproj_data\FlowFM\input\wind.spw"));

                project = projectService.OpenProject("windtest2.dsproj");
                loadedModel = project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(loadedModel);
                Assert.AreEqual("wind2.spw", Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().WindFilePath));
                Assert.IsTrue(File.Exists(@"windtest2.dsproj_data\FlowFM\input\wind2.spw"));
            }
        }

        [Test]
        public void SaveLoadVerifyAreaFeatures()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                var model = new WaterFlowFMModel();

                // Embankments are mapped in dbase (and will also be written to file..)
                model.Area.Embankments.Add(new Embankment { Name = "embankment", Region = model.Area, Geometry = new LineString(new[] { new Coordinate(10, 10), new Coordinate(-10,10) }) });

                // Thin Dams are written to file and file only
                model.Area.ThinDams.Add(new ThinDam2D {Name = "thin", Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(1,1)})});

                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs("saveLoadAreaFeaturesTest.dsproj");
                projectService.CloseProject();

                Project project = projectService.OpenProject("saveLoadAreaFeaturesTest.dsproj");

                var loadedModel = project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;
                Assert.AreEqual(1, loadedModel.Area.Embankments.Count);
                Assert.AreEqual(1, loadedModel.Area.ThinDams.Count);
            }
        }

        [Test]
        public void SaveLoadFixedWeirTest()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";

                var model = new WaterFlowFMModel();
                projectService.Project.RootFolder.Add(model);

                var fixedWeir = new FixedWeir
                {
                    Name = "fixed weir",
                    Geometry =
                        new LineString(new [] { new Coordinate(0.0, 0.0), new Coordinate(0.3, 0.3), new Coordinate(0.6, 1.3) })
                };
                
                var hydroArea = new HydroArea();
                model.Area = hydroArea;
                hydroArea.FixedWeirs.Add(fixedWeir);
                model.FixedWeirsProperties[0].DataColumns[0].ValueList[0] = 0.9876;
                projectService.SaveProjectAs(path);

                projectService.CloseProject();

                Project project = projectService.OpenProject(path);

                WaterFlowFMModel loadedModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

                Assert.IsNotNull(loadedModel);
                Assert.IsNotNull(loadedModel.Area.FixedWeirs.First());
                Assert.AreEqual(0.9876, loadedModel.FixedWeirsProperties[0].DataColumns[0].ValueList[0]);
            }
        }

        [Test]
        public void SaveLoadDeleteGridTest()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu_grid.dsproj";

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                projectService.CloseProject();

                Project project = projectService.OpenProject(path);

                WaterFlowFMModel loadedModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

                Assert.NotNull(loadedModel);

                loadedModel.RemoveGrid();

                Assert.NotNull(loadedModel.NetFilePath);
                Assert.AreEqual(0, loadedModel.Grid.Cells.Count);
            }
        }

        [Test]
        public void SaveAndLoadMorphologySedimentSimpleModel()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";

                var model = new WaterFlowFMModel();
                //Enable Morphology and Sediment
                model.ModelDefinition.UseMorphologySediment = true;
                projectService.Project.RootFolder.Add(model);

                projectService.SaveProjectAs(path);

                //Check sed and mor files exist
                var morFile = model.MorFilePath;
                var sedFile = model.SedFilePath;

                Assert.IsTrue(File.Exists(morFile));
                Assert.IsTrue(File.Exists(sedFile));
                projectService.CloseProject();

                Project project = projectService.OpenProject(path);

                WaterFlowFMModel loadedModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

                Assert.IsNotNull(loadedModel);
                
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenFmModel_WhenAddingAndRemovingAHydroAreaFeature_ThenFeatureFileReferenceIsRemovedFromTheMduFile()
        {
            var filePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\BasicModel\FlowFM.mdu");
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath));

            using (var fmModel = new WaterFlowFMModel(filePath))
            {
                var obsFileModelProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.ObsFile);
                var dryPointsFileModelProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.DryPointsFile);
                var modelArea = fmModel.Area;
                Assert.That(modelArea.ObservationPoints.Count, Is.EqualTo(3));
                Assert.That(modelArea.DryPoints.Count, Is.EqualTo(1));
                Assert.That(obsFileModelProperty.GetValueAsString(), Is.EqualTo("FlowFM_obs.xyn"));
                Assert.That(dryPointsFileModelProperty.GetValueAsString(), Is.EqualTo("FlowFM_dry.xyz"));

                modelArea.ObservationPoints.Clear();
                modelArea.DryPoints.Clear();
                fmModel.ExportTo(filePath);
                Assert.IsEmpty(obsFileModelProperty.GetValueAsString());
                Assert.IsEmpty(dryPointsFileModelProperty.GetValueAsString());
            }

            var importedModel = new WaterFlowFMModel(filePath);
            Assert.IsEmpty(importedModel.Area.ObservationPoints);
            Assert.IsEmpty(importedModel.Area.DryPoints);
        }

        [Test]
        public void SaveAndLoadMorphologySedimentParametersSimpleModel()
        {
            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                const string path = "mdu.dsproj";

                var model = new WaterFlowFMModel();
                //Enable Morphology and Sediment
                model.ModelDefinition.UseMorphologySediment = true;
                projectService.Project.RootFolder.Add(model);

                model.SedimentOverallProperties.Add(new SedimentProperty<double>("MyOverall Property", 0,0, false, 1, false, string.Empty,string.Empty, false) {Value = 0.5});
                var fraction = new SedimentFraction();
                var sedType = fraction.AvailableSedimentTypes.ElementAtOrDefault(1);
                Assert.IsNotNull(sedType);
                /*var sedTypeFirstDoubleProperty = sedType.Properties.OfType<SedimentProperty<double>>().FirstOrDefault();
                Assert.IsNotNull(sedTypeFirstDoubleProperty);*/
                var sedTypeFirstDoubleProperty = new SedimentProperty<double>("MySediment Property", 2, 1.5, false, 2.5, false, string.Empty, String.Empty, false);
                sedTypeFirstDoubleProperty.Value = new Random().NextDouble() * (sedTypeFirstDoubleProperty.MaxValue - sedTypeFirstDoubleProperty.MinValue) + sedTypeFirstDoubleProperty.MinValue;

                projectService.SaveProjectAs(path);

                //Check sed and mor files exist
                var morFile = model.MorFilePath;
                var sedFile = model.SedFilePath;

                Assert.IsTrue(File.Exists(morFile));
                Assert.IsTrue(File.Exists(sedFile));
                projectService.CloseProject();

                Project project = projectService.OpenProject(path);

                WaterFlowFMModel loadedModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

                Assert.IsNotNull(loadedModel);
                
            }
        }

        [Test]
        [TestCase(@"c01_trench_VanRijn1993\test.mdu")]
        [TestCase(@"c02_trench_EH\test.mdu")]
        [TestCase(@"c03_fractions\test.mdu")]
        [TestCase(@"c04_concentration_bnds\test.mdu")]
        [TestCase(@"c06_spatiald50\test.mdu")]
        [TestCase(@"c07_sedimentavailability\test.mdu")]
        [TestCase(@"c08_moroutput_NETCDF\test.mdu")]
        [TestCase(@"c09_moroutput_UGRID\test.mdu")]
        [TestCase(@"c10_fixed_layer\test.mdu")]
        [TestCase(@"c12_trench_VanRijn1993_sandbnd\test.mdu")]
        [TestCase(@"c13_bedload_bcm_depth_function_of_time\test.mdu")]
        [TestCase(@"c14_morphology_with_weir\test.mdu")]
        [TestCase(@"c15_morphology_with_weir_no_bedupd\test.mdu")]
        [TestCase(@"c16_morphology_off_beyer004_contr10_bedlevtype1\test.mdu")]
        [TestCase(@"c17_bedslope_effect_slope022\test.mdu")]
        [TestCase(@"c18_bedslope_effect_slope044\test.mdu")]
        [TestCase(@"c19_bedslope_effect_slope-044_invDir\test.mdu")]
        [TestCase(@"c20_bedslope_effect_slope044_AShld0425\test.mdu")]
        [TestCase(@"c21_boundary_condition_icond2\test.mdu")]
        [TestCase(@"c22_boundary_condition_icond2_constant\test.mdu")]
        [TestCase(@"c23_boundary_condition_icond3\test.mdu")]
        [TestCase(@"c24_boundary_condition_icond3_constant\test.mdu")]
        [TestCase(@"c25_boundary_condition_icond4\test.mdu")]
        [TestCase(@"c26_boundary_condition_icond4_graded\test.mdu")]
        [TestCase(@"c27_boundary_condition_icond4_none\test.mdu")]
        [TestCase(@"c28_boundary_condition_icond2_graded\test.mdu")]
        [TestCase(@"c29_curvebend_spiral\test.mdu")]
        [TestCase(@"c30_curvebend_spiral_mor\test.mdu")]
        [TestCase(@"c31_curvebend_spiral_mor_Bedup\test.mdu")]
        [TestCase(@"c35_dad_inside_dumping\test.mdu")]
        [TestCase(@"c36_dad_outside_dumping\test.mdu")]
        [TestCase(@"c37_dad_2dredge1dump\test.mdu")]
        [TestCase(@"c38_dad_dumpisdredge\test.mdu")]
        [TestCase(@"c39_dad_sandmining\test.mdu")]
        [TestCase(@"c40_dad_inside_dumping_layered_bed\test.mdu")]
        [TestCase(@"c41_dad_outside_dumping_layered_bed\test.mdu")]
        [TestCase(@"c42_dad_2dredge1dump_layered_bed\test.mdu")]
        [TestCase(@"c43_daddumpisdredge_layered_bed\test.mdu")]
        [TestCase(@"c44_dad_sandmining_layered_bed\test.mdu")]
        [TestCase(@"c46_dad_Sediment_nourishment\test.mdu")]
        public void OpenSaveAndLoadAllMorphologySedimentTestModels(string mduTestPath)
        {
            var testModelPath = Path.Combine(@"MorphologySediment_Models\", mduTestPath);
            var mduPath = TestHelper.GetTestFilePath(testModelPath);
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            using (IApplication app = CreateRunningApplication())
            {
                IProjectService projectService = GetProjectServiceWithProject(app);

                var model = new WaterFlowFMModel(mduPath);
                //Enable Morphology and Sediment
                model.ModelDefinition.UseMorphologySediment = true;
                projectService.Project.RootFolder.Add(model);

                string projectPath = mduPath + ".dsproj";
                projectService.SaveProjectAs(projectPath);
                projectService.CloseProject();

                Project project = projectService.OpenProject(projectPath);

                WaterFlowFMModel loadedModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

                Assert.IsNotNull(loadedModel);

            }
        }

        [Test]
        [TestCase(@"MorAndSedFile.mdu", true)]
        [TestCase(@"MorFileAndNoSedFile.mdu", true)] 
        [TestCase(@"NoMorFileAndSedFile.mdu", false)]
        [TestCase(@"NoMorFileAndNoSedFile.mdu", false)]
        public void LoadMduDeterminesIfUseMorSed(string mduTestPath, bool morSedEnabled)
        {
            /*
             * We only set to True UseMorSed if the *.mor (morphology) file is present 
             * In any other case we do not care.             
             */
            var testModelPath = Path.Combine(@"MorphologySediment_Models\SimpleModels\", mduTestPath);
            var mduPath = TestHelper.GetTestFilePath(testModelPath);
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(mduPath))
            {
                Assert.AreEqual(morSedEnabled, model.UseMorSed);
            }
        }
    }
}
using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class WaterFlowFMSaveLoadTest
    {
        private IApplication app;

        [SetUp]
        public void SetUp()
        {
            app = DeltaShellCoreFactory.CreateApplication();
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
        }

        [TearDown]
        public void TearDown()
        {
            app.Dispose();
        }

        [Test]
        public void SaveLoadModelEmptyModel()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(path);

            app.CloseProject();
            app.OpenProject(path);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

            Assert.IsTrue(retrievedModel.NetFilePath.EndsWith("_net.nc"));
            Assert.AreEqual(0, retrievedModel.BoundaryConditions.Count());
        }

        [Test]
        public void SaveLoadModelVerifyStartTimeIsSaved()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu_time.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            app.Project.RootFolder.Add(model);

            var newStartTime = new DateTime(2000, 1, 2, 11, 15, 5);
            model.StartTime = newStartTime;

            var dtUserTimeSpan = new TimeSpan(0, 1, 0, 1, 430);
            model.TimeStep = dtUserTimeSpan;

            app.SaveProjectAs(path);
            app.CloseProject();
            app.OpenProject(path);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

            Assert.AreEqual(newStartTime, retrievedModel.StartTime);
            Assert.AreEqual(dtUserTimeSpan, retrievedModel.TimeStep);
        }

        [Test]
        public void SaveLoadModelVerifyHeatFluxModelTypeIsSaved()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdutemp.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            app.Project.RootFolder.Add(model);

            model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("3");
            Assert.AreEqual(true, model.UseTemperature);

            app.SaveProjectAs(path);
            app.CloseProject();
            app.OpenProject(path);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

            Assert.AreEqual(HeatFluxModelType.ExcessTemperature, retrievedModel.HeatFluxModelType);
            Assert.AreEqual(true, retrievedModel.UseTemperature);

            retrievedModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("0");
            Assert.AreEqual(false, retrievedModel.UseTemperature);

            app.SaveProjectAs(path);
            app.CloseProject();
            app.OpenProject(path);

            retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

            Assert.AreEqual(HeatFluxModelType.None, retrievedModel.HeatFluxModelType);
            Assert.AreEqual(false, retrievedModel.UseTemperature);
        }

        [Test]
        public void ExportImportModelVerifyHeatFluxModelTypeIsExported()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdutemp.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            app.Project.RootFolder.Add(model);

            model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("3");
            Assert.AreEqual(true, model.UseTemperature);

            model.ExportTo("tempexport1\\mdutemp1.mdu", false);

            var retrievedModel = new WaterFlowFMModel();
            retrievedModel.ImportFromMdu("tempexport1\\mdutemp1.mdu");

            app.Project.RootFolder.Add(retrievedModel);

            Assert.AreEqual(HeatFluxModelType.ExcessTemperature, retrievedModel.HeatFluxModelType);
            Assert.AreEqual(true, retrievedModel.UseTemperature);

            retrievedModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("0");
            Assert.AreEqual(false, retrievedModel.UseTemperature);

            retrievedModel.ExportTo("tempexport2\\mdutemp2.mdu", false);

            retrievedModel = new WaterFlowFMModel();
            retrievedModel.ImportFromMdu("tempexport2\\mdutemp2.mdu");

            Assert.AreEqual(HeatFluxModelType.None, retrievedModel.HeatFluxModelType);
            Assert.AreEqual(false, retrievedModel.UseTemperature);
        }

        [Test]
        public void SaveLoadModelVerifyMduIsReloaded()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(path);
            app.CloseProject();
            app.OpenProject(path);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];

            Assert.IsTrue(retrievedModel.NetFilePath.EndsWith("bend1_net.nc"));
            Assert.AreEqual(2, retrievedModel.BoundaryConditions.Count());
        }

        [Test]
        public void SaveLoadModelVerifyMduIsReloadedAndModelDoesNotLeakEventSubscriptions()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            app.Project.RootFolder.Add(model);

            int subscriptionsBefore = TestReferenceHelper.FindEventSubscriptions(model);

            app.SaveProjectAs(path);
            app.SaveProject();
            app.SaveProject();

            app.OpenProject(path);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Models.First();

            int subscriptionsAfter = TestReferenceHelper.FindEventSubscriptions(retrievedModel);

            Assert.AreEqual(subscriptionsBefore, subscriptionsAfter, "event leak!");
        }

        [Test]
        public void ImportIntoProjectVerifyGridFileIsDirectlyCopiedToDeltaShellDataDir()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path);

            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            app.Project.RootFolder.Add(model);

            string netFile = Path.GetFullPath(Path.Combine(path + "_data", "bendprof", "input", "bend1_net.nc"));
            Assert.IsTrue(File.Exists(netFile), "grid file should be in the data directory after import (for rgfgrid)");
        }

        [Test]
        public void ImportIntoProjectVerifyFilesNotYetSupportedInUiButReferencedInMduAreCopiedAlong()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "copyalong.dsproj";
            app.SaveProjectAs(path);

            string mduPath = TestHelper.GetTestFilePath(@"copyalong\manholes.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            // upon adding to project, non-memory based stuff should be copied to the project temp directory
            app.Project.RootFolder.Add(model);
            string tempSaveInputDir = Path.GetFullPath(Path.Combine(path + "_data", "manholes", "input"));

            // check various files are copied along (even though they aren't supported yet by the UI):
            string netFile = Path.Combine(tempSaveInputDir, "manholes_net.nc");
            Assert.IsTrue(File.Exists(netFile), "grid file should be in the data directory after import (for rgfgrid)");

            string manholeFile = Path.Combine(tempSaveInputDir, "manholes.dat");
            Assert.IsTrue(File.Exists(manholeFile),
                          "manhole file should be copied along while not yet supported in UI");

            string profdefFile = Path.Combine(tempSaveInputDir, "manhls_profdef.txt");
            Assert.IsTrue(File.Exists(profdefFile), "prof def file should be copied along while not yet support in UI");

            string proflocFile = Path.Combine(tempSaveInputDir, "manhls_profloc.xyz");
            Assert.IsTrue(File.Exists(proflocFile), "prof loc file should be copied along while not yet support in UI");
        }

        [Test]
        public void SaveAsLoadModelVerifyMduIsCopiedAlong()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path);

            const string path2 = "mdu_save_as.dsproj";

            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(path);

            app.SaveProjectAs(path2);

            app.CloseProject();

            app.OpenProject(path2);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
            Assert.AreEqual(
                Path.GetFullPath(Path.Combine(path2 + "_data", "bendprof", "input", "bend1_net.nc")),
                retrievedModel.NetFilePath);
        }

        [Test]
        public void SaveAsLoadModelVerifyGridIsCopiedAlong()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu_grid.dsproj";
            app.SaveProjectAs(path);

            const string path2 = "mdu_save_as_grid.dsproj";

            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(path);

            app.SaveProjectAs(path2);

            Assert.IsTrue(File.Exists("mdu_save_as_grid.dsproj_data\\bendprof\\input\\bend1_net.nc"),
                          "grid file does not exist");
            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
            Assert.AreEqual(451, retrievedModel.Grid.Vertices.Count);
        }

        [Test]
        public void CreateModelFromScratchModifySaveAsAndReload()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu_obs.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            app.Project.RootFolder.Add(model);

            // add obs point
            model.Area.ObservationPoints.Add(new GroupableFeature2DPoint
            {
                Name = "obs1",
                Geometry = new Point(15, 15)
            });

            // save & reload
            app.SaveProjectAs(path);
            app.CloseProject();
            app.OpenProject(path);

            // check obs point still exists
            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
            Assert.AreEqual(1, retrievedModel.Area.ObservationPoints.Count, "#obs points");
        }

        [Test]
        public void CreateModelFromScratchSaveModifySaveAndReload()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu_resave.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(path); // save

            // add obs point
            model.Area.ObservationPoints.Add(new GroupableFeature2DPoint
            {
                Name = "obs1",
                Geometry = new Point(15, 15)
            });

            // save & reload
            app.SaveProject(); //this only works if nhibernate is aware that something changed and actually does something
            app.CloseProject();
            app.OpenProject(path);

            // check obs point still exists
            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
            Assert.AreEqual(1, retrievedModel.Area.ObservationPoints.Count, "#obs points");
        }

        [Test]
        public void SaveLoadBathymetryDefinitions()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu_resave.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(path);

            app.CloseProject();

            app.OpenProject(path);
        }

        [Test]
        public void CreateFromScratchAddBoundarySaveAndReload()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu_drt.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel { Name = "mdu_drt" };
            app.Project.RootFolder.Add(model);

            var line = new LineString(new[]
            {
                new Coordinate(15, 15),
                new Coordinate(20, 20)
            });

            var boundary = new Feature2D
            {
                Name = "bound1",
                Geometry = line
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));
            model.BoundaryConditions.First().AddPoint(0);

            // save & reload
            app.SaveProjectAs(path);
            app.CloseProject();
            app.OpenProject(path);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
            Assert.AreEqual(1, retrievedModel.Boundaries.Count, "#boundaries");
            Assert.AreEqual(1, retrievedModel.BoundaryConditions.Count(), "#bcs");
        }

        [Test]
        public void ImportHarlingenRunSaveAsLoadCheckOutput()
        {
            app.Run();
            app.CreateNewProject();

            // import
            const string path = "har.dsproj";
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);

            app.Project.RootFolder.Add(model);
            // run
            ActivityRunner.RunActivity(model);
            // save
            app.SaveProjectAs(path);

            // close
            app.CloseProject();

            // reopen
            app.OpenProject(path);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
            Assert.That(retrievedModel.OutputHisFileStore.Functions[0].Components[0].Values.Count > 0);
            Assert.That(retrievedModel.OutputMapFileStore.Functions[0].Components[0].Values.Count > 0);
        }

        [Test]
        public void ImportHarlingenSaveRunCloseLoadCheckOutput()
        {
            app.Run();
            app.CreateNewProject();

            // import
            const string path = "har.dsproj";
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);

            app.Project.RootFolder.Add(model);

            ActivityRunner.RunActivity(model);

            // close
            app.SaveProjectAs(path);
            app.CloseProject();

            //reopen
            app.OpenProject(path);

            var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
            Assert.IsNotNull(retrievedModel.OutputMapFileStore);
        }

        [Test]
        public void DeleteDataPointSaveLoadShouldNotKeepDataPoint()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";

            var model = new WaterFlowFMModel();

            app.Project.RootFolder.Add(model);

            var feature2D = new Feature2D
            {
                Name = "bnd",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, 1),
                        new Coordinate(1, 1),
                        new Coordinate(1, 2)
                    })
            };

            model.Boundaries.Add(feature2D);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(feature2D));

            Assert.AreEqual(1, model.BoundaryConditionSets.Count);
            Assert.AreEqual(1, model.BoundaryConditions.Count());

            IBoundaryCondition waterLevelBoundaryCondition = model.BoundaryConditions.First();
            waterLevelBoundaryCondition.AddPoint(0);
            waterLevelBoundaryCondition.AddPoint(1);
            app.SaveProjectAs(path);
            app.CloseProject();

            app.OpenProject(path);
            var loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

            Assert.IsNotNull(loadedModel);
            Assert.AreEqual(1, loadedModel.BoundaryConditionSets.Count);
            Assert.AreEqual(1, loadedModel.BoundaryConditions.Count());
            Assert.AreEqual(new[]
            {
                0,
                1
            }, loadedModel.BoundaryConditions.First().DataPointIndices);

            loadedModel.BoundaryConditions.First().RemovePoint(0);
            app.SaveProject();
            app.CloseProject();

            app.OpenProject(path);
            var secondLoadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

            Assert.IsNotNull(secondLoadedModel);
            Assert.AreEqual(1, secondLoadedModel.BoundaryConditionSets.Count);
            Assert.AreEqual(1, secondLoadedModel.BoundaryConditions.Count());
            Assert.AreEqual(new[]
            {
                1
            }, secondLoadedModel.BoundaryConditions.First().DataPointIndices);
        }

        [Test]
        public void SaveModelBuiltFromScratchWithWind()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();

            app.Project.RootFolder.Add(model);

            model.WindFields.Add(SpiderWebWindField.Create(TestHelper.GetTestFilePath(@"windtest\wind.spw")));

            app.SaveProjectAs("windtest.dsproj");
            app.CloseProject();

            app.OpenProject("windtest.dsproj");
            var loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

            Assert.IsNotNull(loadedModel);
            Assert.AreEqual(
                "wind.spw", Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().Path));
            Assert.IsTrue(File.Exists(@"windtest.dsproj_data\FlowFM\input\wind.spw"));
        }

        [Test]
        public void SaveLoadSaveAsSaveShouldCopyWindFiles()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();

            app.Project.RootFolder.Add(model);
            model.WindFields.Add(SpiderWebWindField.Create(TestHelper.GetTestFilePath(@"windtest\wind.spw")));

            app.SaveProjectAs("windtest.dsproj");
            app.CloseProject();

            app.OpenProject("windtest.dsproj");
            var loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

            Assert.IsNotNull(loadedModel);

            loadedModel.WindFields.RemoveAt(0);
            loadedModel.WindFields.Add(SpiderWebWindField.Create(TestHelper.GetTestFilePath(@"windtest\wind2.spw")));

            app.SaveProjectAs("windtest2.dsproj");
            app.CloseProject();

            app.OpenProject("windtest.dsproj");
            loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

            Assert.IsNotNull(loadedModel);
            Assert.AreEqual(
                "wind.spw", Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().WindFilePath));
            Assert.IsTrue(File.Exists(@"windtest.dsproj_data\FlowFM\input\wind.spw"));

            app.OpenProject("windtest2.dsproj");
            loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

            Assert.IsNotNull(loadedModel);
            Assert.AreEqual("wind2.spw",
                            Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().WindFilePath));
            Assert.IsTrue(File.Exists(@"windtest2.dsproj_data\FlowFM\input\wind2.spw"));
        }

        [Test]
        public void TestRunWithGate()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            string mduPath = TestHelper.GetTestFilePath(@"structures_gate\structsFM.dsproj_data\har\har.mdu");

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
            // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
            // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
            // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
            model.Area.Structures.Select(c =>
            {
                c.CrestWidth = 1.0;
                return c;
            }).ToList();
            model.StopTime = model.StartTime.AddMinutes(15);

            Assert.IsTrue(model.Area.Structures.Where(w => w.Formula is SimpleGateFormula).ToList().Count > 0);

            app.Project.RootFolder.Add(model);

            ActivityRunner.RunActivity(model);

            Assert.AreNotEqual(ActivityStatus.Failed, model.Status);

            // close
            app.CloseProject();
        }

        [Test]
        public void SaveLoadVerifyAreaFeatures()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();

            // Thin Dams are written to file and file only
            model.Area.ThinDams.Add(new ThinDam2D
            {
                Name = "thin",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 1)
                })
            });

            app.Project.RootFolder.Add(model);

            app.SaveProjectAs("saveLoadAreaFeaturesTest.dsproj");
            app.CloseProject();

            app.OpenProject("saveLoadAreaFeaturesTest.dsproj");

            var loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;
            Assert.AreEqual(1, loadedModel.Area.ThinDams.Count);
        }

        [Test]
        public void SaveLoadFixedWeirTest()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            app.Project.RootFolder.Add(model);

            var fixedWeir = new FixedWeir
            {
                Name = "fixed weir",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0.0, 0.0),
                        new Coordinate(0.3, 0.3),
                        new Coordinate(0.6, 1.3)
                    })
            };

            var hydroArea = new HydroArea();
            model.Area = hydroArea;
            hydroArea.FixedWeirs.Add(fixedWeir);
            model.FixedWeirsProperties.ElementAt(0).DataColumns[0].ValueList[0] = 0.9876;
            app.SaveProjectAs(path);

            app.CloseProject();

            app.OpenProject(path);

            WaterFlowFMModel loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

            Assert.IsNotNull(loadedModel);
            Assert.IsNotNull(loadedModel.Area.FixedWeirs.First());
            Assert.AreEqual(0.9876, loadedModel.FixedWeirsProperties.ElementAt(0).DataColumns[0].ValueList[0]);
        }

        [Test]
        public void SaveLoadDeleteGridTest()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu_grid.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            string mduPath = GetBendProfPath();
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(path);

            app.CloseProject();

            app.OpenProject(path);

            WaterFlowFMModel loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

            Assert.NotNull(loadedModel);

            loadedModel.RemoveGrid();

            Assert.NotNull(loadedModel.NetFilePath);
            Assert.AreEqual(0, loadedModel.Grid.Cells.Count);
        }

        [Test]
        public void SaveAndLoadMorphologySedimentSimpleModel()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            //Enable Morphology and Sediment
            model.ModelDefinition.UseMorphologySediment = true;
            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(path);

            //Check sed and mor files exist
            string morFile = model.MorFilePath;
            string sedFile = model.SedFilePath;

            Assert.IsTrue(File.Exists(morFile));
            Assert.IsTrue(File.Exists(sedFile));
            app.CloseProject();

            app.OpenProject(path);

            WaterFlowFMModel loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

            Assert.IsNotNull(loadedModel);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void
            GivenFmModel_WhenAddingAndRemovingAHydroAreaFeature_ThenFeatureFileReferenceIsRemovedFromTheMduFile()
        {
            string filePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\BasicModel\FlowFM.mdu");
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath));

            using (var fmModel = new WaterFlowFMModel())
            {
                fmModel.ImportFromMdu(filePath);

                WaterFlowFMProperty obsFileModelProperty =
                    fmModel.ModelDefinition.GetModelProperty(KnownProperties.ObsFile);
                WaterFlowFMProperty dryPointsFileModelProperty =
                    fmModel.ModelDefinition.GetModelProperty(KnownProperties.DryPointsFile);
                HydroArea modelArea = fmModel.Area;
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

            var importedModel = new WaterFlowFMModel();
            importedModel.ImportFromMdu(filePath);

            Assert.IsEmpty(importedModel.Area.ObservationPoints);
            Assert.IsEmpty(importedModel.Area.DryPoints);
        }

        [Test]
        public void SaveAndLoadMorphologySedimentParametersSimpleModel()
        {
            app.Run();
            app.CreateNewProject();

            const string path = "mdu.dsproj";
            app.SaveProjectAs(path); // save to initialize file repository..

            var model = new WaterFlowFMModel();
            //Enable Morphology and Sediment
            model.ModelDefinition.UseMorphologySediment = true;
            app.Project.RootFolder.Add(model);

            model.SedimentOverallProperties.Add(
                new SedimentProperty<double>("MyOverall Property", 0, 0, false, 1, false, string.Empty,
                                             string.Empty, false)
                { Value = 0.5 });
            var fraction = new SedimentFraction();
            ISedimentType sedType = fraction.AvailableSedimentTypes.ElementAtOrDefault(1);
            Assert.IsNotNull(sedType);
            /*var sedTypeFirstDoubleProperty = sedType.Properties.OfType<SedimentProperty<double>>().FirstOrDefault();
            Assert.IsNotNull(sedTypeFirstDoubleProperty);*/
            var sedTypeFirstDoubleProperty =
                new SedimentProperty<double>("MySediment Property", 2, 1.5, false, 2.5, false, string.Empty,
                                             string.Empty, false);
            sedTypeFirstDoubleProperty.Value =
                (new Random().NextDouble() *
                 (sedTypeFirstDoubleProperty.MaxValue - sedTypeFirstDoubleProperty.MinValue)) +
                sedTypeFirstDoubleProperty.MinValue;

            app.SaveProjectAs(path);

            //Check sed and mor files exist
            string morFile = model.MorFilePath;
            string sedFile = model.SedFilePath;

            Assert.IsTrue(File.Exists(morFile));
            Assert.IsTrue(File.Exists(sedFile));
            app.CloseProject();

            app.OpenProject(path);

            WaterFlowFMModel loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

            Assert.IsNotNull(loadedModel);
        }

        [Test]
        public void GivenASavedModelWithOutput_WhenRenamingTheModelAndSaving_ThenOutputFilesAreRelinked()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                try
                {
                    // Arrange
                    app.Run();
                    app.CreateNewProject();

                    using (WaterFlowFMModel model = CreatedModelWithOutputInProject())
                    {
                        app.SaveProjectAs(Path.Combine(tempDirectory.Path, "project.dsproj"));

                        // Precondition
                        Assert.That(File.Exists(model.OutputMapFileStore.Path),
                                    "Precondition violated: before renaming, the model should refer to an existing map file.");

                        // Act: rename model and save
                        model.Name = "new_name";
                        app.SaveProject();

                        // Assert
                        Assert.That(File.Exists(model.OutputMapFileStore.Path),
                                    "Output file path does not exist: output should be correctly relinked.");
                    }
                }
                finally
                {
                    app.CloseProject();
                }
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
            app.Run();
            app.CreateNewProject();

            string testModelPath = Path.Combine(@"MorphologySediment_Models\", mduTestPath);
            string mduPath = TestHelper.GetTestFilePath(testModelPath);
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            //Enable Morphology and Sediment
            model.ModelDefinition.UseMorphologySediment = true;
            app.Project.RootFolder.Add(model);

            app.SaveProjectAs(mduPath);
            app.CloseProject();

            app.OpenProject(mduPath);

            WaterFlowFMModel loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

            Assert.IsNotNull(loadedModel);
        }

        private static string GetBendProfPath()
        {
            return TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
        }

        private WaterFlowFMModel CreatedModelWithOutputInProject()
        {
            var model = new WaterFlowFMModel();

            app.Project.RootFolder.Add(model);

            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 2, 2);
            model.ReloadGrid(true, true);

            Assert.AreEqual(0, model.Validate().AllErrors.Count(),
                            "Precondition violated: there are errors in the model.");
            app.RunActivity(model);
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status,
                            "Precondition violated: model run failed.");

            return model;
        }
    }
}
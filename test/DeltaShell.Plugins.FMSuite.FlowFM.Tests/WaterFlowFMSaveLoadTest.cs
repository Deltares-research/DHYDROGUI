using System;
using System.IO;
using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

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

        [Test]
        public void SaveLoadModelEmptyModel()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

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
        }

        [Test]
        public void SaveLoadModelVerifyStartTimeIsSaved()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                app.Run();

                const string path = "mdu_time.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                app.Project.RootFolder.Add(model);

                var newStartTime = new DateTime(2000, 1, 2, 11, 15, 5, 2); //time with milliseconds!
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
        }

        [Test]
        public void SaveLoadModelVerifyHeatFluxModelTypeIsSaved()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                app.Run();

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
        }

        [Test]
        public void ExportImportModelVerifyHeatFluxModelTypeIsExported()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                app.Run();

                const string path = "mdutemp.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

                model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("3");
                Assert.AreEqual(true, model.UseTemperature);

                model.ExportTo("tempexport1\\mdutemp1.mdu", false);

                var retrievedModel = new WaterFlowFMModel("tempexport1\\mdutemp1.mdu");
                app.Project.RootFolder.Add(retrievedModel);

                Assert.AreEqual(HeatFluxModelType.ExcessTemperature, retrievedModel.HeatFluxModelType);
                Assert.AreEqual(true, retrievedModel.UseTemperature);

                retrievedModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("0");
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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();
                
                const string path = "mdu.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);
                app.CloseProject();
                app.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel) app.Project.RootFolder.Items[0];

                Assert.IsTrue(retrievedModel.NetFilePath.EndsWith("bend1_net.nc"));
                Assert.AreEqual(2, retrievedModel.BoundaryConditions.Count());
            }
        }
        
        [Test]
        public void RenameModelShouldRenameFilesWhereApplicable()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                var path = "mdu.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                // add existing model to project
                var mduPath = TestHelper.GetTestFilePath(@"rename\goot.mdu");
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                app.Project.RootFolder.Add(model);

                // rename model
                model.Name = "newgoot";

                // save model
                path = "test\\new.dsproj";
                app.SaveProjectAs(path);

                // check files
                var dataDir = path + "_data\\newgoot";
                Assert.AreEqual("newgoot", model.ModelDefinition.ModelName);
                Assert.AreEqual("newgoot.ext", model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).Value);
                Assert.AreEqual("newgoot_net.nc", model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value);
                Assert.AreEqual("newgoot_obs.xyn", model.ModelDefinition.GetModelProperty(KnownProperties.ObsFile).Value);

                Assert.IsFalse(File.Exists(Path.Combine(dataDir, "goot.mdu")), "mdu");
                Assert.IsTrue(File.Exists(Path.Combine(dataDir, "newgoot.mdu")), "mdu");
                Assert.IsTrue(File.Exists(Path.Combine(dataDir, "newgoot.ext")), "ext");
                Assert.IsTrue(File.Exists(Path.Combine(dataDir, "newgoot_obs.xyn")), "obs");
                Assert.IsTrue(File.Exists(Path.Combine(dataDir, "newgoot_net.nc")), "net3");
            }
        }

        [Test]
        public void RenameModelTwiceShouldRenameFilesWhereApplicable()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                var path = "mdu.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                // add existing model to project
                var mduPath = TestHelper.GetTestFilePath(@"rename\goot.mdu");
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                app.Project.RootFolder.Add(model);

                // rename model
                model.Name = "newgoot";

                // save model
                path = "test\\new.dsproj";
                app.SaveProjectAs(path);

                model.Name = "newgoot2";
                app.SaveProject();

                // check files
                Assert.AreEqual("newgoot2", model.ModelDefinition.ModelName);
                Assert.AreEqual("newgoot2.ext", model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).Value);
                Assert.AreEqual("newgoot2_net.nc", model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value);
                Assert.AreEqual("newgoot2_obs.xyn", model.ModelDefinition.GetModelProperty(KnownProperties.ObsFile).Value);

                var dataDir = path + "_data\\newgoot2";
                //Assert.IsFalse(File.Exists(Path.Combine(dataDir, "newgoot.mdu")), "mdu");
                Assert.IsTrue(File.Exists(Path.Combine(dataDir, "newgoot2.mdu")), "mdu");
                Assert.IsTrue(File.Exists(Path.Combine(dataDir, "newgoot2.ext")), "ext");
                Assert.IsTrue(File.Exists(Path.Combine(dataDir, "newgoot2_obs.xyn")), "obs");
                Assert.IsTrue(File.Exists(Path.Combine(dataDir, "newgoot2_net.nc")), "net3");
            }
        }

        [Test]
        public void SaveLoadModelVerifyMduIsReloadedAndModelDoesNotLeakEventSubscriptions()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
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
        }
        
        [Test]
        public void ImportIntoProjectVerifyGridFileIsDirectlyCopiedToDeltaShellDataDir()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu.dsproj";
                app.SaveProjectAs(path);

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);

                app.Project.RootFolder.Add(model);

                var netFile = Path.GetFullPath(Path.Combine(Path.Combine(path + "_data", "bendprof"), "bend1_net.nc"));
                Assert.IsTrue(File.Exists(netFile), "grid file should be in the data directory after import (for rgfgrid)");
            }
        }

        [Test]
        public void ImportIntoProjectVerifyFilesNotYetSupportedInUiButReferencedInMduAreCopiedAlong()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "copyalong.dsproj";
                app.SaveProjectAs(path);

                var mduPath = TestHelper.GetTestFilePath(@"copyalong\manholes_1d2d.mdu");
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);

                // upon adding to project, non-memory based stuff should be copied to the project temp directory
                app.Project.RootFolder.Add(model);
                var tempSaveDir = Path.GetFullPath(Path.Combine(path + "_data", "manholes_1d2d"));

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
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu.dsproj";
                app.SaveProjectAs(path);

                const string path2 = "mdu_save_as.dsproj";

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                    
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);

                app.SaveProjectAs(path2);

                app.CloseProject();

                app.OpenProject(path2);
                
                var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
                Assert.AreEqual(
                    Path.GetFullPath(Path.Combine(Path.Combine(path2 + "_data", "bendprof"), "bend1_net.nc")),
                    retrievedModel.NetFilePath);
            }
        }

        [Test]
        public void SaveAsLoadModelVerifyGridIsCopiedAlong()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu_grid.dsproj";
                app.SaveProjectAs(path);

                const string path2 = "mdu_save_as_grid.dsproj";

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);

                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);

                app.SaveProjectAs(path2);

                Assert.IsTrue(File.Exists("mdu_save_as_grid.dsproj_data\\bendprof\\bend1_net.nc"), "grid file does not exist");
                var retrievedModel = ((WaterFlowFMModel) app.Project.RootFolder.Items[0]);
                Assert.AreEqual(451, retrievedModel.Grid.Vertices.Count);
            }
        }

        [Test]
        public void CreateModelFromScratchModifySaveAsAndReload()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu_obs.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

                // add obs point
                model.Area.ObservationPoints.Add(new Feature2DPoint { Name = "obs1", Geometry = new Point(15, 15) });

                // save & reload
                app.SaveProjectAs(path);
                app.CloseProject();
                app.OpenProject(path);

                // check obs point still exists
                var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
                Assert.AreEqual(1, retrievedModel.Area.ObservationPoints.Count, "#obs points");
            }
        }

        [Test]
        public void CreateModelFromScratchSaveModifySaveAndReload()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu_resave.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path); // save

                // add obs point
                model.Area.ObservationPoints.Add(new Feature2DPoint { Name = "obs1", Geometry = new Point(15, 15) });

                // save & reload
                app.SaveProject(); //this only works if nhibernate is aware that something changed and actually does something
                app.CloseProject();
                app.OpenProject(path);

                // check obs point still exists
                var retrievedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
                Assert.AreEqual(1, retrievedModel.Area.ObservationPoints.Count, "#obs points");
            }
        }

        [Test]
        public void SaveLoadBathymetryDefinitions()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu_resave.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

                var polygon = new Polygon(new LinearRing(new[]
                    {
                        new Coordinate(-135, -105), new Coordinate(-85, -100), new Coordinate(-75, -205),
                        new Coordinate(-125, -200), new Coordinate(-135, -105)
                    }));
                
                app.SaveProjectAs(path);

                app.CloseProject();

                app.OpenProject(path);
            }
        }

        [Test]
        public void CreateFromScratchAddBoundarySaveAndReload()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu_drt.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaterFlowFMModel { Name = "mdu_drt" };
                app.Project.RootFolder.Add(model);

                var line = new LineString(new [] { new Coordinate(15, 15), new Coordinate(20, 20) });

                var boundary = new Feature2D { Name = "bound1", Geometry = line };
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
        }

        [Test]
        public void ImportHarlingenRunSaveAsLoadCheckOutput()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                // import
                const string path = "har.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..
                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                // run
                model.Initialize();
                model.Execute();
                model.Finish();
                model.Cleanup();

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
        }

        [Test]
        public void ImportHarlingenSaveRunCloseLoadCheckOutput()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                // import
                const string path = "har.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..
                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                // save
                app.SaveProjectAs(path);

                // run
                model.Initialize();
                model.Execute();
                model.Finish();
                model.Cleanup();

                // close
                app.SaveProject();
                app.CloseProject();

                //reopen
                app.OpenProject(path);

                var retrievedModel = (WaterFlowFMModel) app.Project.RootFolder.Items[0];
                Assert.IsNotNull(retrievedModel.OutputMapFileStore);
            }
        }

        [Test]
        public void DeleteDataPointSaveLoadShouldNotKeepDataPoint()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                app.Run();
                
                const string path = "mdu.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaterFlowFMModel();

                app.Project.RootFolder.Add(model);

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
                app.SaveProject();
                app.CloseProject();

                app.OpenProject(path);
                var loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(loadedModel);
                Assert.AreEqual(1, loadedModel.BoundaryConditionSets.Count);
                Assert.AreEqual(1, loadedModel.BoundaryConditions.Count());
                Assert.AreEqual(new[] {0, 1}, loadedModel.BoundaryConditions.First().DataPointIndices);

                loadedModel.BoundaryConditions.First().RemovePoint(0);
                app.SaveProject();
                app.CloseProject();

                app.OpenProject(path);
                var secondLoadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(secondLoadedModel);
                Assert.AreEqual(1, secondLoadedModel.BoundaryConditionSets.Count);
                Assert.AreEqual(1, secondLoadedModel.BoundaryConditions.Count());
                Assert.AreEqual(new[] {1}, secondLoadedModel.BoundaryConditions.First().DataPointIndices);
            }
        }

        [Test]
        public void SaveModelBuiltFromScratchWithWind()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                app.Run();

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
                Assert.AreEqual("wind.spw", Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().Path));
                Assert.IsTrue(File.Exists(@"windtest.dsproj_data\FlowFM\wind.spw"));
            }
        }

        [Test]
        public void SaveLoadSaveAsSaveShouldCopyWindFiles()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                app.Run();

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
                Assert.AreEqual("wind.spw", Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().WindFilePath));
                Assert.IsTrue(File.Exists(@"windtest.dsproj_data\FlowFM\wind.spw"));

                app.OpenProject("windtest2.dsproj");
                loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;

                Assert.IsNotNull(loadedModel);
                Assert.AreEqual("wind2.spw", Path.GetFileName(loadedModel.WindFields.OfType<SpiderWebWindField>().First().WindFilePath));
                Assert.IsTrue(File.Exists(@"windtest2.dsproj_data\FlowFM\wind2.spw"));
            }
        }

        [Test]
        public void TestRunWithGate()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                const string path = "mdu.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..
                
                var mduPath = TestHelper.GetTestFilePath(@"structures_gate\structsFM.dsproj_data\har\har.mdu");

                var model = new WaterFlowFMModel(mduPath);
                model.StopTime = model.StartTime.AddMinutes(15);

                Assert.IsTrue(model.Area.Gates.Any());

                app.Project.RootFolder.Add(model);
                
                ActivityRunner.RunActivity(model);

                Assert.AreNotEqual(ActivityStatus.Failed, model.Status);

                // close
                app.CloseProject();
            }
        }

        [Test]
        public void SaveLoadVerifyAreaFeatures()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                app.Run();

                const string path = "mdu.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaterFlowFMModel();

                // Embankments are mapped in dbase (and will also be written to file..)
                model.Area.Embankments.Add(new Embankment { Name = "embankment", Region = model.Area, Geometry = new LineString(new[] { new Coordinate(10, 10), new Coordinate(-10,10) }) });

                // Thin Dams are written to file and file only
                model.Area.ThinDams.Add(new ThinDam2D {Name = "thin", Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(1,1)})});

                app.Project.RootFolder.Add(model);

                app.SaveProjectAs("saveLoadAreaFeaturesTest.dsproj");
                app.CloseProject();

                app.OpenProject("saveLoadAreaFeaturesTest.dsproj");

                var loadedModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterFlowFMModel;
                Assert.AreEqual(1, loadedModel.Area.Embankments.Count);
                Assert.AreEqual(1, loadedModel.Area.ThinDams.Count);
            }
        }

        [Test]
        public void SaveLoadFixedWeirTest()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                const string path = "mdu.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

                var fixedWeir = new FixedWeir
                {
                    Name = "fixed weir",
                    Geometry =
                        new LineString(new [] { new Coordinate(0.0, 0.0), new Coordinate(0.3, 0.3), new Coordinate(0.6, 1.3) })
                };
                fixedWeir.CrestLevels[1] = 0.9876;
                var hydroArea = new HydroArea();
                hydroArea.FixedWeirs.Add(fixedWeir);
                model.Area = hydroArea;

                app.SaveProjectAs(path);

                app.CloseProject();

                app.OpenProject(path);

                var loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

                Assert.IsNotNull(loadedModel);
                Assert.IsNotNull(loadedModel.Area.FixedWeirs.First());
                Assert.AreEqual(0.9876, loadedModel.Area.FixedWeirs[0].CrestLevels[1]);
            }
        }

        [Test]
        public void SaveLoadDeleteGridTest()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                const string path = "mdu_grid.dsproj";
                app.SaveProjectAs(path); // save to initialize file repository..

                var mduPath = GetBendProfPath();
                mduPath = TestHelper.CreateLocalCopy(mduPath);
                var model = new WaterFlowFMModel(mduPath);
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(path);

                app.CloseProject();

                app.OpenProject(path);

                var loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

                Assert.NotNull(loadedModel);

                loadedModel.RemoveGrid();

                Assert.NotNull(loadedModel.NetFilePath);
                Assert.AreEqual(0, loadedModel.Grid.Cells.Count);
            }
        }
    }
}
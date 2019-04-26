using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;
using System;
using System.IO;
using System.Linq;
using Point = NetTopologySuite.Geometries.Point;
using PointwiseOperationType = SharpMap.SpatialOperations.PointwiseOperationType;

namespace DeltaShell.Plugins.NGHS.IntegrationTests
{
    /// <summary>
    /// So we are testing the plugin configurations here.
    /// 1. We create a model in a certain plugin configuration
    /// 2. We save this and close the project and the DSApp
    /// 3. We open a new DSApp with another more elaborate plugin configuration (NEVER WITH LESS PLUGINS!!) 
    /// 4. We verify and validate if the model is still in the project
    /// 5. We save the model in the new plugin configuration and close the project
    /// 6. We open the project again in the same plugin configuration
    /// 7. We verify and validate if the model is still in the project 
    /// </summary>
    [TestFixture]
    public class PluginPortabilityTest
    {
        private static readonly DateTime fmModelStartTime = new DateTime(2000, 1, 1);
        private static readonly Coordinate[] coordinates = { new Coordinate(60, 60), new Coordinate(60, 80), new Coordinate(80, 60), new Coordinate(60, 60) };
        private static readonly Polygon myPolygon = new Polygon(
                new LinearRing(new [] { new Coordinate(50, 10), new Coordinate(30, 20), new Coordinate(70, 20), new Coordinate(50, 10) }));

        // BART TEST ID : 7
        /// /// <summary>
        /// Test if an FM model can be saved in an environment with FM and RTC plugins.
        /// Then read it in an environment that contains FM, RTC, WAQ and Wave.
        /// Simple model, FM minimal -> FM maximal
        /// TOOLS-22951
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void ReadSimpleFlowFmModelWithRtcWithoutAndWithWaqandWavePluginsConfiguration()
        {
            string dsprojName = "FMRTC_FMRTCWAQWAVE.dsproj";
            using (var app = new DeltaShellApplication())
            {
                //apps : common
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                //apps : FM+RTC
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());

                app.Run();
                app.SaveProjectAs(dsprojName); // save to initialize file repository..

                var fmModel = MyWaterFlowFmModel();

                app.Project.RootFolder.Add(fmModel);

                fmModel.ReloadGrid();

                app.SaveProject();
                app.CloseProject();
            }


            using (var app = new DeltaShellApplication())
            {
                //apps : common
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                //apps : FM+RTC
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());

                //apps : WAQ+Wave
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());

                app.Run();

                app.OpenProject(dsprojName);

                var savedFmModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.NotNull(savedFmModel);
                ValidateModel(savedFmModel);

                app.SaveProjectAs("new" + dsprojName);
                app.CloseProject();
                app.OpenProject("new" + dsprojName);

                savedFmModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.NotNull(savedFmModel);
                ValidateModel(savedFmModel);

                app.CloseProject();

            }
        }

        // BART TEST ID : 9
        /// /// <summary>
        /// Test if an integrated hydro model with a FM model and a RTC model can be saved in an environment with FM and RTC plugins.
        /// Then read it in an environment that contains FM, RTC, WAQ and Wave.
        /// Complex / Integrated model, FM minimal -> FM maximal
        /// TOOLS-22951
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void ReadIntegratedlowFmModelWithRtcWithoutAndWithWaqandWavePluginsConfiguration()
        {
            string dsprojName = "IM_FMRTC_FMRTCWAQWAVE.dsproj";
            using (var app = new DeltaShellApplication())
            {
                //apps : common
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                
                //apps : helper
                app.Plugins.Add(new ScriptingApplicationPlugin());
                
                //apps : FM+RTC
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();
                app.SaveProjectAs(dsprojName); // save to initialize file repository..

                //create a hydromodel with fm, rtc it (FM Integrated model)
                var hydroModel = MyFmRtcHydroModel();

                app.Project.RootFolder.Add(hydroModel);
                var fmModel = hydroModel.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                if (fmModel != null)
                {
                    NetFile.Write(fmModel.NetFilePath,fmModel.Grid);
                }
                app.SaveProject();
                app.CloseProject();
            }


            using (var app = new DeltaShellApplication())
            {
                //apps : common
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                //apps : FM+RTC
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                //apps : WAQ+Wave
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());

                app.Run();

                app.OpenProject(dsprojName);

                var savedHydroModel = app.Project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
                Assert.NotNull(savedHydroModel);
                ValidateModel(savedHydroModel,ModelGroup.FMWaveRtcModels);

                var fmModel = savedHydroModel.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.NotNull(fmModel);
                ValidateModel(fmModel);

                var rtcModel = savedHydroModel.Models.OfType<RealTimeControlModel>().FirstOrDefault();
                Assert.NotNull(rtcModel);
                ValidateModel(rtcModel);

                app.SaveProjectAs("new" + dsprojName);
                app.CloseProject();
                app.OpenProject("new" + dsprojName);

                savedHydroModel = app.Project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
                Assert.NotNull(savedHydroModel);
                ValidateModel(savedHydroModel,ModelGroup.FMWaveRtcModels);

                fmModel = savedHydroModel.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.NotNull(fmModel);
                ValidateModel(fmModel);

                rtcModel = savedHydroModel.Models.OfType<RealTimeControlModel>().FirstOrDefault();
                Assert.NotNull(rtcModel);
                ValidateModel(rtcModel);

                app.CloseProject();

            }
        }

        // BART TEST ID : 14

        /// <summary>
        /// Test if a WAQ model can be saved in an environment with WAQ plugins.
        /// Then read it in an environment that contains FM, RTC, WAQ and Wave plugins.
        /// Transition WAQ->FM
        /// TOOLS-22951
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void ReadSimpleWaqModelWithoutAndWithFmAndRtcAndWaqAndWavePluginsConfiguration()
        {
            string dsprojName = "WAQ_FMRTCWAQWAVE.dsproj";
            using (var app = new DeltaShellApplication())
            {
                //apps : common
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                //apps : WAQ
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                app.Run();
                app.SaveProjectAs(dsprojName); // save to initialize file repository..

                //create only a WAQ model
                var waterQualityModel = MyWaqModel();
                app.Project.RootFolder.Add(waterQualityModel);
                waterQualityModel.BoundaryDataManager.CreateNewDataTable("A", "B", "C.d", "E");
                waterQualityModel.LoadsDataManager.CreateNewDataTable("F", "G", "H.i", "J");

                app.SaveProject();
                app.CloseProject();
            }


            using (var app = new DeltaShellApplication())
            {
                //apps : common
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                //apps : WAQ
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                //apps : FM+RTC+Wave
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());

                app.Run();

                app.OpenProject(dsprojName);

                var savedWaqModel = app.Project.RootFolder.Models.OfType<WaterQualityModel>().FirstOrDefault();
                Assert.NotNull(savedWaqModel);
                ValidateModel(savedWaqModel);

                app.SaveProjectAs("new" + dsprojName);
                app.CloseProject();

                app.OpenProject("new" + dsprojName);

                savedWaqModel = app.Project.RootFolder.Models.OfType<WaterQualityModel>().FirstOrDefault();
                Assert.NotNull(savedWaqModel);
                ValidateModel(savedWaqModel);

                app.CloseProject();

            }
        }

        private static HydroModel MyFmRtcHydroModel()
        {
            //create a hydromodel with fm and rtc (FM minimal)
            var fmModel = MyWaterFlowFmModel();
            var rtcModel = MyRealTimeControlModel(fmModel);

            var hydroModel = new HydroModel
            {
                Activities = { rtcModel },
                OverrideStartTime = false,
                OverrideStopTime = false,
                OverrideTimeStep = false
            };

            fmModel.MoveModelIntoIntegratedModel(null, hydroModel);
            var workflow = new ParallelActivity
            {
                Activities =
                {
                    new ActivityWrapper {Activity = fmModel},
                    new ActivityWrapper {Activity = rtcModel}
                }

            };
            hydroModel.Workflows.Add(workflow);
            return hydroModel;
        }

        private void ValidateModel(WaterQualityModel waqModel)
        {
            Assert.Greater((int) waqModel.SubstanceProcessLibrary.Substances.Count, 0);
            var oxy = waqModel.SubstanceProcessLibrary.Substances.FirstOrDefault(s => s.Name == "OXY");
            Assert.NotNull(oxy);
            Assert.AreEqual((double) 1.23d, (double) oxy.InitialValue, 0.01d);

            Assert.Greater((int) waqModel.ProcessCoefficients.Count, 0);

            var procCoof = waqModel.ProcessCoefficients.FirstOrDefault(p => p.Name == "fTEWOROXY");
            Assert.NotNull(procCoof);
            Assert.AreEqual((int) 1, (int) procCoof.Components.Count);
            Assert.AreEqual("gO2/m3/d", procCoof.Components[0].Unit.Name);

            Assert.AreEqual((int) 2, (int) waqModel.Loads.Count());
            Assert.AreEqual((double) 1.1d, (double) waqModel.Loads.First().Z, 0.1d);
            Assert.AreEqual((double) 2.2d, (double) waqModel.Loads.Last().Z, 0.1d);
            Assert.AreEqual((int) 2, (int) waqModel.ObservationPoints.Count());
            Assert.AreEqual((double) 3.3d, (double) waqModel.ObservationPoints.First().Z, 0.1d);
            Assert.AreEqual((double) 4.4d, (double) waqModel.ObservationPoints.Last().Z, 0.1d);

            var boundaryDataFolder = waqModel.BoundaryDataManager.FolderPath;
            var loadsDataFolder = waqModel.LoadsDataManager.FolderPath;

            Assert.IsTrue(Directory.Exists(boundaryDataFolder));
            Assert.AreEqual(2, Directory.GetFiles(boundaryDataFolder).Length);

            Assert.IsTrue(Directory.Exists(loadsDataFolder));
            Assert.AreEqual(2, Directory.GetFiles(loadsDataFolder).Length);
        }

        private void ValidateModel(WaterFlowFMModel fmModel)
        {
            Assert.AreEqual((int) 121, (int) fmModel.Grid.Vertices.Count());
            Assert.AreEqual((int) 220, (int) fmModel.Grid.Edges.Count());
            Assert.AreEqual((int) 2, (int) fmModel.Area.Pumps.Count());
            var pump1 = fmModel.Area.Pumps.FirstOrDefault();
            Assert.NotNull(pump1);
            Assert.AreEqual((int) 2, (int) pump1.Geometry.Coordinates.Count());
            Assert.AreEqual((object) new Coordinate(20, 20), pump1.Geometry.Coordinates.First());
            Assert.AreEqual((object) new Coordinate(30, 30), pump1.Geometry.Coordinates.Last());
            Assert.AreEqual((double) 100.0d, (double) pump1.Capacity, 0.1d);
            Assert.AreEqual(false, pump1.UseCapacityTimeSeries);

            var pump2 = fmModel.Area.Pumps.LastOrDefault();
            Assert.NotNull(pump2);
            Assert.AreEqual((int) 2, (int) pump2.Geometry.Coordinates.Count());
            Assert.AreEqual((object) new Coordinate(20, 30), pump2.Geometry.Coordinates.First());
            Assert.AreEqual((object) new Coordinate(30, 40), pump2.Geometry.Coordinates.Last());
            Assert.AreEqual(true, pump2.UseCapacityTimeSeries);
            Assert.AreEqual(7.8d, (double)pump2.CapacityTimeSeries[fmModelStartTime], 0.1d);

            Assert.AreEqual((int) 2, (int) fmModel.Area.ObservationPoints.Count());
            Assert.AreEqual((object) new Point(15, 15), fmModel.Area.ObservationPoints.First().Geometry);
            Assert.AreEqual((object) new Point(40, 40), fmModel.Area.ObservationPoints.Last().Geometry);

            var coverageSpatialOperationValueConverters = fmModel.AllDataItems.Select(di => di.ValueConverter).OfType<CoverageSpatialOperationValueConverter>().ToList();
            Assert.AreEqual((int) 1, (int) coverageSpatialOperationValueConverters.Count());
            var spatialCov = coverageSpatialOperationValueConverters.First();
            Assert.AreEqual(typeof(ICoverage), spatialCov.OperationDataType);
            Assert.AreEqual((object) fmModel.Bathymetry, spatialCov.ConvertedValue);

            var spatialOperation = spatialCov.SpatialOperationSet.Operations[0];
            Assert.AreEqual(typeof(SetValueOperation), spatialOperation.GetType());
            Assert.AreEqual((int) 2, (int) spatialOperation.Inputs.Count());
            Assert.AreEqual(typeof(FeatureCollection), spatialOperation.Inputs[1].Provider.GetType());
            Assert.AreEqual(typeof(Feature), spatialOperation.Inputs[1].Provider.FeatureType);
            Assert.AreEqual((int) 1, (int) spatialOperation.Inputs[1].Provider.Features.Count);
            Assert.AreEqual((object) myPolygon, ((Feature)spatialOperation.Inputs[1].Provider.Features[0]).Geometry);

            var setValueOperation = ((SetValueOperation) (spatialOperation));
            Assert.AreEqual((double) 100.0d, (double) setValueOperation.Value,0.1d);
            Assert.AreEqual((object) PointwiseOperationType.Overwrite, setValueOperation.OperationType);
                
            Assert.AreEqual((int) 1, (int) fmModel.Area.Weirs.Count());
            Assert.AreEqual((int) 1, (int) fmModel.Area.ThinDams.Count());
            Assert.AreEqual((int) 1, (int) fmModel.Area.FixedWeirs.Count());
            Assert.AreEqual((int) 1, (int) fmModel.Area.ObservationCrossSections.Count());
            Assert.AreEqual((int) 1, (int) fmModel.Area.DryAreas.Count());
            Assert.AreEqual((object) new Polygon(new LinearRing(coordinates)), fmModel.Area.DryAreas[0].Geometry);

            //Can't check drypoints and dryareas at the same time.... info is saved in same file, is a feature of comp core
//            Assert.AreEqual(1, fmModel.Area.DryPoints.Count());
            Assert.AreEqual((int) 1,(int) fmModel.SourcesAndSinks.Count);
        }

        private void ValidateModel(RealTimeControlModel rtcModel)
        {
            Assert.AreEqual((int) 1, (int) rtcModel.ControlGroups.Count());
            var controlGroup = rtcModel.ControlGroups[0];
            Assert.AreEqual((int) 2, (int) controlGroup.Inputs.Count());
            Assert.AreEqual((int) 1, (int) controlGroup.Outputs.Count());
            var hydraulicRule1A = (HydraulicRule)controlGroup.Rules[0];
            Assert.AreEqual(1.0, hydraulicRule1A.Function[0.0]);
        }

        private void ValidateModel(HydroModel hydroModel, ModelGroup modelGroup)
        {
            switch (modelGroup)
            {
                case ModelGroup.SobekModels:
                    ValidateMinimalHydroSobekModel(hydroModel);
                    break;

                case ModelGroup.FMWaveRtcModels:
                    ValidateMinimalHydroFmModel(hydroModel);
                    break;
            }
        }

        private static void ValidateMinimalHydroSobekModel(HydroModel hydroModel)
        {
            // check nr of model (3 => RR, RTC and F1D)
            Assert.AreEqual((int) 3, (int) hydroModel.Models.Count());

            // Check if we have a RR model
            Assert.NotNull(hydroModel.Models.OfType<IRainfallRunoffModel>().FirstOrDefault());

            // Check if we have a RTC model
            Assert.NotNull(hydroModel.Models.OfType<IRealTimeControlModel>().FirstOrDefault());

            // Check if we have a F1D model
            Assert.NotNull(hydroModel.Models.OfType<WaterFlowModel1D>().FirstOrDefault());

            // Check the workflow
            // - is is a sequential wf :
            Assert.AreEqual(typeof (SequentialActivity), hydroModel.CurrentWorkflow.GetEntityType());

            // - with 2 activities, RR and ParallelActivity (in this order) :
            Assert.AreEqual((int) 2, (int) hydroModel.CurrentWorkflow.Activities.Count);
            var activities = hydroModel.CurrentWorkflow.Activities;
            Assert.NotNull(
                activities.OfType<ActivityWrapper>().Select(a => a.Activity).OfType<RainfallRunoffModel>().FirstOrDefault());
            var activityWrapperForRr = activities[0] as ActivityWrapper;
            Assert.NotNull(activityWrapperForRr);
            Assert.AreEqual(typeof (RainfallRunoffModel), activityWrapperForRr.Activity.GetEntityType());
            Assert.AreEqual(typeof (ParallelActivity), activities[1].GetEntityType());

            // - and in the parallel activity 2 models, RTC and F1D (in this order):
            var parallelActivity = hydroModel.CurrentWorkflow.Activities.OfType<ParallelActivity>().FirstOrDefault();
            Assert.NotNull(parallelActivity);
            Assert.AreEqual((int) 2, (int) parallelActivity.Activities.Count);

            var parallelActivities = parallelActivity.Activities.OfType<ActivityWrapper>().Select(a => a.Activity).ToList();
            Assert.NotNull(parallelActivities.OfType<RealTimeControlModel>().FirstOrDefault());
            Assert.NotNull(parallelActivities.OfType<WaterFlowModel1D>().FirstOrDefault());
            Assert.AreEqual(typeof (RealTimeControlModel), parallelActivities[0].GetEntityType());
            Assert.AreEqual(typeof (WaterFlowModel1D), parallelActivities[1].GetEntityType());
        }

        private static void ValidateMinimalHydroFmModel(HydroModel hydroModel)
        {
            // check nr of model (2 => FM and RTC)
            Assert.AreEqual((int) 2, (int) hydroModel.Models.Count());

            // Check if we have a FM model
            Assert.NotNull(hydroModel.Models.OfType<WaterFlowFMModel>().FirstOrDefault());

            // Check if we have a RTC model
            Assert.NotNull(hydroModel.Models.OfType<IRealTimeControlModel>().FirstOrDefault());

            // Check the workflow
            // - is is a sequential wf :
            Assert.AreEqual(typeof(ParallelActivity), hydroModel.CurrentWorkflow.GetEntityType());

            // - with 2 activities, FM and RTC (in this order) :
            Assert.AreEqual((int) 2, (int) hydroModel.CurrentWorkflow.Activities.Count);
            var activities = hydroModel.CurrentWorkflow.Activities.OfType<ActivityWrapper>().Select(a => a.Activity).ToList();
            Assert.NotNull(activities.OfType<WaterFlowFMModel>().FirstOrDefault());
            Assert.NotNull(activities.OfType<RealTimeControlModel>().FirstOrDefault());
            Assert.AreEqual(typeof(RealTimeControlModel), activities[0].GetEntityType());
            Assert.AreEqual(typeof(WaterFlowFMModel), activities[1].GetEntityType());
        }

        private static WaterQualityModel MyWaqModel()
        {
            //copied from DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.WaterQualityModelWorkDirectoryTest.CreateWaqModelWithData()
            var dataDir = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(WaterQualityModelApplicationPluginTest).Assembly);
            var squareHydFile = Path.Combine(dataDir, "IO", "square", "square.hyd");

            var hydFile = squareHydFile;

            var data = HydFileReader.ReadAll(new FileInfo(hydFile));

            var model = new WaterQualityModel();
            model.ImportHydroData(data);

            var subFilePath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(WaterQualityModelApplicationPluginTest).Assembly, @"IO\03d_Tewor2003.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);
            var oxy = model.SubstanceProcessLibrary.Substances.FirstOrDefault(s => s.Name == "OXY");
            Assert.NotNull(oxy);
            oxy.InitialValue = 1.23d;

            var procCoof = model.ProcessCoefficients.FirstOrDefault(p => p.Name == "fTEWOROXY");
            Assert.NotNull(procCoof);
            Assert.AreEqual((int) 1, (int) procCoof.Components.Count);
            Assert.AreEqual("gO2/m3/d", procCoof.Components[0].Unit.Name);
            
            model.Loads.AddRange(new[]
            {
                new WaterQualityLoad
                {
                    Z = 1.1
                },
                new WaterQualityLoad
                {
                    Z = 2.2
                }
            });

            model.ObservationPoints.AddRange(new[]
            {
                new WaterQualityObservationPoint
                {
                    Z = 3.3
                },
                new WaterQualityObservationPoint
                {
                    Z = 4.4
                }
            });
            return model;
        }

        private static WaterFlowFMModel MyWaterFlowFmModel()
        {
            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(10, 10, 100, 100);
            var area = new HydroArea();
            area.Pumps.Add(new Pump2D("Pump1")
            {
                Geometry = new LineString(new [] {new Coordinate(20, 20), new Coordinate(30, 30)}),
                UseCapacityTimeSeries = false,
                Capacity = 100.0
               
            });
            var pump2 = new Pump2D("Pump2", true)
            {
                Geometry = new LineString(new [] { new Coordinate(20, 30), new Coordinate(30, 40) }),
                UseCapacityTimeSeries = true

            };
            

            pump2.CapacityTimeSeries[fmModelStartTime] = 7.8;
            area.Pumps.Add(pump2);
            

            area.ObservationPoints.Add(new GroupableFeature2DPoint(){Name = "ObservationPoint1" , Geometry = new Point(15, 15)});
            area.ObservationPoints.Add(new GroupableFeature2DPoint(){Name = "ObservationPoint2", Geometry = new Point(40, 40)});
            area.Weirs.Add(new Weir2D("weir1"){
                Geometry = new LineString(new [] { new Coordinate(25, 20), new Coordinate(30, 30) }),
                OffsetY = 150,
                CrestWidth = 10.0,
                CrestLevel = 6.0,
                FlowDirection = FlowDirection.Both,
                WeirFormula = new SimpleWeirFormula
                {
                    LateralContraction = 0.9,
                    DischargeCoefficient = 1.0
                }
            });
            var lineString = new LineString(new []{new Coordinate(0,0), new Coordinate(1,1)});            
            area.ThinDams.Add(new ThinDam2D(){Name = "thin", Geometry = lineString});
            area.FixedWeirs.Add(new FixedWeir(){Name ="fixedweir", Geometry = lineString});
            area.ObservationCrossSections.Add(new ObservationCrossSection2D(){Name="ObsCS", Geometry = lineString});
            area.DryAreas.Add(new GroupableFeature2DPolygon { Name="dryarea", Geometry = new Polygon(new LinearRing(coordinates)) });

            //Can't check drypoints and dryareas at the same time.... info is saved in same file, is a feature of comp core
            //area.DryPoints.Add(new PointFeature() { Geometry = new Point(5, 5) });
            
            var model = new WaterFlowFMModel
            {
                TimeStep = new TimeSpan(0, 0, 1, 0),
                StartTime = fmModelStartTime,
                StopTime = new DateTime(2000, 1, 2),
                OutputTimeStep = new TimeSpan(0, 0, 2, 0),
                Name = "MyWaterFlowFmModel",
                Grid =
                {
                    Vertices = grid.Vertices,
                    Edges = grid.Edges
                },
                Area = area
                

            };
            model.SourcesAndSinks.Add(new SourceAndSink(){Feature = new Feature2D(){Name="test", Geometry = lineString}});
            

            model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value = new DateTime(2000, 1, 1);

            ICoverage coverage = model.Bathymetry;
            SetValueOnCoverage(((IModel) model).GetDataItemByValue(coverage), myPolygon, 100.0d);
            
            return model;
            
        }

        private static void SetValueOnCoverage(IDataItem coverageDataItem, Polygon polygon, double value)
        {
            if (coverageDataItem.ValueConverter == null)
            {
                coverageDataItem.ValueConverter = SpatialOperationValueConverterFactory.Create(coverageDataItem.Value, coverageDataItem.ValueType); 
            }

            var spatialOperationSet = ((SpatialOperationSetValueConverter)coverageDataItem.ValueConverter).SpatialOperationSet;

            var operation = new SetValueOperation
            {
                Value = value,
                OperationType = PointwiseOperationType.Overwrite,
                Name = "SetValue"
            };
            operation.SetInputData(SpatialOperation.MaskInputName,new FeatureCollection(new []{new Feature(){Geometry = polygon}},typeof(Feature)));
            spatialOperationSet.AddOperation(operation);
            spatialOperationSet.Execute();
        }

        private static RealTimeControlModel MyRealTimeControlModel(WaterFlowFMModel fmModel)
        {
            var rtcModel = new RealTimeControlModel();

#pragma warning disable 618
            var controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
#pragma warning restore 618
            controlGroup1.Name = "controlGroup1";

            controlGroup1.Inputs[0].Feature = fmModel.Area.ObservationPoints.First();
            controlGroup1.Inputs[0].ParameterName = "test";
            controlGroup1.Inputs[1].Feature = fmModel.Area.ObservationPoints.Last();
            controlGroup1.Inputs[1].ParameterName = "test";
            controlGroup1.Outputs[0].Feature = fmModel.Area.Pumps.First();
            controlGroup1.Outputs[0].ParameterName = "test";
            var hydraulicRule1A = (HydraulicRule)controlGroup1.Rules[0];
            hydraulicRule1A.Function[0.0] = 1.0;
            rtcModel.ControlGroups.Add(controlGroup1);
            return rtcModel;
        }

        private static void AddStructureToF1DModel(WaterFlowModel1D myWaterFlowModel1D, IBranch branch, BranchStructure structure, int chainage)
        {
            var compositeStructure = new CompositeBranchStructure
            {
                Network = myWaterFlowModel1D.Network,
                Geometry = new Point(chainage, 0),
                Chainage = chainage
            };
            NetworkHelper.AddBranchFeatureToBranch(compositeStructure, branch, compositeStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeStructure, structure);
        }
    }
}
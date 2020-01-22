using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;
using FixedWeir = DelftTools.Hydro.Structures.FixedWeir;
using ObservationCrossSection2D = DelftTools.Hydro.ObservationCrossSection2D;
using PointwiseOperationType = SharpMap.SpatialOperations.PointwiseOperationType;
using Point = NetTopologySuite.Geometries.Point;
using ThinDam2D = DelftTools.Hydro.Structures.ThinDam2D;

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
        const double discharge = 5.0;
        const string lsource1 = "lSource1";
        private static readonly DateTime fmModelStartTime = new DateTime(2000, 1, 1);
        private static readonly Coordinate[] coordinates = { new Coordinate(60, 60), new Coordinate(60, 80), new Coordinate(80, 60), new Coordinate(60, 60) };
        private static readonly Polygon myPolygon = new Polygon(
                new LinearRing(new [] { new Coordinate(50, 10), new Coordinate(30, 20), new Coordinate(70, 20), new Coordinate(50, 10) }));


        // BART TEST ID : 5
        /// /// <summary>
        /// Test if an FM model can be saved in an environment with FM plugin only.
        /// Then read it in an environment that contains FM, Flow1d, RTC and RR.
        /// Transition FM->SOBEK
        /// TOOLS-22951
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void ReadSimpleFlowFmModelWithoutAndWithF1DRtcAndRrPluginsConfiguration()
        {
            string dsprojName = "FM_FMF1dRTCRR.dsproj";
            
            using (var app = new DeltaShellApplication())
            {
                //apps : common
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                //apps : FM
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
                
                var fmModel = MyWaterFlowFmModel();
                app.Project.RootFolder.Add(fmModel);
                NetFile.Write(fmModel.NetFilePath,fmModel.Grid);
                app.SaveProject();
                
                
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

                //apps : FM
                app.Plugins.Add(new FlowFMApplicationPlugin());

                //apps : F1d+RTC+RR
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());

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
                
            Assert.AreEqual((int) 1, (int) fmModel.Area.Gates.Count());
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

        private void ValidateModel(RainfallRunoffModel rrModel)
        {
            Assert.AreEqual((int) 6, (int) rrModel.Basin.AllCatchments.Count());

            Assert.AreEqual((int) 5, (int) rrModel.Basin.Catchments.Count());

            Assert.AreEqual(CatchmentType.Paved, rrModel.Basin.Catchments.ElementAt(0).CatchmentType);
            var pavedData = (PavedData)rrModel.GetCatchmentModelData(rrModel.Basin.Catchments.ElementAt(0));
            Assert.AreEqual((object) 25000, pavedData.CalculationArea);
            Assert.AreEqual((object) PavedEnums.SewerPumpDischargeTarget.WWTP, pavedData.MixedAndOrRainfallSewerPumpDischarge);
            Assert.AreEqual((double) 1.0d, (double) pavedData.CapacityMixedAndOrRainfall, 0.1d);
            Assert.AreEqual((object) PavedEnums.SpillingDefinition.UseRunoffCoefficient, pavedData.SpillingDefinition);
            Assert.AreEqual((double) 0.001d, (double) pavedData.RunoffCoefficient, 0.001d); 
            
            Assert.AreEqual(CatchmentType.Unpaved, rrModel.Basin.Catchments.ElementAt(1).CatchmentType);
            Assert.AreEqual(CatchmentType.GreenHouse, rrModel.Basin.Catchments.ElementAt(2).CatchmentType);
            Assert.AreEqual(CatchmentType.OpenWater, rrModel.Basin.Catchments.ElementAt(3).CatchmentType);
            Assert.AreEqual(CatchmentType.Polder, rrModel.Basin.Catchments.ElementAt(4).CatchmentType);
            Assert.AreEqual((int) 1, (int) rrModel.Basin.Catchments.ElementAt(4).SubCatchments.Count()); 
            Assert.AreEqual(CatchmentType.Paved, rrModel.Basin.Catchments.ElementAt(4).SubCatchments.ElementAt(0).CatchmentType);

            Assert.AreEqual((int) 1, (int) rrModel.Basin.Boundaries.Count());
            var retrievedBoundary = rrModel.Basin.Boundaries.First();
            var retrievedBoundaryData = rrModel.BoundaryData.First();
            Assert.AreSame(retrievedBoundary, retrievedBoundaryData.Boundary);
            Assert.AreEqual(15.0, retrievedBoundaryData.Series.Data.Components[0].Values[0]);
            Assert.AreEqual(11.0, retrievedBoundaryData.Series.Value);

            Assert.AreEqual((int) 1, (int) rrModel.Basin.WasteWaterTreatmentPlants.Count());
            Assert.AreEqual((object) new Point(55, 55), rrModel.Basin.WasteWaterTreatmentPlants.First().Geometry);

            Assert.AreEqual((int) 25, (int) rrModel.Precipitation.Data.GetValues().Count);
            Assert.AreEqual((int) 2, (int) rrModel.Evaporation.Data.GetValues().Count);
            
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
            area.Gates.Add(new Gate2D(){Name = "gate", Geometry = lineString});
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

        private static RainfallRunoffModel MyRainfallRunoffModel()
        {
            var rrModel = new RainfallRunoffModel();
            var c1 = Catchment.CreateDefault();
            c1.CatchmentType = CatchmentType.Paved;
            
            rrModel.Basin.Catchments.Add(c1);
            var wwtp = new WasteWaterTreatmentPlant { Geometry = new Point(55, 55) }; 
            rrModel.Basin.WasteWaterTreatmentPlants.Add(wwtp);
            c1.LinkTo(wwtp);

            var pavedData = (PavedData)rrModel.GetCatchmentModelData(c1);
            pavedData.CalculationArea = 25000;
            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
            pavedData.CapacityMixedAndOrRainfall = 1.0;

            pavedData.SpillingDefinition = PavedEnums.SpillingDefinition.UseRunoffCoefficient;
            pavedData.RunoffCoefficient = 0.001; //drag it out: gives us a nice spread of boundary outflow
            
            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(rrModel.Precipitation.Data, rrModel.StartTime, rrModel.StopTime,
                                         new TimeSpan(0, 1, 0, 0));
            generator.GenerateTimeSeries(rrModel.Evaporation.Data, rrModel.StartTime, rrModel.StopTime,
                                         new TimeSpan(1, 0, 0, 0));
            
            var c2 = Catchment.CreateDefault();
            var c3 = Catchment.CreateDefault();
            var c4 = Catchment.CreateDefault();
            var c5 = Catchment.CreateDefault();
            var c6 = Catchment.CreateDefault();

            
            c2.CatchmentType = CatchmentType.Unpaved;
            c3.CatchmentType = CatchmentType.GreenHouse;
            c4.CatchmentType = CatchmentType.OpenWater;
            c5.CatchmentType = CatchmentType.Polder;
            c6.CatchmentType = CatchmentType.Paved;

            c5.SubCatchments.Add(c6);
            
            rrModel.Basin.Catchments.AddRange(new[] { c2, c3, c4, c5 });
            
            var runoffBoundary = new RunoffBoundary();
            rrModel.Basin.Boundaries.Add(runoffBoundary);
            var boundaryData = rrModel.BoundaryData.First(bd => bd.Boundary == runoffBoundary);
            boundaryData.Series.Data[new DateTime(2005, 1, 1)] = 15.0;
            boundaryData.Series.Value = 11.0;
            return rrModel;
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
    }
}
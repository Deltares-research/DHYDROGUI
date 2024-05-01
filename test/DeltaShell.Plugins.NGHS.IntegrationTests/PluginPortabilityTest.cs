using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;

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
            
            var pluginsToAdd = new List<IPlugin>()
            {
                //apps : common
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),

                //apps : FM
                new FlowFMApplicationPlugin(),
            };

            using (var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build())
            {
                app.Run();

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
                
                var fmModel = MyWaterFlowFmModel();
                app.Project.RootFolder.Add(fmModel);
                NetFile.Write(fmModel.NetFilePath,fmModel.Grid);
                app.SaveProject();
                
                
                app.SaveProject();
                app.CloseProject();
            }


            var pluginsToAdd2 = new List<IPlugin>()
            {
                //apps : common
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),

                //apps : FM
                new FlowFMApplicationPlugin(),

                //apps : F1d+RTC+RR
                new RealTimeControlApplicationPlugin(),
                new RainfallRunoffApplicationPlugin(),
            };
            
            using (var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd2).Build())
            {
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
            
            var pluginsToAdd = new List<IPlugin>()
            {
                //apps : common
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),

                //apps : FM+RTC
                new FlowFMApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
            };
            
            using (var app =  new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build())
            {
                app.Run();
                app.SaveProjectAs(dsprojName); // save to initialize file repository..

                var fmModel = MyWaterFlowFmModel();

                app.Project.RootFolder.Add(fmModel);

                fmModel.ReloadGrid();

                app.SaveProject();
                app.CloseProject();
            }

            var pluginsToAdd2 = new List<IPlugin>()
            {
                //apps : common
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),

                //apps : FM+RTC
                new FlowFMApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
            };
            
            using (var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd2).Build())
            {
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
                    CorrectionCoefficient = 0.9
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
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ExtForceFileTest
    {
        private static void AddBoundaryCondition(WaterFlowFMModel model, FlowBoundaryCondition bc)
        {
            var modelDefinition = model.ModelDefinition;
            var set =
                modelDefinition.BoundaryConditionSets.FirstOrDefault(
                    bcs => bcs.Feature == ((IBoundaryCondition) bc).Feature);
            if (set != null)
            {
                set.BoundaryConditions.Add(bc);
            }
            else
            {
                modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet
                {
                    Feature = ((IBoundaryCondition) bc).Feature as Feature2D,
                    BoundaryConditions = new EventedList<IBoundaryCondition> {bc}
                });
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadPolygonForcings()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"harlingen\001.ext");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def);

            //extForceFile.ImportSpatialOperations(extPath, def);

            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.AreEqual(2, def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName).Count);

            var roughnessOperations = def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
            Assert.IsNotNull(roughnessOperations);
            Assert.AreEqual(1, roughnessOperations.Count);

            var firstOperation = roughnessOperations.First() as SetValueOperation;
            Assert.IsNotNull(firstOperation);

            Assert.AreEqual(PointwiseOperationType.Overwrite, firstOperation.OperationType);
            Assert.AreEqual(0.04, firstOperation.Value); //undefined

            var secondInitialSalinityOperation = (SetValueOperation)def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialSalinityDataItemName)[1];
            Assert.AreEqual(PointwiseOperationType.Add, secondInitialSalinityOperation.OperationType);
            Assert.AreEqual(10.0, secondInitialSalinityOperation.Value); //undefined
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSampleForcings()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.ext");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def);
            
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.AreEqual(1, def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName).Count);

            IList<ISpatialOperation> roughnessOperations = def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
            
            Assert.IsTrue(roughnessOperations[0] is ImportSamplesOperation);

            var sampleDef = (ImportSamplesOperation)roughnessOperations[0];
            Assert.AreEqual("chezy", sampleDef.Name);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CheckReadWriteOfSampleForcingsWithAOperator()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"chezy_samples\chezy_A.ext");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def);

            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.ViscosityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            Assert.IsNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.IsNotNull(def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName));

            var roughnessOperations = def.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
            Assert.AreEqual(2, roughnessOperations.Count);

            var samplesOperation = (ImportSamplesOperation)roughnessOperations[1];
            Assert.AreEqual("chezy", samplesOperation.Name);
            Assert.AreEqual(4, samplesOperation.GetPoints().Count());

            const string newPath = "local.ext";
            extForceFile.Write(newPath, def); // write loaded definition to new location

            var newExtFile = new ExtForceFile();
            var newDef = new WaterFlowFMModelDefinition();
            
            newExtFile.Read(newPath, newDef); // load written definition back
            var newRoughnessOperations = newDef.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName);
            Assert.AreEqual(4, ((ImportSamplesOperation)newRoughnessOperations[1]).GetPoints().Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWriteSampleForcingsWaterLevel()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"chezy_samples\waterlevel.ext");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def);

            Assert.AreEqual(1, def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName).Count);

            // add polygon
            var geometry = new Polygon(new LinearRing(new []
                {
                    new Coordinate(-135, -105), new Coordinate(-85, -100),
                    new Coordinate(-75, -205), new Coordinate(-125, -200),
                    new Coordinate(-135, -105)
                }));
            var f = new Feature
            {
                    Geometry = geometry,
                };
            var maskCollection = new FeatureCollection(new[] {f}, typeof (Feature));
            var operation = new SetValueOperation
            {
                Name = "poly",
            };
            operation.SetInputData(SpatialOperation.MaskInputName, maskCollection);
            def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName).Add(operation);

            // add samples
            var samples = new AddSamplesOperation(false);
            samples.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
            {
                    PointCloud = new PointCloud
                    {
                            PointValues = new List<IPointValue>
                            {
                                    new PointValue { X = 5, Y = 5, Value = 12},
                                    new PointValue { X = 10, Y = 10, Value = 30},
                                    new PointValue { X = 20, Y = 10, Value = 31},
                                },
                        },
                });
            
            def.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName).Add(samples);

            const string newExtPath = "test.ext";
            extForceFile.Write(newExtPath, def);

            var newDef = new WaterFlowFMModelDefinition();
            var newExtFile = new ExtForceFile();
            newExtFile.Read(newExtPath, newDef);

            Assert.AreEqual(3, newDef.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName).Count);
            Assert.AreEqual(3,
                ((ImportSamplesOperation)
                    newDef.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName)[2]).GetPoints()
                    .Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ExportImportBoundaryConditionWithOffsetAndFactor()
        {
            var model = new WaterFlowFMModel();

            var feature = new Feature2D
            {
                Name = "boundary",
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.AstroComponents)
            {
                Feature = feature,
                Offset = -0.3,
                Factor = 2.5
            };

            bc1.AddPoint(1);
            var data = bc1.GetDataAtPoint(1);
            data["M1"] = new[] { 0.5, 120 };

            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[] { 0.7, 60 };
            AddBoundaryCondition(model, bc1);

            var mduPath = Path.GetFullPath(@"exportbc.mdu");

            model.ExportTo(mduPath);

            var importedModel = new WaterFlowFMModel(mduPath);
            var boundaries = importedModel.Boundaries;
            Assert.AreEqual(1, boundaries.Count);
            Assert.AreEqual(feature.Geometry, boundaries.First().Geometry);

            var boundaryConditions = importedModel.BoundaryConditions.ToList();
            Assert.AreEqual(1, boundaryConditions.Count);

            Assert.AreEqual(-0.3, ((FlowBoundaryCondition)boundaryConditions[0]).Offset);
            Assert.AreEqual(2.5, ((FlowBoundaryCondition)boundaryConditions[0]).Factor);

            var pointData1 = ((BoundaryCondition)boundaryConditions[0]).GetDataAtPoint(1);
            Assert.AreEqual(pointData1.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M1" });
            Assert.AreEqual(pointData1.Components[0].Values.OfType<double>().ToArray(), new[] { 0.5 });
            Assert.AreEqual(pointData1.Components[1].Values.OfType<double>().ToArray(), new[] { 120 });

            var pointData2 = ((BoundaryCondition)boundaryConditions[0]).GetDataAtPoint(2);
            Assert.AreEqual(pointData2.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M2" });
            Assert.AreEqual(pointData2.Components[0].Values.OfType<double>().ToArray(), new[] { 0.7 });
            Assert.AreEqual(pointData2.Components[1].Values.OfType<double>().ToArray(), new[] { 60 });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportMultipleBoundaryConditionsOnSameFeature()
        {
            var model = new WaterFlowFMModel();
            
            var feature = new Feature2D
                {
                    Name = "boundary",
                    Geometry =
                        new LineString(new [] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0)})
                };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents)
                {
                    Feature = feature
                };

            bc1.AddPoint(1);
            var data = bc1.GetDataAtPoint(1);
            data["M1"] = new[] {0.5, 120};
            
            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[] {0.7, 60};
            AddBoundaryCondition(model, bc1);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.NormalVelocity, BoundaryConditionDataType.AstroComponents)
                {
                    Feature = feature
                };
            bc2.AddPoint(1);
            data = bc2.GetDataAtPoint(1);
            data["M1"] = new[] {0.6, 0};
            bc2.AddPoint(2);
            data = bc2.GetDataAtPoint(2);
            data["M2"] = new[] {0.8, 30};
            AddBoundaryCondition(model, bc2);

            var mduPath = Path.GetFullPath(@"exportbcs.mdu");

            model.ExportTo(mduPath);

            var importedModel = new WaterFlowFMModel(mduPath);
            var boundaries = importedModel.Boundaries;
            Assert.AreEqual(1, boundaries.Count);
            Assert.AreEqual(feature.Geometry, boundaries.First().Geometry);

            var boundaryConditions = importedModel.BoundaryConditions.ToList();
            Assert.AreEqual(2, boundaryConditions.Count);

            var pointData1 = ((BoundaryCondition)boundaryConditions[0]).GetDataAtPoint(1);
            Assert.AreEqual(pointData1.Arguments[0].Values.OfType<string>().ToArray(), new[] {"M1"});
            Assert.AreEqual(pointData1.Components[0].Values.OfType<double>().ToArray(), new[] {0.5});
            Assert.AreEqual(pointData1.Components[1].Values.OfType<double>().ToArray(), new[] {120});

            var pointData2 = ((BoundaryCondition)boundaryConditions[0]).GetDataAtPoint(2);
            Assert.AreEqual(pointData2.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M2" });
            Assert.AreEqual(pointData2.Components[0].Values.OfType<double>().ToArray(), new[] { 0.7 });
            Assert.AreEqual(pointData2.Components[1].Values.OfType<double>().ToArray(), new[] { 60 });

            var pointData3 = ((BoundaryCondition)boundaryConditions[1]).GetDataAtPoint(1);
            Assert.AreEqual(pointData3.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M1" });
            Assert.AreEqual(pointData3.Components[0].Values.OfType<double>().ToArray(), new[] { 0.6 });
            Assert.AreEqual(pointData3.Components[1].Values.OfType<double>().ToArray(), new[] { 0 });

            var pointData4 = ((BoundaryCondition)boundaryConditions[1]).GetDataAtPoint(2);
            Assert.AreEqual(pointData4.Arguments[0].Values.OfType<string>().ToArray(), new[] { "M2" });
            Assert.AreEqual(pointData4.Components[0].Values.OfType<double>().ToArray(), new[] { 0.8 });
            Assert.AreEqual(pointData4.Components[1].Values.OfType<double>().ToArray(), new[] { 30 });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportSummedWaterLevelsOnSameFeature()
        {
            var model = new WaterFlowFMModel() {Name = "test"};

            var feature = new Feature2D
            {
                Name = "boundary",
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents)
            {
                Feature = feature
            };

            bc1.AddPoint(1);
            var data = bc1.GetDataAtPoint(1);
            data["M1"] = new[] { 0.5, 120 };

            bc1.AddPoint(2);
            data = bc1.GetDataAtPoint(2);
            data["M2"] = new[] { 0.7, 60 };
            AddBoundaryCondition(model, bc1);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Harmonics)
            {
                Feature = feature
            };
            bc2.AddPoint(1);
            data = bc2.GetDataAtPoint(1);
            data[250.0] = new[] { 0.6, 0 };
            bc2.AddPoint(2);
            data = bc2.GetDataAtPoint(2);
            data[360.0] = new[] { 0.8, 30 };
            AddBoundaryCondition(model, bc2);

            var mduPath = Path.GetFullPath(@"exportwls.mdu");

            model.ExportTo(mduPath);

            
            var path = model.BndExtFilePath;
            var blocks = new DelftIniReader().ReadDelftIniFile(path);
            Assert.AreEqual(2, blocks.Count());
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ReadExtForcingsShouldBeFast()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"dcsm\dcsmv6.ext");

            var extForceFile = new ExtForceFile();
            TestHelper.AssertIsFasterThan(30000, () => extForceFile.Read(extPath, def));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteSourcesAndSinksTest()
        {
            var def = new WaterFlowFMModelDefinition();

            var extPath = TestHelper.GetTestFilePath(@"c070_sourcesink_2D\sourcesink_2D.ext");

            var extForceFile = new ExtForceFile();
            extForceFile.Read(extPath, def);

            Assert.AreEqual(6,def.Pipes.Count);
            Assert.AreEqual(6, def.SourcesAndSinks.Count);

            Assert.AreEqual(1.5d, def.SourcesAndSinks[0].Area);

            extForceFile.Write("sourcesink.ext", def);

            Assert.IsTrue(File.Exists("sourcesink.ext"));

            Assert.IsTrue(File.Exists("chan2_east_outflow.pli"));
            Assert.IsTrue(File.Exists("chan2_east_outflow.tim"));
        }
    }
}
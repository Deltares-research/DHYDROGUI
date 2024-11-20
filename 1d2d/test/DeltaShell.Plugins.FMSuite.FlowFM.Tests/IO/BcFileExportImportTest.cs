using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class BcFileExportImportTest
    {
        [SetUp]
        public void Setup()
        {
            if (Map.CoordinateSystemFactory == null) Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [Test]
        public void CheckCaseInsensitivity()
        {
            var bcFile = TestHelper.GetTestFilePath(@"BcFiles/BoundsCaseInsensitive.bc");
            var importer = new BcFileImporter { DeleteDataBeforeImport = false, FilePaths = new[] { bcFile } };


            var bcSet1 = CreateDummyFlowBoundaryConditionSet("140703_391887");
            var bcSet2 = CreateDummyFlowBoundaryConditionSet("141134_395446");
            var bcSet3 = CreateDummyFlowBoundaryConditionSet("nieuw9");
            IEventedList<BoundaryConditionSet> boundaryConditionSets = new EventedList<BoundaryConditionSet>(new [] { bcSet1, bcSet2, bcSet3 });

            importer.ImportItem(null, boundaryConditionSets);

            var bcData1 = bcSet1.BoundaryConditions[0];
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, bcData1.DataType);
            IFunction function1 = (IFunction) ((EventedList<DelftTools.Functions.IFunction>)bcData1.Data)[0];
            Assert.AreEqual("Discharge" , bcData1.VariableName);
            Assert.AreEqual(InterpolationType.Linear, function1.Components[0].InterpolationType) ;

            var bcData2 = bcSet2.BoundaryConditions[0];
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, bcData2.DataType);
            IFunction function2 = (IFunction)((EventedList<DelftTools.Functions.IFunction>)bcData2.Data)[0];
            Assert.AreEqual("WaterLevel", bcData2.VariableName);
            Assert.AreEqual(InterpolationType.Linear, function2.Components[0].InterpolationType);

            var bcData3 = bcSet3.BoundaryConditions[0];
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, bcData3.DataType);
            IFunction function3 = (IFunction)((EventedList<DelftTools.Functions.IFunction>)bcData3.Data)[0];
            Assert.AreEqual("Discharge", bcData3.VariableName);
            Assert.AreEqual(InterpolationType.Linear, function3.Components[0].InterpolationType);
        }

        private static BoundaryConditionSet CreateDummyFlowBoundaryConditionSet(string locationName)
        {
            Geometry geom = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)});
            var feature2D = new Feature2D {Name = locationName, Geometry = geom};
            var flowBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                BoundaryConditionDataType.Constant) {Feature = feature2D};
            BoundaryConditionSet bcSet = new BoundaryConditionSet() {Feature = feature2D};
            return bcSet;
        }

        [Test]
        public void ExportImportSalinityToSingleFile()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"harlingen/har.mdu"));

            var salinityBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                     .ToList();

            var salinityConditionCount = salinityBoundaryConditions.Count;
            var salinityDataPointCount = salinityBoundaryConditions.Sum(bc => bc.DataPointIndices.Count);
            var salinityArgumentValues =
                salinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                          .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>()).ToList();
            var salinityComponentValues =
                salinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                          .SelectMany(f => f.Components[0].Values.OfType<double>()).ToList();


            var exporter = new BcFileExporter
                {
                    ExcludedQuantities = new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.Discharge},
                    WriteMode = BcFile.WriteMode.SingleFile
                };
            exporter.Export(model.BoundaryConditionSets, "har_sal.bc");

            foreach (var boundaryConditionSet in model.BoundaryConditionSets)
            {
                foreach (
                    var boundaryCondition in boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                    {
                        boundaryCondition.ClearData();
                    }
                }
            }

            var importer = new BcFileImporter {DeleteDataBeforeImport = false, FilePaths = new[] {"har_sal.bc"}};
            importer.ImportItem(null, model.BoundaryConditionSets);

            var importedSalinityBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                     .ToList();

            Assert.AreEqual(salinityConditionCount, importedSalinityBoundaryConditions.Count);

            Assert.AreEqual(salinityDataPointCount,
                            importedSalinityBoundaryConditions.Sum(bc => bc.DataPointIndices.Count));

            var importedSalinityArgumentValues =
                importedSalinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                                  .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>())
                                                  .ToList();

            Assert.IsTrue(salinityArgumentValues.SequenceEqual(importedSalinityArgumentValues));

            var importedSalinityComponentValues =
                importedSalinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                                  .SelectMany(f => f.Components[0].Values.OfType<double>())
                                                  .ToList();

            Assert.IsTrue(salinityComponentValues.SequenceEqual(importedSalinityComponentValues));
        }

        [Test]
        public void ExportImportWaterLevelToFilePerFeature()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"harlingen/har.mdu"));

            var waterLevelBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel)
                     .ToList();

            var waterLevelBoundaryNames =
                waterLevelBoundaryConditions.Select(bc => bc.Feature).Distinct().Select(f => f.Name);

            var waterLevelConditionCount = waterLevelBoundaryConditions.Count;
            var waterLevelDataPointCount = waterLevelBoundaryConditions.Sum(bc => bc.DataPointIndices.Count);
            var waterLevelArgumentValues =
                waterLevelBoundaryConditions.SelectMany(bc => bc.PointData)
                                            .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>()).ToList();
            var waterLevelComponentValues =
                waterLevelBoundaryConditions.SelectMany(bc => bc.PointData)
                                            .SelectMany(f => f.Components[0].Values.OfType<double>()).ToList();


            var exporter = new BcFileExporter
                {
                    ExcludedQuantities = new[] {FlowBoundaryQuantityType.Salinity, FlowBoundaryQuantityType.Discharge},
                    WriteMode = BcFile.WriteMode.FilePerFeature
                };
            exporter.Export(model.BoundaryConditionSets, "harwaterlevel.bc");

            foreach (var boundaryConditionSet in model.BoundaryConditionSets)
            {
                foreach (
                    var boundaryCondition in boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.WaterLevel)
                    {
                        boundaryCondition.ClearData();
                    }
                }
            }

            var filePaths = waterLevelBoundaryNames.Select(n => "harwaterlevel_" + n + ".bc").ToArray();

            foreach (var filePath in filePaths)
            {
                Assert.IsTrue(File.Exists(filePath));
            }

            var importer = new BcFileImporter {DeleteDataBeforeImport = false, FilePaths = filePaths};
            importer.ImportItem(null, model.BoundaryConditionSets);

            var importedWaterLevelBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel)
                     .ToList();

            Assert.AreEqual(waterLevelConditionCount, importedWaterLevelBoundaryConditions.Count);

            Assert.AreEqual(waterLevelDataPointCount,
                            importedWaterLevelBoundaryConditions.Sum(bc => bc.DataPointIndices.Count));

            var importedSalinityArgumentValues =
                importedWaterLevelBoundaryConditions.SelectMany(bc => bc.PointData)
                                                    .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>())
                                                    .ToList();

            Assert.IsTrue(waterLevelArgumentValues.SequenceEqual(importedSalinityArgumentValues));

            var importedSalinityComponentValues =
                importedWaterLevelBoundaryConditions.SelectMany(bc => bc.PointData)
                                                    .SelectMany(f => f.Components[0].Values.OfType<double>())
                                                    .ToList();

            Assert.IsTrue(waterLevelComponentValues.SequenceEqual(importedSalinityComponentValues));
        }

        [Test]
        public void ExportImportWaterLevelSalinityToSeparateFiles()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"harlingen/har.mdu"));

            var salinityBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                     .ToList();

            var salinityConditionCount = salinityBoundaryConditions.Count;
            var salinityDataPointCount = salinityBoundaryConditions.Sum(bc => bc.DataPointIndices.Count);
            var salinityArgumentValues =
                salinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                          .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>()).ToList();
            var salinityComponentValues =
                salinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                          .SelectMany(f => f.Components[0].Values.OfType<double>()).ToList();


            var exporter = new BcFileExporter
            {
                ExcludedQuantities = new[] { FlowBoundaryQuantityType.Discharge },
                WriteMode = BcFile.WriteMode.FilePerProcess
            };
            exporter.Export(model.BoundaryConditionSets, "har.bc");

            Assert.IsTrue(File.Exists("har_salinity.bc"));

            foreach (var boundaryConditionSet in model.BoundaryConditionSets)
            {
                foreach (
                    var boundaryCondition in boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                    {
                        boundaryCondition.ClearData();
                    }
                }
            }

            var importer = new BcFileImporter { DeleteDataBeforeImport = false, FilePaths = new[] { "har_salinity.bc" } };
            importer.ImportItem(null, model.BoundaryConditionSets);

            var importedSalinityBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                     .ToList();

            Assert.AreEqual(salinityConditionCount, importedSalinityBoundaryConditions.Count);

            Assert.AreEqual(salinityDataPointCount,
                            importedSalinityBoundaryConditions.Sum(bc => bc.DataPointIndices.Count));

            var importedSalinityArgumentValues =
                importedSalinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                                  .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>())
                                                  .ToList();

            Assert.IsTrue(salinityArgumentValues.SequenceEqual(importedSalinityArgumentValues));

            var importedSalinityComponentValues =
                importedSalinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                                  .SelectMany(f => f.Components[0].Values.OfType<double>())
                                                  .ToList();

            Assert.IsTrue(salinityComponentValues.SequenceEqual(importedSalinityComponentValues));
        }

        [Test]
        public void ExportImportWaterLevelHarmonicCorrectionsToSeparateFiles()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"simplebox/simplebox.mdu"));

            var harmonicBoundaryCondition =
                model.BoundaryConditions.First(bc => bc.DataType == BoundaryConditionDataType.Harmonics);

            var dataIndices = harmonicBoundaryCondition.DataPointIndices.ToList();

            foreach (var dataIndex in dataIndices)
            {
                harmonicBoundaryCondition.RemovePoint(dataIndex);
            }

            harmonicBoundaryCondition.DataType = BoundaryConditionDataType.HarmonicCorrection;

            harmonicBoundaryCondition.AddPoint(0);

            var frequencies = new[] {0.0, 6.5, 23.5};
            var amplitudes = new[] {1.25, 0.12, 0.4};
            var phases = new[] {0.0, 132, 230};
            var ampcorrections = new[] {0.0, 1.1, 0.99};
            var phasecorrections = new[] {0.0, 10.4, 20};

            var data = harmonicBoundaryCondition.PointData[0];
            FunctionHelper.SetValuesRaw<double>(data.Arguments[0], frequencies);
            FunctionHelper.SetValuesRaw<double>(data.Components[0], amplitudes);
            FunctionHelper.SetValuesRaw<double>(data.Components[1], phases);
            FunctionHelper.SetValuesRaw<double>(data.Components[2], ampcorrections);
            FunctionHelper.SetValuesRaw<double>(data.Components[3], phasecorrections);

            var exporter = new BcFileExporter
            {
                WriteMode = BcFile.WriteMode.SingleFile
            };
            exporter.Export(model.BoundaryConditionSets, "simplebox.bc");

            Assert.IsTrue(File.Exists("simplebox.bc"));
            Assert.IsTrue(File.Exists("simplebox_corr.bc"));

            model.BoundaryConditionSets[0].BoundaryConditions.Clear();

            var importer = new BcFileImporter
            {
                DeleteDataBeforeImport = false,

                FilePaths = new[] {"simplebox.bc", "simplebox_corr.bc"}
            };
            importer.ImportItem(null, model.BoundaryConditionSets);

            var correctionBc =
                model.BoundaryConditions.FirstOrDefault(
                    bc => bc.DataType == BoundaryConditionDataType.HarmonicCorrection);

            Assert.IsNotNull(correctionBc);
            Assert.IsTrue(correctionBc.DataPointIndices.Contains(0));

            data = correctionBc.GetDataAtPoint(0);

            ListTestUtils.AssertAreEqual(data.Arguments[0].GetValues<double>().ToList(), frequencies, 0.0000000001);
            Assert.IsTrue(data.Components[0].GetValues<double>().ToList().SequenceEqual(amplitudes));
            Assert.IsTrue(data.Components[1].GetValues<double>().ToList().SequenceEqual(phases));
            Assert.IsTrue(data.Components[2].GetValues<double>().ToList().SequenceEqual(ampcorrections));
            Assert.IsTrue(data.Components[3].GetValues<double>().ToList().SequenceEqual(phasecorrections));
        }

        [Test]
        public void ExportImportSedimentConcentrationToSingleFile()
        {
            //Note, for the moment we assume these type of sediments are compatible with waterflowfm.
            var testFilePath = TestHelper.GetTestFilePath(@"simplebox/simplebox.mdu");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);
            var model = new WaterFlowFMModel(testFilePath);
            model.Name = "newname";

            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac = new SedimentFraction() { Name = "frac1" };
            model.SedimentFractions.Add(sedFrac);

            var boundary = new Feature2D
            {
                Name = "bound",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
            };
            model.Boundaries.Add(boundary);

            var fbcFactory = new FlowBoundaryConditionFactory();
            fbcFactory.Model = model;
            var bCond = fbcFactory.CreateBoundaryCondition(boundary,
                sedFrac.Name,
                BoundaryConditionDataType.TimeSeries,
                FlowBoundaryQuantityType.SedimentConcentration.GetDescription());

            model.BoundaryConditionSets[0].BoundaryConditions.Add(bCond);

            bCond.AddPoint(0);
            var dataAtZero = bCond.GetDataAtPoint(0);
            dataAtZero[new DateTime(2000, 1, 1)] = new[] { 36.0 };

            var exporter = new BcFileExporter
            {
                WriteMode = BcFile.WriteMode.SingleFile
            };
            exporter.Export(model.BoundaryConditionSets, "sedimentConcentration.bc");

            Assert.IsTrue(File.Exists("sedimentConcentration.bc"));

            var fileText = File.ReadAllText("sedimentConcentration.bc");
            Assert.IsTrue(fileText.Contains(BcFileFlowBoundaryDataBuilder.ConcentrationAtBound + "frac1") &&
                          fileText.Contains("L1_0001") &&
                          fileText.Contains(new BcFile().BlockKey) &&
                          fileText.Contains(BcFile.QuantityKey));
            //Import
            var importer = new BcFileImporter
            {
                DeleteDataBeforeImport = false,

                FilePaths = new[] { "sedimentConcentration.bc" }
            };
            importer.ImportItem(null, model.BoundaryConditionSets);

            var scBoundCond =
                model.BoundaryConditions.FirstOrDefault(
                    bc => bc.DataType == BoundaryConditionDataType.TimeSeries);

            Assert.IsNotNull(scBoundCond);
            Assert.IsTrue(scBoundCond.DataPointIndices.Contains(0));

            var data = scBoundCond.GetDataAtPoint(0);
            Assert.That(data.GetValues<double>().First(), Is.EqualTo(36.0).Within(0.01));
        }



    }
}

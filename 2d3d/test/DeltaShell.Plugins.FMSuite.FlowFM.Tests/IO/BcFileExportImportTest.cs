using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class BcFileExportImportTest
    {
        [Test]
        public void ExportImportSalinityToSingleFile()
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"harlingen/har.mdu"));

            List<FlowBoundaryCondition> salinityBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                     .ToList();

            int salinityConditionCount = salinityBoundaryConditions.Count;
            int salinityDataPointCount = salinityBoundaryConditions.Sum(bc => bc.DataPointIndices.Count);
            List<DateTime> salinityArgumentValues =
                salinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                          .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>()).ToList();
            List<double> salinityComponentValues =
                salinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                          .SelectMany(f => f.Components[0].Values.OfType<double>()).ToList();

            var exporter = new BcFileExporter
            {
                ExcludedQuantities = new[]
                {
                    FlowBoundaryQuantityType.WaterLevel,
                    FlowBoundaryQuantityType.Discharge
                },
                WriteMode = BcFile.WriteMode.SingleFile
            };
            exporter.Export(model.BoundaryConditionSets, "har_sal.bc");

            foreach (BoundaryConditionSet boundaryConditionSet in model.BoundaryConditionSets)
            {
                foreach (
                    FlowBoundaryCondition boundaryCondition in boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                    {
                        boundaryCondition.ClearData();
                    }
                }
            }

            var importer = new BcFileImporter
            {
                DeleteDataBeforeImport = false,
                FilePaths = new[]
                {
                    "har_sal.bc"
                }
            };
            importer.ImportItem(null, model.BoundaryConditionSets);

            List<FlowBoundaryCondition> importedSalinityBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                     .ToList();

            Assert.AreEqual(salinityConditionCount, importedSalinityBoundaryConditions.Count);

            Assert.AreEqual(salinityDataPointCount,
                            importedSalinityBoundaryConditions.Sum(bc => bc.DataPointIndices.Count));

            List<DateTime> importedSalinityArgumentValues =
                importedSalinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                                  .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>())
                                                  .ToList();

            Assert.IsTrue(salinityArgumentValues.SequenceEqual(importedSalinityArgumentValues));

            List<double> importedSalinityComponentValues =
                importedSalinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                                  .SelectMany(f => f.Components[0].Values.OfType<double>())
                                                  .ToList();

            Assert.IsTrue(salinityComponentValues.SequenceEqual(importedSalinityComponentValues));
        }

        [Test]
        public void ExportImportWaterLevelToFilePerFeature()
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"harlingen/har.mdu"));

            List<FlowBoundaryCondition> waterLevelBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel)
                     .ToList();

            IEnumerable<string> waterLevelBoundaryNames =
                waterLevelBoundaryConditions.Select(bc => bc.Feature).Distinct().Select(f => f.Name);

            int waterLevelConditionCount = waterLevelBoundaryConditions.Count;
            int waterLevelDataPointCount = waterLevelBoundaryConditions.Sum(bc => bc.DataPointIndices.Count);
            List<DateTime> waterLevelArgumentValues =
                waterLevelBoundaryConditions.SelectMany(bc => bc.PointData)
                                            .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>()).ToList();
            List<double> waterLevelComponentValues =
                waterLevelBoundaryConditions.SelectMany(bc => bc.PointData)
                                            .SelectMany(f => f.Components[0].Values.OfType<double>()).ToList();

            var exporter = new BcFileExporter
            {
                ExcludedQuantities = new[]
                {
                    FlowBoundaryQuantityType.Salinity,
                    FlowBoundaryQuantityType.Discharge
                },
                WriteMode = BcFile.WriteMode.FilePerFeature
            };
            exporter.Export(model.BoundaryConditionSets, "harwaterlevel.bc");

            foreach (BoundaryConditionSet boundaryConditionSet in model.BoundaryConditionSets)
            {
                foreach (
                    FlowBoundaryCondition boundaryCondition in boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.WaterLevel)
                    {
                        boundaryCondition.ClearData();
                    }
                }
            }

            string[] filePaths = waterLevelBoundaryNames.Select(n => "harwaterlevel_" + n + ".bc").ToArray();

            foreach (string filePath in filePaths)
            {
                Assert.IsTrue(File.Exists(filePath));
            }

            var importer = new BcFileImporter
            {
                DeleteDataBeforeImport = false,
                FilePaths = filePaths
            };
            importer.ImportItem(null, model.BoundaryConditionSets);

            List<FlowBoundaryCondition> importedWaterLevelBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel)
                     .ToList();

            Assert.AreEqual(waterLevelConditionCount, importedWaterLevelBoundaryConditions.Count);

            Assert.AreEqual(waterLevelDataPointCount,
                            importedWaterLevelBoundaryConditions.Sum(bc => bc.DataPointIndices.Count));

            List<DateTime> importedSalinityArgumentValues =
                importedWaterLevelBoundaryConditions.SelectMany(bc => bc.PointData)
                                                    .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>())
                                                    .ToList();

            Assert.IsTrue(waterLevelArgumentValues.SequenceEqual(importedSalinityArgumentValues));

            List<double> importedSalinityComponentValues =
                importedWaterLevelBoundaryConditions.SelectMany(bc => bc.PointData)
                                                    .SelectMany(f => f.Components[0].Values.OfType<double>())
                                                    .ToList();

            Assert.IsTrue(waterLevelComponentValues.SequenceEqual(importedSalinityComponentValues));
        }

        [Test]
        public void ExportImportWaterLevelSalinityToSeparateFiles()
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"harlingen/har.mdu"));

            List<FlowBoundaryCondition> salinityBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                     .ToList();

            int salinityConditionCount = salinityBoundaryConditions.Count;
            int salinityDataPointCount = salinityBoundaryConditions.Sum(bc => bc.DataPointIndices.Count);
            List<DateTime> salinityArgumentValues =
                salinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                          .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>()).ToList();
            List<double> salinityComponentValues =
                salinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                          .SelectMany(f => f.Components[0].Values.OfType<double>()).ToList();

            var exporter = new BcFileExporter
            {
                ExcludedQuantities = new[]
                {
                    FlowBoundaryQuantityType.Discharge
                },
                WriteMode = BcFile.WriteMode.FilePerProcess
            };
            exporter.Export(model.BoundaryConditionSets, "har.bc");

            Assert.IsTrue(File.Exists("har_salinity.bc"));

            foreach (BoundaryConditionSet boundaryConditionSet in model.BoundaryConditionSets)
            {
                foreach (
                    FlowBoundaryCondition boundaryCondition in boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    if (boundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                    {
                        boundaryCondition.ClearData();
                    }
                }
            }

            var importer = new BcFileImporter
            {
                DeleteDataBeforeImport = false,
                FilePaths = new[]
                {
                    "har_salinity.bc"
                }
            };
            importer.ImportItem(null, model.BoundaryConditionSets);

            List<FlowBoundaryCondition> importedSalinityBoundaryConditions =
                model.BoundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions.OfType<FlowBoundaryCondition>())
                     .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Salinity)
                     .ToList();

            Assert.AreEqual(salinityConditionCount, importedSalinityBoundaryConditions.Count);

            Assert.AreEqual(salinityDataPointCount,
                            importedSalinityBoundaryConditions.Sum(bc => bc.DataPointIndices.Count));

            List<DateTime> importedSalinityArgumentValues =
                importedSalinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                                  .SelectMany(f => f.Arguments[0].Values.OfType<DateTime>())
                                                  .ToList();

            Assert.IsTrue(salinityArgumentValues.SequenceEqual(importedSalinityArgumentValues));

            List<double> importedSalinityComponentValues =
                importedSalinityBoundaryConditions.SelectMany(bc => bc.PointData)
                                                  .SelectMany(f => f.Components[0].Values.OfType<double>())
                                                  .ToList();

            Assert.IsTrue(salinityComponentValues.SequenceEqual(importedSalinityComponentValues));
        }

        [Test]
        public void ExportImportWaterLevelHarmonicCorrectionsToSeparateFiles()
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"simplebox/simplebox.mdu"));

            IBoundaryCondition harmonicBoundaryCondition =
                model.BoundaryConditions.First(bc => bc.DataType == BoundaryConditionDataType.Harmonics);

            List<int> dataIndices = harmonicBoundaryCondition.DataPointIndices.ToList();

            foreach (int dataIndex in dataIndices)
            {
                harmonicBoundaryCondition.RemovePoint(dataIndex);
            }

            harmonicBoundaryCondition.DataType = BoundaryConditionDataType.HarmonicCorrection;

            harmonicBoundaryCondition.AddPoint(0);

            var frequencies = new[]
            {
                0.0,
                6.5,
                23.5
            };
            var amplitudes = new[]
            {
                1.25,
                0.12,
                0.4
            };
            double[] phases = new[]
            {
                0.0,
                132,
                230
            };
            var ampcorrections = new[]
            {
                0.0,
                1.1,
                0.99
            };
            double[] phasecorrections = new[]
            {
                0.0,
                10.4,
                20
            };

            IFunction data = harmonicBoundaryCondition.PointData[0];
            FunctionHelper.SetValuesRaw<double>(data.Arguments[0], frequencies);
            FunctionHelper.SetValuesRaw<double>(data.Components[0], amplitudes);
            FunctionHelper.SetValuesRaw<double>(data.Components[1], phases);
            FunctionHelper.SetValuesRaw<double>(data.Components[2], ampcorrections);
            FunctionHelper.SetValuesRaw<double>(data.Components[3], phasecorrections);

            var exporter = new BcFileExporter {WriteMode = BcFile.WriteMode.SingleFile};
            exporter.Export(model.BoundaryConditionSets, "simplebox.bc");

            Assert.IsTrue(File.Exists("simplebox.bc"));
            Assert.IsTrue(File.Exists("simplebox_corr.bc"));

            model.BoundaryConditionSets[0].BoundaryConditions.Clear();

            var importer = new BcFileImporter
            {
                DeleteDataBeforeImport = false,
                FilePaths = new[]
                {
                    "simplebox.bc",
                    "simplebox_corr.bc"
                }
            };
            importer.ImportItem(null, model.BoundaryConditionSets);

            IBoundaryCondition correctionBc =
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
            string testFilePath = TestHelper.GetTestFilePath(@"simplebox/simplebox.mdu");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(testFilePath);

            model.Name = "newname";

            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac = new SedimentFraction() {Name = "frac1"};
            model.SedimentFractions.Add(sedFrac);

            var boundary = new Feature2D
            {
                Name = "bound",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100)
                })
            };
            model.Boundaries.Add(boundary);

            var fbcFactory = new FlowBoundaryConditionFactory();
            fbcFactory.Model = model;
            IBoundaryCondition bCond = fbcFactory.CreateBoundaryCondition(boundary,
                                                                          sedFrac.Name,
                                                                          BoundaryConditionDataType.TimeSeries,
                                                                          FlowBoundaryQuantityType.SedimentConcentration.GetDescription());

            model.BoundaryConditionSets[0].BoundaryConditions.Add(bCond);

            bCond.AddPoint(0);
            IFunction dataAtZero = bCond.GetDataAtPoint(0);
            dataAtZero[new DateTime(2000, 1, 1)] = new[]
            {
                36.0
            };

            var exporter = new BcFileExporter {WriteMode = BcFile.WriteMode.SingleFile};
            exporter.Export(model.BoundaryConditionSets, "sedimentConcentration.bc");

            Assert.IsTrue(File.Exists("sedimentConcentration.bc"));

            string fileText = File.ReadAllText("sedimentConcentration.bc");
            Assert.That(fileText, Does.Contain(ExtForceQuantNames.ConcentrationAtBound + "frac1").And.Contains("L1_0001").And.Contains(BcFile.BlockKey).And.Contains(BcFile.BlockKey).And.Contains(BcFile.QuantityKey));
            //Import
            var importer = new BcFileImporter
            {
                DeleteDataBeforeImport = false,
                FilePaths = new[]
                {
                    "sedimentConcentration.bc"
                }
            };
            importer.ImportItem(null, model.BoundaryConditionSets);

            IBoundaryCondition scBoundCond =
                model.BoundaryConditions.FirstOrDefault(
                    bc => bc.DataType == BoundaryConditionDataType.TimeSeries);

            Assert.IsNotNull(scBoundCond);
            Assert.IsTrue(scBoundCond.DataPointIndices.Contains(0));

            IFunction data = scBoundCond.GetDataAtPoint(0);
            Assert.That(data.GetValues<double>().First(), Is.EqualTo(36.0).Within(0.01));
        }
    }
}
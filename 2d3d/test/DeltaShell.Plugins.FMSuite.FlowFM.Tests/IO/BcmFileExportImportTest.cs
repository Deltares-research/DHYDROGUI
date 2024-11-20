using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Units;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
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
    public class BcmFileExportImportTest
    {
        [Test]
        [TestCase(2, 3, @"sedmor\files\2section3parameter3records.bcm")]
        public void BcmFileIsReadForMorphology(int numberOfBoundaries, int numberOfParameters, string filePath)
        {
            var fileReader = new BcmFile();
            List<BcBlockData> dataBlocks = fileReader.Read(TestHelper.GetTestFilePath(filePath)).ToList();

            Assert.True(dataBlocks.Count == numberOfBoundaries);

            Assert.True(dataBlocks[0].Quantities.Count == numberOfParameters);
            var expectedB0 = new List<List<string>>() /* Each row represents the values per column in the file.*/
            {
                new List<string>()
                {
                    "0.00000000",
                    "2.50000000",
                    "1.4400000e+003"
                },
                new List<string>()
                {
                    "0.0",
                    "0.0",
                    "0.0"
                },
                new List<string>()
                {
                    "1",
                    "1",
                    "1"
                }
            };
            List<IList<string>> actualB0 = dataBlocks[0].Quantities.Select(q => q.Values).ToList();
            CollectionAssert.AreEqual(expectedB0, actualB0);

            Assert.True(dataBlocks[1].Quantities.Count == numberOfParameters);
            var expectedB1 = new List<List<string>>() /* Each row represents the values per column in the file.*/
            {
                new List<string>()
                {
                    "0.00000000",
                    "2.50000000",
                    "1.4400000e+003"
                },
                new List<string>()
                {
                    "0.0",
                    "0.0",
                    "0.0"
                },
                new List<string>()
                {
                    "1",
                    "1",
                    "1"
                }
            };
            List<IList<string>> actualB1 = dataBlocks[0].Quantities.Select(q => q.Values).ToList();
            CollectionAssert.AreEqual(expectedB1, actualB1);
        }

        [Test]
        public void BcmFileIsWrittenFromMorphology()
        {
            var feature = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(0, 1)
                }),
                Name = "pli1"
            };

            var boundaryConditionSet = new BoundaryConditionSet {Feature = feature};
            var flowBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport,
                                                   BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
                SedimentFractionNames = new List<string>()
                {
                    "Frac1",
                    "Frac2"
                }
            };

            Assert.NotNull(flowBc);

            //3 entries, 3 parameters (2 fractions + time).
            flowBc.AddPoint(0);

            IFunction bcData0 = flowBc.GetDataAtPoint(0);
            bcData0.Arguments[0].Unit = new Unit("-", "-");
            bcData0[new DateTime(2014, 1, 1, 0, 0, 0)] = new[]
            {
                0.2,
                -0.3
            };
            bcData0[new DateTime(2014, 1, 1, 0, 10, 0)] = new[]
            {
                0.22,
                -0.32
            };
            bcData0[new DateTime(2014, 1, 1, 0, 20, 0)] = new[]
            {
                0.26,
                -0.36
            };

            //1 entries, 5 parameters (4 comps + time).
            flowBc.AddPoint(2);
            IFunction velocityData2 = flowBc.GetDataAtPoint(2);
            velocityData2.Arguments[0].Unit = new Unit("-", "-");
            velocityData2[new DateTime(2014, 1, 1, 0, 0, 0)] = new[]
            {
                -0.28,
                0.38
            };

            boundaryConditionSet.BoundaryConditions.Add(flowBc);

            const string filePath = "BcmFileIsWrittenFromMorphology.bcm";

            var writer = new BcmFile() {MultiFileMode = BcFile.WriteMode.SingleFile};
            writer.Write(new[]
            {
                boundaryConditionSet
            }, filePath, new BcmFileFlowBoundaryDataBuilder());

            //Check whether if there are two blocks as well.
            var fileReader = new BcmFile();
            List<BcBlockData> dataBlocks = fileReader.Read(filePath).ToList();

            Assert.IsNotNull(dataBlocks);
            Assert.AreEqual(2, dataBlocks.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ExportImportSedimentBedLoadToSingleFile()
        {
            //Note, for the moment we assume these type of sediments are compatible with waterflowfm.
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"simplebox/simplebox.mdu"));

            model.Name = "newname";

            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac1 = new SedimentFraction() {Name = "frac1"};
            var sedFrac2 = new SedimentFraction() {Name = "frac2"};
            model.SedimentFractions.Add(sedFrac1);
            model.SedimentFractions.Add(sedFrac2);

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
            IBoundaryCondition boundCond = fbcFactory.CreateBoundaryCondition(boundary,
                                                                              FlowBoundaryQuantityType.MorphologyBedLoadTransport.ToString(),
                                                                              BoundaryConditionDataType.TimeSeries,
                                                                              FlowBoundaryQuantityType.MorphologyBedLoadTransport.GetDescription());

            model.BoundaryConditionSets[0].BoundaryConditions.Add(boundCond);

            boundCond.AddPoint(0);
            IFunction dataAtZero = boundCond.GetDataAtPoint(0);
            dataAtZero[new DateTime(2000, 1, 1)] = new[]
            {
                4.5,
                1.2
            };

            var exporter = new BcmFileExporter {WriteMode = BcFile.WriteMode.SingleFile};
            exporter.Export(model.BoundaryConditionSets, "sedimentBedLoadTransport.bcm");

            Assert.IsTrue(File.Exists("sedimentBedLoadTransport.bcm"));

            string fileText = File.ReadAllText("sedimentBedLoadTransport.bcm");
            Assert.That(fileText, Does.Contain("frac1").And.Contains("frac2"));

            //Import
            var importer = new BcmFileImporter
            {
                DeleteDataBeforeImport = false,
                FilePaths = new[]
                {
                    "sedimentBedLoadTransport.bcm"
                }
            };
            importer.ImportItem(null, model.BoundaryConditionSets);

            IBoundaryCondition scBoundCond =
                model.BoundaryConditions.FirstOrDefault(
                    bc => bc.DataType == BoundaryConditionDataType.TimeSeries);

            Assert.IsNotNull(scBoundCond);
            Assert.IsTrue(scBoundCond.DataPointIndices.Contains(0));

            IFunction data = scBoundCond.GetDataAtPoint(0);
            IVariable frac1 = data.Components.FirstOrDefault(a => a.Name == "frac1");
            Assert.IsNotNull(frac1);
            Assert.That(frac1.GetValues<double>().First(), Is.EqualTo(4.5).Within(0.01));
            IVariable frac2 = data.Components.FirstOrDefault(a => a.Name == "frac2");
            Assert.IsNotNull(frac2);
            Assert.That(frac2.GetValues<double>().First(), Is.EqualTo(1.2).Within(0.01));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Units;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class BcFileFlowBoundaryDataBuilderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportTwoAstroWaterLevelBoundaryConditions()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcFiles\TwoAstroWaterLevels.bc");
            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var feature = new Feature2D
                {
                    Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)}),
                    Name = "pli1"
                };

            var boundaryConditionSet = new BoundaryConditionSet {Feature = feature};

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks.ElementAt(0));
            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks.ElementAt(1));

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var boundaryCondition = boundaryConditionSet.BoundaryConditions.FirstOrDefault() as FlowBoundaryCondition;

            Assert.IsNotNull(boundaryCondition);
            Assert.AreEqual(new[] {0, 1}, boundaryCondition.DataPointIndices);
            Assert.AreEqual(BoundaryConditionDataType.AstroComponents, boundaryCondition.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel, boundaryCondition.FlowQuantity);
            Assert.AreEqual(new[] {"M2", "S2"}, boundaryCondition.GetDataAtPoint(0).Arguments[0].Values);
            Assert.AreEqual(new[] {0.9, 0.95}, boundaryCondition.GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] {10, -7.5}, boundaryCondition.GetDataAtPoint(0).Components[1].Values);
            Assert.AreEqual(new[] { 0.8, 1.1 }, boundaryCondition.GetDataAtPoint(1).Components[0].Values);
            Assert.AreEqual(new[] { 20, -11.5 }, boundaryCondition.GetDataAtPoint(1).Components[1].Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWaterLevelAndSalinityLayersConditions()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcFiles\WaterLevelAndSalinityLayers.bc");
            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var feature = new Feature2D
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }),
                Name = "pli1"
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks.ElementAt(0));

            Assert.AreEqual(2, boundaryConditionSet.BoundaryConditions.Count);

            var waterLevelBoundaryCondition =
                boundaryConditionSet.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;

            Assert.AreEqual(new[] {0}, waterLevelBoundaryCondition.DataPointIndices);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, waterLevelBoundaryCondition.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel, waterLevelBoundaryCondition.FlowQuantity);
            Assert.AreEqual(new[] {new DateTime(2013, 1, 1), new DateTime(2013, 1, 2)},
                            waterLevelBoundaryCondition.GetDataAtPoint(0).Arguments[0].Values);
            Assert.AreEqual(new[] {0.5, 0.65},
                            waterLevelBoundaryCondition.GetDataAtPoint(0).Components[0].Values);

            var salinityBoundaryCondition =
                boundaryConditionSet.BoundaryConditions.ElementAt(1) as FlowBoundaryCondition;

            Assert.IsNotNull(salinityBoundaryCondition);
            Assert.AreEqual(new[] {0}, salinityBoundaryCondition.DataPointIndices);
            Assert.AreEqual(VerticalProfileType.TopBottom,
                            salinityBoundaryCondition.GetDepthLayerDefinitionAtPoint(0).Type);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, salinityBoundaryCondition.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.Salinity, salinityBoundaryCondition.FlowQuantity);
            Assert.AreEqual(new[] { new DateTime(2013, 1, 1), new DateTime(2013, 1, 2) },
                            salinityBoundaryCondition.GetDataAtPoint(0).Arguments[0].Values);
            Assert.AreEqual(2, salinityBoundaryCondition.GetDataAtPoint(0).Components.Count);
            Assert.AreEqual(new[] {22.0, 30.0}, salinityBoundaryCondition.GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] { 0.0, 0.0 }, salinityBoundaryCondition.GetDataAtPoint(0).Components[1].Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadVelocityAndSalinityLayersConditions()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcFiles\VelocityAndSalinityLayers.bc");
            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var feature = new Feature2D
                {
                    Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)}),
                    Name = "pli1"
                };

            var boundaryConditionSet = new BoundaryConditionSet {Feature = feature};

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks);

            Assert.AreEqual(2, boundaryConditionSet.BoundaryConditions.Count);

            var velocityBC = boundaryConditionSet.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;

            Assert.AreEqual(new[] {0}, velocityBC.DataPointIndices);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, velocityBC.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.VelocityVector, velocityBC.FlowQuantity);
            Assert.AreEqual(VerticalProfileType.PercentageFromBed, velocityBC.GetDepthLayerDefinitionAtPoint(0).Type);
            Assert.AreEqual(new[] {20, 50, 70}, velocityBC.GetDepthLayerDefinitionAtPoint(0).SortedPointDepths.ToList());
            Assert.AreEqual(new[] {new DateTime(2013, 1, 1), new DateTime(2013, 9, 23, 12, 0, 0)},
                            velocityBC.GetDataAtPoint(0).Arguments[0].Values);
            Assert.AreEqual(6, velocityBC.GetDataAtPoint(0).Components.Count);
            Assert.AreEqual(new[] {0.1, 0.2}, velocityBC.GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] {-0.1, 0.1}, velocityBC.GetDataAtPoint(0).Components[1].Values);
            Assert.AreEqual(new[] {0.5, -0.1}, velocityBC.GetDataAtPoint(0).Components[2].Values);
            Assert.AreEqual(new[] {-0.3, 0.5}, velocityBC.GetDataAtPoint(0).Components[3].Values);
            Assert.AreEqual(new[] {0.8, -0.5}, velocityBC.GetDataAtPoint(0).Components[4].Values);
            Assert.AreEqual(new[] {-0.5, 0.9}, velocityBC.GetDataAtPoint(0).Components[5].Values);

            var salinityBC = boundaryConditionSet.BoundaryConditions.ElementAt(1) as FlowBoundaryCondition;

            Assert.AreEqual(new[] {0}, salinityBC.DataPointIndices);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, salinityBC.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.Salinity, salinityBC.FlowQuantity);
            Assert.AreEqual(VerticalProfileType.ZFromDatum, salinityBC.GetDepthLayerDefinitionAtPoint(0).Type);
            Assert.AreEqual(new[] {-8, -6, -4, -2, 0}, salinityBC.GetDepthLayerDefinitionAtPoint(0).SortedPointDepths);
            Assert.AreEqual(new[] {new DateTime(2013, 1, 1), new DateTime(2013, 1, 1, 0, 24, 0)},
                            salinityBC.GetDataAtPoint(0).Arguments[0].Values);
            Assert.AreEqual(5, salinityBC.GetDataAtPoint(0).Components.Count);
            Assert.AreEqual(new[] {0.5, 0.65}, salinityBC.GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] {22, 30}, salinityBC.GetDataAtPoint(0).Components[1].Values);
            Assert.AreEqual(new[] {18, 22}, salinityBC.GetDataAtPoint(0).Components[2].Values);
            Assert.AreEqual(new[] {12, 17}, salinityBC.GetDataAtPoint(0).Components[3].Values);
            Assert.AreEqual(new[] {7, 14}, salinityBC.GetDataAtPoint(0).Components[4].Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadAstroWaterLevelCondition()
        {
            var feature = new Feature2D
                {
                    Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)}),
                    Name = "pli1"
                };

            var boundaryConditionSet = new BoundaryConditionSet {Feature = feature};

            var waterLevelBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                         BoundaryConditionDataType.AstroComponents) {Feature = feature};
            waterLevelBc.AddPoint(1);
            var waterLevelAstroFunction1 = waterLevelBc.GetDataAtPoint(1);
            waterLevelAstroFunction1["A0"] = new[] {0.8, 0};
            waterLevelAstroFunction1["A1"] = new[] {0.2, 120};
            waterLevelAstroFunction1["Q1"] = new[] {0.4123, 300};

            waterLevelBc.AddPoint(2);
            var waterLevelAstroFunction2 = waterLevelBc.GetDataAtPoint(2);
            waterLevelAstroFunction2["A0"] = new[] {0.522, 0};
            waterLevelAstroFunction2["A1"] = new[] {1, 170};

            boundaryConditionSet.BoundaryConditions.Add(waterLevelBc);

            const string filePath = "AstroWaterLevel.bc";

            var writer = new BcFile() {MultiFileMode = BcFile.WriteMode.SingleFile};
            writer.Write(new[] {boundaryConditionSet}, filePath, new BcFileFlowBoundaryDataBuilder());

            boundaryConditionSet.BoundaryConditions.Clear();

            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks);

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var importedBc = boundaryConditionSet.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;

            Assert.IsNotNull(importedBc);
            Assert.AreEqual(new[] {1, 2}, importedBc.DataPointIndices);
            Assert.AreEqual(BoundaryConditionDataType.AstroComponents, importedBc.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel, importedBc.FlowQuantity);
            Assert.AreEqual(new[] {"A0", "A1", "Q1"}, importedBc.GetDataAtPoint(1).Arguments[0].Values);
            Assert.AreEqual(new[] {0.8, 0.2, 0.4123}, importedBc.GetDataAtPoint(1).Components[0].Values);
            Assert.AreEqual(new[] {0, 120, 300}, importedBc.GetDataAtPoint(1).Components[1].Values);
            Assert.AreEqual(new[] {"A0", "A1"}, importedBc.GetDataAtPoint(2).Arguments[0].Values);
            Assert.AreEqual(new[] {0.522, 1}, importedBc.GetDataAtPoint(2).Components[0].Values);
            Assert.AreEqual(new[] {0, 170}, importedBc.GetDataAtPoint(2).Components[1].Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadSalinityWithVerticalProfile()
        {
            var feature = new Feature2D
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }),
                Name = "pli1"
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };

            var salinityBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                         BoundaryConditionDataType.TimeSeries) { Feature = feature };
            salinityBc.AddPoint(0);
            salinityBc.PointDepthLayerDefinitions[0] =
                new VerticalProfileDefinition(VerticalProfileType.PercentageFromSurface, 100, 30, 5);
            var salinityData1 = salinityBc.GetDataAtPoint(0);
            salinityData1.Arguments[0].Unit = new Unit("-", "-");
            var times1 = new[]
                {
                    new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 2, 0, 0, 0), new DateTime(2014, 1, 3, 0, 0, 0)
                };
            salinityData1[times1[0]] = new[] {20, 10, 5};
            salinityData1[times1[1]] = new[] { 21, 10.5, 5 };
            salinityData1[times1[2]] = new[] { 22, 11, 5.5 };
            
            salinityBc.AddPoint(2);
            salinityBc.PointDepthLayerDefinitions[1] =
                new VerticalProfileDefinition(VerticalProfileType.TopBottom);
            var salinityData2 = salinityBc.GetDataAtPoint(2);
            salinityData2.Arguments[0].Unit = new Unit("-", "-");
            var times2 = new[]
                {
                    new DateTime(2014, 1, 1, 10, 0, 0), new DateTime(2014, 1, 2, 10, 10, 0), new DateTime(2014, 1, 3, 10, 10, 10)
                };
            salinityData2[times2[0]] = new[] { 20, 5 };
            salinityData2[times2[1]] = new[] { 21, 5 };
            salinityData2[times2[2]] = new[] { 22, 5.5 };

            boundaryConditionSet.BoundaryConditions.Add(salinityBc);

            const string filePath = "SalinityTimeSeries.bc";

            var writer = new BcFile {MultiFileMode = BcFile.WriteMode.SingleFile};
            writer.Write(new[] {boundaryConditionSet}, filePath, new BcFileFlowBoundaryDataBuilder());

            boundaryConditionSet.BoundaryConditions.Clear();

            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks);

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var importedBc = boundaryConditionSet.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;

            Assert.IsNotNull(importedBc);
            Assert.AreEqual(new[] { 0, 2 }, importedBc.DataPointIndices);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, importedBc.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.Salinity, importedBc.FlowQuantity);
            Assert.AreEqual(VerticalProfileType.PercentageFromSurface, importedBc.GetDepthLayerDefinitionAtPoint(0).Type);
            Assert.AreEqual(new[] {100, 30, 5}, importedBc.GetDepthLayerDefinitionAtPoint(0).PointDepths);
            Assert.AreEqual(new[] {times1[0], times1[1], times1[2]}, importedBc.GetDataAtPoint(0).Arguments[0].Values);
            Assert.AreEqual(new[] {20, 21, 22}, importedBc.GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] {10, 10.5, 11}, importedBc.GetDataAtPoint(0).Components[1].Values);
            Assert.AreEqual(new[] {5, 5, 5.5}, importedBc.GetDataAtPoint(0).Components[2].Values);
            Assert.AreEqual(VerticalProfileType.TopBottom, importedBc.GetDepthLayerDefinitionAtPoint(2).Type);
            Assert.AreEqual(new[] { times2[0], times2[1], times2[2] }, importedBc.GetDataAtPoint(2).Arguments[0].Values);
            Assert.AreEqual(new[] {20, 21, 22}, importedBc.GetDataAtPoint(2).Components[0].Values);
            Assert.AreEqual(new[] {5, 5, 5.5}, importedBc.GetDataAtPoint(2).Components[1].Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadVectorVelocityTimeSeriesWithVerticalProfile()
        {
            var feature = new Feature2D
                {
                    Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)}),
                    Name = "pli1"
                };

            var boundaryConditionSet = new BoundaryConditionSet {Feature = feature};

            var velocityBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.VelocityVector,
                                                       BoundaryConditionDataType.TimeSeries) {Feature = feature};
            velocityBc.AddPoint(0);
            velocityBc.PointDepthLayerDefinitions[0] =
                new VerticalProfileDefinition(VerticalProfileType.PercentageFromSurface, 100, 30, 5);

            var velocityData1 = velocityBc.GetDataAtPoint(0);
            velocityData1.Arguments[0].Unit = new Unit("-", "-");
            velocityData1[new DateTime(2014, 1, 1, 0, 0, 0)] = new[] {0.2, -0.3, 0.18, -0.24, 0.14, -0.1};
            velocityData1[new DateTime(2014, 1, 1, 0, 10, 0)] = new[] {0.22, -0.32, 0.2, -0.26, 0.16, -0.12};
            velocityData1[new DateTime(2014, 1, 1, 0, 20, 0)] = new[] {0.26, -0.36, 0.222, -0.3, 0.2, -0.16};

            velocityBc.AddPoint(2);
            velocityBc.PointDepthLayerDefinitions[1] =
                new VerticalProfileDefinition(VerticalProfileType.TopBottom);
            var velocityData2 = velocityBc.GetDataAtPoint(2);
            velocityData2.Arguments[0].Unit = new Unit("-", "-");
            velocityData2[new DateTime(2014, 1, 1, 0, 0, 0)] = new[] {-0.28, 0.38, -0.2, 0.26};

            boundaryConditionSet.BoundaryConditions.Add(velocityBc);

            const string filePath = "VelocityVectorSeries.bc";

            var writer = new BcFile {MultiFileMode = BcFile.WriteMode.SingleFile};
            writer.Write(new[] {boundaryConditionSet}, filePath, new BcFileFlowBoundaryDataBuilder());

            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            boundaryConditionSet.BoundaryConditions.Clear();

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks);

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var importedBc = boundaryConditionSet.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;

            Assert.IsNotNull(importedBc);
            Assert.AreEqual(new[] {0, 2}, importedBc.DataPointIndices);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, importedBc.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.VelocityVector, importedBc.FlowQuantity);
            Assert.AreEqual(VerticalProfileType.PercentageFromSurface, importedBc.GetDepthLayerDefinitionAtPoint(0).Type);
            Assert.AreEqual(new[] {100, 30, 5}, importedBc.GetDepthLayerDefinitionAtPoint(0).PointDepths);
            Assert.AreEqual(
                new[]
                    {
                        new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 1, 0, 10, 0),
                        new DateTime(2014, 1, 1, 0, 20, 0)
                    }, importedBc.GetDataAtPoint(0).Arguments[0].Values);

            Assert.AreEqual(new[] {0.2, 0.22, 0.26}, importedBc.GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] {-0.3, -0.32, -0.36}, importedBc.GetDataAtPoint(0).Components[1].Values);
            Assert.AreEqual(new[] {0.18, 0.2, 0.222}, importedBc.GetDataAtPoint(0).Components[2].Values);
            Assert.AreEqual(new[] {-0.24, -0.26, -0.3}, importedBc.GetDataAtPoint(0).Components[3].Values);
            Assert.AreEqual(new[] {0.14, 0.16, 0.2}, importedBc.GetDataAtPoint(0).Components[4].Values);
            Assert.AreEqual(new[] {-0.1, -0.12, -0.16}, importedBc.GetDataAtPoint(0).Components[5].Values);

            Assert.AreEqual(VerticalProfileType.TopBottom, importedBc.GetDepthLayerDefinitionAtPoint(2).Type);
            Assert.AreEqual(new[] {new DateTime(2014, 1, 1, 0, 0, 0)}, importedBc.GetDataAtPoint(2).Arguments[0].Values);
            Assert.AreEqual(new[] {-0.28}, importedBc.GetDataAtPoint(2).Components[0].Values);
            Assert.AreEqual(new[] {0.38}, importedBc.GetDataAtPoint(2).Components[1].Values);
            Assert.AreEqual(new[] {-0.2}, importedBc.GetDataAtPoint(2).Components[2].Values);
            Assert.AreEqual(new[] {0.26}, importedBc.GetDataAtPoint(2).Components[3].Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadVectorVelocityHarmonicCorrection()
        {
            var feature = new Feature2D
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)}),
                Name = "pli1"
            };

            var boundaryConditionSet = new BoundaryConditionSet {Feature = feature};

            var velocityBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.VelocityVector,
                BoundaryConditionDataType.HarmonicCorrection) {Feature = feature};
            velocityBc.AddPoint(0);

            var velocityData1 = velocityBc.GetDataAtPoint(0);
            velocityData1[20.0] = new[] {0.9, 88.5, 1.01, -2, -0.95, 20.0, 0.99, 2};
            velocityData1[100.0] = new[] {0.95, 6, 1.02, 0.1, -0.85, 691, 0.87, -20};
            velocityData1[175.000] = new[] {0.999, 276, 1.05, 3.5, -0.91, 127, 0.99, 10};

            velocityBc.AddPoint(2);
            var velocityData2 = velocityBc.GetDataAtPoint(2);
            velocityData2[100.0] = new[] {0.94, 30, 1.01, 1.5, -0.89, 391, 0.97, -10};

            boundaryConditionSet.BoundaryConditions.Add(velocityBc);

            const string filePath = "VelocityVectorHarmonicCorrection.bc";
            const string corrFilePath = "VelocityVectorHarmonicCorrectioncorr.bc";

            var boundaryDataBuilder = new BcFileFlowBoundaryDataBuilder();
            var writer = new BcFile {MultiFileMode = BcFile.WriteMode.SingleFile, CorrectionFile = false};
            writer.Write(new[] {boundaryConditionSet}, filePath, boundaryDataBuilder);
            writer.CorrectionFile = true;
            writer.Write(new[] {boundaryConditionSet}, corrFilePath, boundaryDataBuilder);

            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            boundaryConditionSet.BoundaryConditions.Clear();

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] {boundaryConditionSet}, dataBlocks);

            var correctionDataBlocks = fileReader.Read(corrFilePath);

            builder.InsertBoundaryData(new[] {boundaryConditionSet}, correctionDataBlocks);

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var importedBc = boundaryConditionSet.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;

            Assert.IsNotNull(importedBc);
            Assert.AreEqual(new[] {0, 2}, importedBc.DataPointIndices);
            Assert.AreEqual(BoundaryConditionDataType.HarmonicCorrection, importedBc.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.VelocityVector, importedBc.FlowQuantity);

            ListTestUtils.AssertAreEqual(new[] {20.0, 100, 175},
                importedBc.GetDataAtPoint(0).Arguments[0].GetValues<double>(), 0.000000001);

            Assert.AreEqual(new[] {0.9, 0.95, 0.999}, importedBc.GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] {88.5, 6, 276}, importedBc.GetDataAtPoint(0).Components[1].Values);
            Assert.AreEqual(new[] {1.01, 1.02, 1.05}, importedBc.GetDataAtPoint(0).Components[2].Values);
            Assert.AreEqual(new[] {-2, 0.1, 3.5}, importedBc.GetDataAtPoint(0).Components[3].Values);
            Assert.AreEqual(new[] {-0.95, -0.85, -0.91}, importedBc.GetDataAtPoint(0).Components[4].Values);
            Assert.AreEqual(new[] {20, 691, 127}, importedBc.GetDataAtPoint(0).Components[5].Values);
            Assert.AreEqual(new[] {0.99, 0.87, 0.99}, importedBc.GetDataAtPoint(0).Components[6].Values);
            Assert.AreEqual(new[] {2, -20, 10}, importedBc.GetDataAtPoint(0).Components[7].Values);

            ListTestUtils.AssertAreEqual(new[] {100.0}, importedBc.GetDataAtPoint(2).Arguments[0].GetValues<double>(),
                0.000000001);
            Assert.AreEqual(new[] {0.94}, importedBc.GetDataAtPoint(2).Components[0].Values);
            Assert.AreEqual(new[] {30}, importedBc.GetDataAtPoint(2).Components[1].Values);
            Assert.AreEqual(new[] {1.01}, importedBc.GetDataAtPoint(2).Components[2].Values);
            Assert.AreEqual(new[] {1.5}, importedBc.GetDataAtPoint(2).Components[3].Values);
            Assert.AreEqual(new[] {-0.89}, importedBc.GetDataAtPoint(2).Components[4].Values);
            Assert.AreEqual(new[] {391}, importedBc.GetDataAtPoint(2).Components[5].Values);
            Assert.AreEqual(new[] {0.97}, importedBc.GetDataAtPoint(2).Components[6].Values);
            Assert.AreEqual(new[] {-10}, importedBc.GetDataAtPoint(2).Components[7].Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void VerifyDischargeBoundariesUseSupportPoint()
        {
            var feature = new Feature2D
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }),
                Name = "pli1"
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };

            var startTime = new DateTime(2014, 1, 1);
            var stopTime = new DateTime(2014, 1, 2);

            var dischargeBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                                                         BoundaryConditionDataType.TimeSeries) { Feature = feature };

            dischargeBc.AddPoint(0);
            var dischargeTimeSeries = dischargeBc.GetDataAtPoint(0);
            dischargeTimeSeries[startTime] = 10.1;
            dischargeTimeSeries[stopTime] = 20.2;

            boundaryConditionSet.BoundaryConditions.Add(dischargeBc);

            var blockData = new BcFileFlowBoundaryDataBuilder().CreateBlockData(dischargeBc,
                boundaryConditionSet.SupportPointNames, startTime).ToList();

            Assert.AreEqual(1, blockData.Count);
            Assert.AreEqual("pli1_0001", blockData[0].SupportPoint);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadDischargeBoundary()
        {
            var feature = new Feature2D
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }),
                Name = "pli1"
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };

            var startTime = new DateTime(2014, 1, 1);
            var stopTime = new DateTime(2014, 1, 2);

            var dischargeBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                                                         BoundaryConditionDataType.TimeSeries) { Feature = feature };

            dischargeBc.AddPoint(0);
            var dischargeTimeSeries = dischargeBc.GetDataAtPoint(0);
            dischargeTimeSeries[startTime] = 10.1;
            dischargeTimeSeries[stopTime] = 20.2;

            boundaryConditionSet.BoundaryConditions.Add(dischargeBc);

            const string filePath = "Discharge.bc";

            var writer = new BcFile { MultiFileMode = BcFile.WriteMode.SingleFile };
            writer.Write(new[] { boundaryConditionSet }, filePath, new BcFileFlowBoundaryDataBuilder());

            boundaryConditionSet.BoundaryConditions.Clear();

            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks);

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var importedBc = boundaryConditionSet.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;

            Assert.IsNotNull(importedBc);
            Assert.AreEqual(FlowBoundaryQuantityType.Discharge, importedBc.FlowQuantity);
            Assert.IsTrue(importedBc.IsHorizontallyUniform);
            Assert.AreEqual(new[] {startTime, stopTime},
                importedBc.GetDataAtPoint(0).Arguments[0].GetValues<DateTime>().ToList());
            Assert.AreEqual(new[] {10.1, 20.2}, importedBc.GetDataAtPoint(0).Components[0].GetValues<double>().ToList());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadThreeSuperposedWaterLevels()
        {
            var feature = new Feature2D
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }),
                Name = "pli1"
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };

            var startTime = new DateTime(2014, 1, 1);
            var stopTime = new DateTime(2014, 1, 2);

            var waterLevelBc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                         BoundaryConditionDataType.TimeSeries) {Feature = feature};
            waterLevelBc1.AddPoint(0);
            var timeSeries1 = waterLevelBc1.GetDataAtPoint(0);
            timeSeries1[startTime] = 0.9;
            timeSeries1[stopTime] = 1.1;
            waterLevelBc1.AddPoint(1);
            var timeSeries2 = waterLevelBc1.GetDataAtPoint(1);
            timeSeries2[startTime] = 0.95;
            timeSeries2[stopTime] = 1.05;

            var waterLevelBc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                         BoundaryConditionDataType.TimeSeries) { Feature = feature };
            waterLevelBc2.AddPoint(0);
            var timeSeries3 = waterLevelBc2.GetDataAtPoint(0);
            timeSeries3[startTime] = 1.0;
            timeSeries3[stopTime] = 1.15;
            waterLevelBc2.AddPoint(2);
            var timeSeries4 = waterLevelBc2.GetDataAtPoint(2);
            timeSeries4[startTime] = 1.05;
            timeSeries4[stopTime] = 1.2;

            var waterLevelBc3 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                         BoundaryConditionDataType.TimeSeries) { Feature = feature };
            waterLevelBc3.AddPoint(1);
            var timeSeries5 = waterLevelBc3.GetDataAtPoint(1);
            timeSeries5[startTime] = 0.95;
            timeSeries5[stopTime] = 1.05;
            waterLevelBc3.AddPoint(2);
            var timeSeries6 = waterLevelBc3.GetDataAtPoint(2);
            timeSeries6[startTime] = 0.9;
            timeSeries6[stopTime] = 1.1;

            boundaryConditionSet.BoundaryConditions.AddRange(new[] {waterLevelBc1, waterLevelBc2, waterLevelBc3});

            const string filePath = "ThreeSuperposedWaterLevels.bc";

            var writer = new BcFile {MultiFileMode = BcFile.WriteMode.SingleFile};
            writer.Write(new[] {boundaryConditionSet}, filePath, new BcFileFlowBoundaryDataBuilder());

            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            boundaryConditionSet.BoundaryConditions.Clear();

            var builder = new BcFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks);

            Assert.AreEqual(3, boundaryConditionSet.BoundaryConditions.Count);
            Assert.AreEqual(new[] {0, 1}, boundaryConditionSet.BoundaryConditions[0].DataPointIndices);
            Assert.AreEqual(new[] {startTime, stopTime},
                            boundaryConditionSet.BoundaryConditions[0].GetDataAtPoint(0).Arguments[0].Values);
            Assert.AreEqual(new[] {0.9, 1.1},
                            boundaryConditionSet.BoundaryConditions[0].GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] { startTime, stopTime },
                boundaryConditionSet.BoundaryConditions[0].GetDataAtPoint(1).Arguments[0].Values);
            Assert.AreEqual(new[] { 0.95, 1.05 },
                            boundaryConditionSet.BoundaryConditions[0].GetDataAtPoint(1).Components[0].Values);

            Assert.AreEqual(new[] { 0, 2 }, boundaryConditionSet.BoundaryConditions[1].DataPointIndices);
            Assert.AreEqual(new[] { startTime, stopTime },
                            boundaryConditionSet.BoundaryConditions[1].GetDataAtPoint(0).Arguments[0].Values);
            Assert.AreEqual(new[] { 1.0, 1.15 },
                            boundaryConditionSet.BoundaryConditions[1].GetDataAtPoint(0).Components[0].Values);
            Assert.AreEqual(new[] { startTime, stopTime },
                boundaryConditionSet.BoundaryConditions[1].GetDataAtPoint(2).Arguments[0].Values);
            Assert.AreEqual(new[] { 1.05, 1.2 },
                            boundaryConditionSet.BoundaryConditions[1].GetDataAtPoint(2).Components[0].Values);

            Assert.AreEqual(new[] { 1, 2 }, boundaryConditionSet.BoundaryConditions[2].DataPointIndices);
            Assert.AreEqual(new[] { startTime, stopTime },
                            boundaryConditionSet.BoundaryConditions[2].GetDataAtPoint(1).Arguments[0].Values);
            Assert.AreEqual(new[] { 0.95, 1.05 },
                            boundaryConditionSet.BoundaryConditions[2].GetDataAtPoint(1).Components[0].Values);
            Assert.AreEqual(new[] { startTime, stopTime },
                boundaryConditionSet.BoundaryConditions[2].GetDataAtPoint(2).Arguments[0].Values);
            Assert.AreEqual(new[] { 0.9, 1.1 },
                            boundaryConditionSet.BoundaryConditions[2].GetDataAtPoint(2).Components[0].Values);

        }
        
        
        [Test]
        [TestCaseSource(nameof(BcBlockDataWithTimeZoneOffset))]
        public void GivenABoundaryConditionSetAndABcBlockDataWithTimeZoneOffset_WhenInsertBoundaryDataIsCalled_ThenTimeZoneIsExpectedTimeZone(string timeUnit, TimeSpan expectedTimeZone)
        {
            // Given
            const string boundaryName = "tfl_01_0001";
            const string valUnit = "m";
            const string quantity = "waterlevelbnd";
            var builder = new BcFileFlowBoundaryDataBuilder();

            BcBlockData dataBlock = CreateBcBlockData(boundaryName, "support", timeUnit, valUnit, quantity);
            BoundaryConditionSet boundaryConditionSet = CreateBoundaryConditionSetWithFlowBoundaryCondition(boundaryName, "sand");

            // When
            builder.InsertBoundaryData(new[]
            {
                boundaryConditionSet
            }, dataBlock);

            // Then
            var flowBoundaryCondition = boundaryConditionSet.BoundaryConditions[1] as FlowBoundaryCondition;
            Assert.That(flowBoundaryCondition, Is.Not.Null);
            Assert.That(flowBoundaryCondition.TimeZone, Is.EqualTo(expectedTimeZone));
        }
        
        private static BcBlockData CreateBcBlockData(string boundaryName,
                                                     string supportPointName,
                                                     string timeUnit,
                                                     string valUnit,
                                                     string quantity)
        {
            var timeQuantity = new BcQuantityData
            {
                Quantity = "time",
                Unit = timeUnit
            };
            timeQuantity.Values.AddRange(new[]
            {
                "0",
                "43200",
                "86400"
            });

            var fractionQuantity = new BcQuantityData
            {
                Quantity = quantity,
                Unit = valUnit
            };
            fractionQuantity.Values.AddRange(new[]
            {
                "1",
                "2",
                "3"
            });

            var dataBlock = new BcBlockData
            {
                SupportPoint = supportPointName,
                FunctionType = "timeseries",
                LineNumber = 1,
                TimeInterpolationType = "linear",
                VerticalInterpolationType = "linear",
                VerticalPositionType = "single"
            };
            dataBlock.SupportPoint = $"{boundaryName}_0001";

            dataBlock.Quantities.AddRange(new[]
            {
                timeQuantity,
                fractionQuantity
            });

            return dataBlock;
        }
        
        private static BoundaryConditionSet CreateBoundaryConditionSetWithFlowBoundaryCondition(string boundaryName, string sedimentFractionName)
        {
            var feature = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 0),
                    new Coordinate(2, 0)
                }),
                Name = boundaryName
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };

            var boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration,
                                                              BoundaryConditionDataType.TimeSeries)
            {
                SedimentFractionName = sedimentFractionName,
                Feature = feature
            };

            boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);

            return boundaryConditionSet;
        }
        
        public static IEnumerable<TestCaseData> BcBlockDataWithTimeZoneOffset()
        {
            yield return new TestCaseData("seconds since 1992-09-01 18:00:00 +04:40", new TimeSpan(4,40,0));
            yield return new TestCaseData("seconds since 1992-09-01 18:00:00 -10:12", new TimeSpan(-10,-12,0));
        }
    }
}

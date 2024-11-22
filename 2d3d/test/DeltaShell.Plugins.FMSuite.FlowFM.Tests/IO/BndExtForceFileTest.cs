﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class BndExtForceFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenWaterFlowFMModelDefinitionWithTwoBoundaries_WhenWriteReadingWithBndExtForceFile_ThenTheTwoBoundariesArePreserved()
        {
            // Given
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            // When
            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            // Then
            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadWaterLevelTimeSeries()
        {
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var firstBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                   BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[0] };
            firstBoundaryCondition.AddPoint(0);
            firstBoundaryCondition.AddPoint(5);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Add(firstBoundaryCondition);

            var startTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;

            IFunction data = firstBoundaryCondition.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = firstBoundaryCondition.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            var secondBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                    BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.Add(secondBoundaryCondition);
            secondBoundaryCondition.AddPoint(9);

            data = secondBoundaryCondition.GetDataAtPoint(9);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 10);

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[1].BoundaryConditions.Count);

            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[0]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFlowBoundaryConditionWithThatcherHarlemanTimeLag_WhenWriteReadingWithBndExtForceFile_ThenThatcherHarlemanTimeLagIsPreserved()
        {
            // Given
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var thatcherHarlemanTimeLag = new TimeSpan(0, 0, 40);
            var firstBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                   BoundaryConditionDataType.TimeSeries)
            {
                Feature = modelDefinition.Boundaries[0],
                ThatcherHarlemanTimeLag = thatcherHarlemanTimeLag
            };
            firstBoundaryCondition.AddPoint(0);
            firstBoundaryCondition.AddPoint(5);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Add(firstBoundaryCondition);

            var startTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;

            IFunction data = firstBoundaryCondition.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = firstBoundaryCondition.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            var secondBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                    BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.Add(secondBoundaryCondition);
            secondBoundaryCondition.AddPoint(9);

            data = secondBoundaryCondition.GetDataAtPoint(9);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 10);

            // When
            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            // Then
            Assert.AreEqual(thatcherHarlemanTimeLag,
                            ((FlowBoundaryCondition)newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0])
                            .ThatcherHarlemanTimeLag);
            Assert.AreEqual(
                ((FlowBoundaryCondition)modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0])
                .ThatcherHarlemanTimeLag,
                ((FlowBoundaryCondition)newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0])
                .ThatcherHarlemanTimeLag);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadWaterLevelQhBoundaryCondition()
        {
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var firstBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                   BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[0] };
            firstBoundaryCondition.AddPoint(1);
            firstBoundaryCondition.AddPoint(5);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Add(firstBoundaryCondition);

            var startTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;

            IFunction data = firstBoundaryCondition.GetDataAtPoint(1);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = firstBoundaryCondition.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            var secondBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                    BoundaryConditionDataType.Qh)
            { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.Add(secondBoundaryCondition);
            secondBoundaryCondition.AddPoint(0);

            data = secondBoundaryCondition.GetDataAtPoint(0);
            data[10.0] = new[]
            {
                0.5
            };
            data[15.0] = new[]
            {
                0.75
            };
            data[20.0] = new[]
            {
                1.25
            };
            data[25.0] = new[]
            {
                2.5
            };

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[1].BoundaryConditions.Count);

            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[0]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadDoubleWaterLevelTimeSeries()
        {
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var firstBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                   BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[0] };
            firstBoundaryCondition.AddPoint(0);
            firstBoundaryCondition.AddPoint(5);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Add(firstBoundaryCondition);

            var startTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;

            IFunction data = firstBoundaryCondition.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = firstBoundaryCondition.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            var secondBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                    BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[1] };

            var thirdBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                   BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[]
            {
                secondBoundaryCondition,
                thirdBoundaryCondition
            });

            secondBoundaryCondition.AddPoint(9);
            thirdBoundaryCondition.AddPoint(2);
            thirdBoundaryCondition.AddPoint(9);

            data = secondBoundaryCondition.GetDataAtPoint(9);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 7);

            data = thirdBoundaryCondition.GetDataAtPoint(2);
            FillTimeSeries(data, i => 0.55 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 10);

            data = thirdBoundaryCondition.GetDataAtPoint(9);
            FillTimeSeries(data, i => 0.45 * Math.Sin(0.4 * Math.PI * i), startTime, stopTime, 12);

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets[1].BoundaryConditions.Count);

            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[1],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[1]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadWaterLevelHarmonicAndAstronomic()
        {
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.Harmonics)
            { Feature = modelDefinition.Boundaries[0] };
            bc1.AddPoint(0);
            bc1.AddPoint(8);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                bc1
            });

            IFunction data = bc1.GetDataAtPoint(0);
            data[15.5] = new[]
            {
                0.6,
                120
            };
            data[4.78] = new[]
            {
                0.1,
                34
            };

            data = bc1.GetDataAtPoint(8);
            data[25.5] = new[]
            {
                0.16,
                20
            };
            data[14.78] = new[]
            {
                0.11,
                3.4
            };

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.AstroComponents)
            { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[]
            {
                bc2
            });

            bc2.AddPoint(2);
            bc2.AddPoint(9);

            data = bc2.GetDataAtPoint(2);
            data["A0"] = new[]
            {
                0.5,
                0
            };
            data["M1"] = new[]
            {
                0.15,
                120
            };
            data["P1"] = new[]
            {
                0.6,
                250
            };

            data = bc2.GetDataAtPoint(9);
            data["A0"] = new[]
            {
                0.6,
                0
            };
            data["M1"] = new[]
            {
                0.16,
                130
            };
            data["P1"] = new[]
            {
                0.61,
                260
            };

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[1].BoundaryConditions.Count);

            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[0]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadWaterLevelTimeSeriesPlusHarmonic()
        {
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[0] };
            bc1.AddPoint(0);
            bc1.AddPoint(5);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.Harmonics)
            { Feature = modelDefinition.Boundaries[0] };
            bc2.AddPoint(0);
            bc2.AddPoint(8);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                bc1,
                bc2
            });

            var startTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;

            IFunction data = bc1.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = bc1.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            data = bc2.GetDataAtPoint(0);
            data[15.5] = new[]
            {
                0.6,
                120
            };
            data[4.78] = new[]
            {
                0.1,
                34
            };

            data = bc2.GetDataAtPoint(8);
            data[25.5] = new[]
            {
                0.16,
                20
            };
            data[14.78] = new[]
            {
                0.11,
                3.4
            };

            var bc3 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[1] };

            var bc4 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.AstroComponents)
            { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[]
            {
                bc3,
                bc4
            });

            bc3.AddPoint(9);
            bc4.AddPoint(2);
            bc4.AddPoint(9);

            data = bc3.GetDataAtPoint(9);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 7);

            data = bc4.GetDataAtPoint(2);
            data["A0"] = new[]
            {
                0.5,
                0
            };
            data["M1"] = new[]
            {
                0.15,
                120
            };
            data["P1"] = new[]
            {
                0.6,
                250
            };

            data = bc4.GetDataAtPoint(9);
            data["A0"] = new[]
            {
                0.6,
                0
            };
            data["M1"] = new[]
            {
                0.16,
                130
            };
            data["P1"] = new[]
            {
                0.61,
                260
            };

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets[1].BoundaryConditions.Count);

            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[1],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[1]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[1],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[1]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadWaterLevelHarmonicAndAstronomicCorrections()
        {
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.HarmonicCorrection)
            { Feature = modelDefinition.Boundaries[0] };
            bc1.AddPoint(0);
            bc1.AddPoint(8);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                bc1
            });

            IFunction data = bc1.GetDataAtPoint(0);
            data[15.5] = new[]
            {
                0.6,
                120,
                0.99,
                10
            };
            data[4.78] = new[]
            {
                0.1,
                34,
                1.01,
                11
            };

            data = bc1.GetDataAtPoint(8);
            data[25.5] = new[]
            {
                0.16,
                20,
                0.98,
                11
            };
            data[14.78] = new[]
            {
                0.11,
                3.4,
                0.87,
                20
            };

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.AstroCorrection)
            { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[]
            {
                bc2
            });

            bc2.AddPoint(2);
            bc2.AddPoint(9);

            data = bc2.GetDataAtPoint(2);
            data["A0"] = new[]
            {
                0.5,
                0,
                0,
                0
            };
            data["M1"] = new[]
            {
                0.15,
                120,
                1.11,
                22
            };
            data["P1"] = new[]
            {
                0.6,
                250,
                0.98,
                12
            };

            data = bc2.GetDataAtPoint(9);
            data["A0"] = new[]
            {
                0.6,
                0,
                0,
                0
            };
            data["M1"] = new[]
            {
                0.16,
                130,
                1.22,
                11
            };
            data["P1"] = new[]
            {
                0.61,
                260,
                0.89,
                34
            };

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);
            Assert.AreEqual(1, newModelDefinition.BoundaryConditionSets[1].BoundaryConditions.Count);

            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[0]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadWaterLevelHarmonicAndAstronomicCorrectionsAndSurge()
        {
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.HarmonicCorrection)
            { Feature = modelDefinition.Boundaries[0] };
            bc1.AddPoint(0);
            bc1.AddPoint(8);

            IFunction data = bc1.GetDataAtPoint(0);
            data[15.5] = new[]
            {
                0.6,
                120,
                0.99,
                10
            };
            data[4.78] = new[]
            {
                0.1,
                34,
                1.01,
                11
            };

            data = bc1.GetDataAtPoint(8);
            data[25.5] = new[]
            {
                0.16,
                20,
                0.98,
                11
            };
            data[14.78] = new[]
            {
                0.11,
                3.4,
                0.87,
                20
            };

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[0] };
            var startTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;

            bc2.AddPoint(0);
            data = bc2.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 7);

            bc2.AddPoint(5);
            data = bc2.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.65 * Math.Sin(0.4 * Math.PI * i), startTime, stopTime, 20);

            var bc3 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[0] };
            bc3.AddPoint(7);
            data = bc3.GetDataAtPoint(7);
            FillTimeSeries(data, i => 0.87 / ((i * i) + 1), startTime, stopTime, 10);

            var bc4 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                BoundaryConditionDataType.AstroCorrection)
            { Feature = modelDefinition.Boundaries[0] };
            bc4.AddPoint(7);
            data = bc4.GetDataAtPoint(7);
            data["A0"] = new[]
            {
                0.5,
                0,
                0,
                0
            };
            data["M1"] = new[]
            {
                0.15,
                120,
                1.11,
                22
            };
            data["P1"] = new[]
            {
                0.6,
                250,
                0.98,
                12
            };

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                bc1,
                bc2,
                bc3,
                bc4
            });

            var bc5 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.AstroCorrection)
            { Feature = modelDefinition.Boundaries[1] };

            bc5.AddPoint(2);
            data = bc5.GetDataAtPoint(2);
            data["A0"] = new[]
            {
                0.5,
                0,
                0,
                0
            };
            data["M1"] = new[]
            {
                0.15,
                120,
                1.11,
                22
            };
            data["P1"] = new[]
            {
                0.6,
                250,
                0.98,
                12
            };

            bc5.AddPoint(9);
            data = bc5.GetDataAtPoint(9);
            data["A0"] = new[]
            {
                0.6,
                0,
                0,
                0
            };
            data["M1"] = new[]
            {
                0.16,
                130,
                1.22,
                11
            };
            data["P1"] = new[]
            {
                0.61,
                260,
                0.89,
                34
            };

            var bc6 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                BoundaryConditionDataType.HarmonicCorrection)
            { Feature = modelDefinition.Boundaries[1] };

            bc6.AddPoint(0);
            data = bc6.GetDataAtPoint(0);
            data[15.5] = new[]
            {
                20,
                120,
                0.99,
                10
            };
            data[4.78] = new[]
            {
                21,
                34,
                1.01,
                11
            };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[]
            {
                bc5,
                bc6
            });

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", "testbnd.ext", newModelDefinition);

            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(4, newModelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets[1].BoundaryConditions.Count);

            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[1],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[1]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[2],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[2]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[0].BoundaryConditions[3],
                                      newModelDefinition.BoundaryConditionSets[0].BoundaryConditions[3]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[0],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[0]);
            CompareBoundaryConditions(modelDefinition.BoundaryConditionSets[1].BoundaryConditions[1],
                                      newModelDefinition.BoundaryConditionSets[1].BoundaryConditions[1]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteMorphBoundaryCondition()
        {
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithTwoBoundaries();
            modelDefinition.ModelName = "MyModelName";

            var morphologyBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
                                                                        BoundaryConditionDataType.TimeSeries)
            { Feature = modelDefinition.Boundaries[0] };

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                morphologyBoundaryCondition
            });

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", "testbnd.ext", modelDefinition);
            Assert.IsTrue(File.Exists(modelDefinition.ModelName + BcmFile.Extension));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteSedimentConcentration()
        {
            //Note, for the moment we assume these type of sediments are compatible with waterflowfm.
            string testFilePath = TestHelper.GetTestFilePath(@"simplebox/simplebox.mdu");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(testFilePath);

            model.Name = "newname";
            model.ModelDefinition.UseMorphologySediment = true;

            var sedFrac = new SedimentFraction { Name = "frac1" };
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

            var fbcFactory = new FlowBoundaryConditionFactory { Model = model };
            IBoundaryCondition bCond = fbcFactory.CreateBoundaryCondition(boundary,
                                                                          sedFrac.Name,
                                                                          BoundaryConditionDataType.TimeSeries,
                                                                          FlowBoundaryQuantityType.SedimentConcentration.GetDescription());

            model.BoundaryConditionSets[1].BoundaryConditions.Add(bCond);

            bCond.AddPoint(0);
            IFunction dataAtZero = bCond.GetDataAtPoint(0);
            dataAtZero[model.StartTime] = new[]
            {
                36.0
            };
            dataAtZero[model.StartTime.AddMinutes(2)] = new[]
            {
                18.0
            };

            var bndExtForceFile = new BndExtForceFile();
            string extFileName = model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).GetValueAsString();
            if (string.IsNullOrEmpty(extFileName))
            {
                extFileName = model.ModelDefinition.ModelName + FileConstants.ExternalForcingFileExtension;
            }

            bndExtForceFile.Write(extFileName, extFileName, model.ModelDefinition);

            Assert.IsTrue(File.Exists(extFileName));
            string fileText = File.ReadAllText(extFileName);
            Assert.That(fileText, Does.Contain("[" + BndExtForceFileConstants.BoundaryBlockKey + "]")
                                    .And.Contains(BndExtForceFileConstants.QuantityKey + "=" + ExtForceQuantNames.ConcentrationAtBound + "frac1")
                                    .And.Contains(BndExtForceFileConstants.LocationFileKey + "=" + "L1.pli")
                                    .And.Contains(BndExtForceFileConstants.ForcingFileKey + "=" + "frac1.bc"));

            //check to see if only 2 boundaries are written
            Assert.AreEqual(2, new Regex(Regex.Escape("[" + BndExtForceFileConstants.BoundaryBlockKey + "]")).Matches(fileText).Count);

            Assert.IsTrue(File.Exists("frac1.bc"));

            fileText = File.ReadAllText("frac1.bc");
            Assert.That(fileText, Does.Contain(ExtForceQuantNames.ConcentrationAtBound + "frac1")
                                    .And.Contains("bound_0001")
                                    .And.Contains(BcFile.BlockKey)
                                    .And.Contains(BcFile.QuantityKey)
                                    .And.Contains("36").And.Contains("18"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadSedimentConcentration()
        {
            //Note, for the moment we assume these type of sediments are compatible with waterflowfm.
            string testFilePath = TestHelper.GetTestFilePath(@"simplebox/simplebox.mdu");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(testFilePath);

            model.Name = "newname";

            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac = new SedimentFraction { Name = "frac1" };
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

            var fbcFactory = new FlowBoundaryConditionFactory { Model = model };
            IBoundaryCondition bCond = fbcFactory.CreateBoundaryCondition(boundary,
                                                                          sedFrac.Name,
                                                                          BoundaryConditionDataType.TimeSeries,
                                                                          FlowBoundaryQuantityType.SedimentConcentration.GetDescription());

            model.BoundaryConditionSets[1].BoundaryConditions.Add(bCond);

            bCond.AddPoint(0);
            IFunction dataAtZero = bCond.GetDataAtPoint(0);
            dataAtZero[model.StartTime] = new[]
            {
                36.0
            };
            dataAtZero[model.StartTime.AddMinutes(2)] = new[]
            {
                18.0
            };

            var bndExtForceFile = new BndExtForceFile();
            string extFileName = model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).GetValueAsString();
            if (string.IsNullOrEmpty(extFileName))
            {
                extFileName = model.ModelDefinition.ModelName + FileConstants.ExternalForcingFileExtension;
            }

            bndExtForceFile.Write(extFileName, extFileName, model.ModelDefinition);
            var modelDefinition = new WaterFlowFMModelDefinition("myModel");
            bndExtForceFile = new BndExtForceFile();
            bndExtForceFile.Read(extFileName, extFileName, modelDefinition);
        }

        [Test]
        public void WriteBndExtForceFileSubFilesReturnsNoItemsIfMissingName()
        {
            // Setup
            var bndExtForceFile = new BndExtForceFile();
            var firstBoundary = new Feature2D { Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray()) };
            Assert.That(firstBoundary.Name, Is.Null.Or.Empty);

            var bcSet = new BoundaryConditionSet { Feature = firstBoundary };

            var modelDefinition = new WaterFlowFMModelDefinition();
            modelDefinition.BoundaryConditionSets.Add(bcSet);
            
            // Call
            IList<IniSection> resultingItems = bndExtForceFile.WriteBndExtForceFileSubFiles(modelDefinition);

            // Assert
            Assert.IsFalse(resultingItems.Any());
        }

        [Test]
        public void WriteBndExtForceFileSubFilesReturnsItemsWhenNameIsNotMissing()
        {
            var bndExtForceFile = new BndExtForceFile();
            string testFolder = Path.GetDirectoryName(Path.GetDirectoryName(TestHelper.GetTestDataDirectory())); //This will place it under /debug
            Assert.IsNotNull(testFolder);

            string fileDirectory = Path.Combine(testFolder, "WriteBndExtForceFileSubFilesReturnsItemsWhenNameIsNotMissing");
            FileUtils.DeleteIfExists(fileDirectory);

            string filePath = Path.Combine(fileDirectory, "file.ext");
            FileUtils.DeleteIfExists(filePath);

            var firstBoundary = new Feature2D
            {
                Name = "TestName",
                Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray())
            };
            var secondBoundary = new Feature2D { Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray()) };
            Assert.That(secondBoundary.Name, Is.Null.Or.Empty);

            var bcSetOne = new BoundaryConditionSet { Feature = firstBoundary };
            var bcSetTwo = new BoundaryConditionSet { Feature = secondBoundary };

            var md = new WaterFlowFMModelDefinition();
            md.BoundaryConditionSets.AddRange(new[]
            {
                bcSetOne,
                bcSetTwo
            });

            Assert.IsFalse(File.Exists(filePath));
            bndExtForceFile.Write(filePath, filePath, md);
            Assert.IsTrue(File.Exists(filePath));

            string[] lines = File.ReadAllLines(filePath);
            Assert.IsNotNull(lines);

            //Make sure only one boundary has been added (double check from the test WriteBndExtForceFileSubFilesReturnsNoItemsIfMissingName above)
            Assert.AreEqual(1, lines.Count(l => l.Contains("[boundary]")));
            Assert.IsTrue(lines.Any(l => l.Contains("TestName.pli")));
            FileUtils.DeleteIfExists(filePath);
        }

        private static WaterFlowFMModelDefinition CreateModelDefinitionWithTwoBoundaries()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var firstBoundary = new Feature2D
            {
                Name = "Boundary1",
                Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray())
            };
            var secondBoundary = new Feature2D
            {
                Name = "Boundary2",
                Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(10.0 * i, 0)).ToArray())
            };
            modelDefinition.Boundaries.AddRange(new[]
            {
                firstBoundary,
                secondBoundary
            });
            modelDefinition.BoundaryConditionSets.AddRange(new[]
            {
                new BoundaryConditionSet {Feature = firstBoundary},
                new BoundaryConditionSet() {Feature = secondBoundary}
            });

            return modelDefinition;
        }

        private static void FillTimeSeries(IFunction function, Func<int, double> mapping, DateTime start, DateTime stop, int steps)
        {
            TimeSpan deltaT = stop - start;
            IEnumerable<DateTime> times = Enumerable.Range(0, steps).Select(i => start + new TimeSpan(i * deltaT.Ticks));
            IEnumerable<double> values = Enumerable.Range(0, steps).Select(mapping);
            FunctionHelper.SetValuesRaw(function.Arguments[0], times);
            FunctionHelper.SetValuesRaw(function.Components[0], values);
        }

        private static void CompareBoundaryConditions(IBoundaryCondition first, IBoundaryCondition second)
        {
            Assert.AreEqual(first.DataType, second.DataType, "data type");
            Assert.AreEqual(first.VariableName, second.VariableName, "quantity");
            Assert.AreEqual(first.DataPointIndices, second.DataPointIndices, "data points");

            foreach (int dataPointIndex in first.DataPointIndices)
            {
                IFunction firstFunction = first.GetDataAtPoint(dataPointIndex);
                IFunction secondFunction = second.GetDataAtPoint(dataPointIndex);

                if (firstFunction.Arguments[0].ValueType == typeof(double))
                {
                    List<double> firstList = firstFunction.Arguments[0].GetValues<double>().ToList();
                    List<double> secondList = secondFunction.Arguments[0].GetValues<double>().ToList();

                    Assert.AreEqual(firstList.Count, secondList.Count, "argument value count at " + dataPointIndex);
                    for (var i = 0; i < firstList.Count; ++i)
                    {
                        Assert.AreEqual(firstList[i], secondList[i], 1e-05,
                                        "argument value " + i + " at " + dataPointIndex);
                    }
                }

                if (firstFunction.Arguments[0].ValueType == typeof(string))
                {
                    Assert.AreEqual(firstFunction.Arguments[0].GetValues<string>().ToList(),
                                    secondFunction.Arguments[0].GetValues<string>().ToList());
                }

                var c = 0;
                foreach (IVariable component in firstFunction.Components)
                {
                    List<double> firstList = component.GetValues<double>().ToList();
                    List<double> secondList = secondFunction.Components[c++].GetValues<double>().ToList();

                    Assert.AreEqual(firstList.Count, secondList.Count,
                                    "component " + c + " value count at " + dataPointIndex);

                    for (var i = 0; i < firstList.Count; ++i)
                    {
                        Assert.AreEqual(firstList[i], secondList[i], 1e-05,
                                        "component " + c + " value " + i + " at " + dataPointIndex);
                    }
                }
            }
        }

        [TestCase("BcFiles\\MixedQuantities.ext", "BcFiles\\MixedQuantities.ext")]
        [TestCase(
            "BcFiles\\ModelBndExtForceFileAndMduInDifferentFoldersPathsRelativeToMdu\\BndExtFolder\\MixedQuantities.ext",
            "BcFiles\\ModelBndExtForceFileAndMduInDifferentFoldersPathsRelativeToMdu\\MduFolder\\EmptyMduFile.mdu")]
        [TestCase(
            "BcFiles\\ModelBndExtForceFileAndMduInDifferentFoldersPathsRelativeToBndExt\\BndExtFolder\\MixedQuantities.ext",
            "BcFiles\\ModelBndExtForceFileAndMduInDifferentFoldersPathsRelativeToBndExt\\BndExtFolder\\MixedQuantities.ext")]
        [Category(TestCategory.DataAccess)]
        public void
            GivenABndExtFileWithTwoBoundaryConditionsForOneBoundary_WhenReadingThisFile_ThenThisSpecificBoundaryShouldBeCreated(
                string bndExtFilePath, string bndExtSubFilesReferenceFilePath)
        {
            // Given
            string absoluteBndExtFilePath = TestHelper.GetTestFilePath(bndExtFilePath);
            string absoluteBndExtSubFilesReferenceFilePath =
                TestHelper.GetTestFilePath(bndExtSubFilesReferenceFilePath);

            var modelDefinition = new WaterFlowFMModelDefinition();

            // When
            var bndExtForceFile = new BndExtForceFile();
            bndExtForceFile.Read(absoluteBndExtFilePath, absoluteBndExtSubFilesReferenceFilePath, modelDefinition);

            // Then
            Assert.AreEqual(1, modelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(2, modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);

            var salinityBoundaryCondition =
                modelDefinition.BoundaryConditionSets[0].BoundaryConditions[1] as FlowBoundaryCondition;
            Assert.AreEqual(new TimeSpan(0, 0, 42), salinityBoundaryCondition.ThatcherHarlemanTimeLag);
            Assert.AreEqual(FlowBoundaryQuantityType.Salinity, salinityBoundaryCondition.FlowQuantity);
            Assert.AreEqual(2, salinityBoundaryCondition.GetDataAtPoint(0).GetValues().Count);
            Assert.AreEqual(2, salinityBoundaryCondition.GetDataAtPoint(1).GetValues().Count);

            var waterlevelBoundaryCondition =
                modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0] as FlowBoundaryCondition;
            Assert.AreEqual(TimeSpan.Zero, waterlevelBoundaryCondition.ThatcherHarlemanTimeLag);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel, waterlevelBoundaryCondition.FlowQuantity);

            CollectionAssert.AreEquivalent(new []
            {
                3,
                4
            }, waterlevelBoundaryCondition.GetDataAtPoint(0).GetValues<double>().ToArray());
            CollectionAssert.AreEquivalent(new []
            {
                5,
                6
            }, waterlevelBoundaryCondition.GetDataAtPoint(1).GetValues<double>().ToArray());
        }

        [TestCase(@"BcFiles\ModelBndExtForceFileAndMduInDifferentFoldersPathsRelativeToMdu\BndExtFolder\MixedQuantities.ext",
                  @"BcFiles\ModelBndExtForceFileAndMduInDifferentFoldersPathsRelativeToMdu\MduFolder\EmptyMduFile.mdu")]
        [TestCase(@"BcFiles\ModelBndExtForceFileAndMduInDifferentFoldersPathsRelativeToBndExt\BndExtFolder\MixedQuantities.ext",
                  @"BcFiles\ModelBndExtForceFileAndMduInDifferentFoldersPathsRelativeToBndExt\BndExtFolder\MixedQuantities.ext")]
        [Category(TestCategory.DataAccess)]
        public void GivenABndExtFileWithBoundaryConditionsAndLaterals_WhenReadingAndWritingThisFile_ThenAllSubFilesShouldBeWrittenWithRespectToParentFile(
            string bndExtFilePath, 
            string bndExtSubFilesReferenceFilePath)
        {
            // Given
            string absoluteBndExtFilePath = TestHelper.GetTestFilePath(bndExtFilePath);
            string absoluteBndExtSubFilesReferenceFilePath = TestHelper.GetTestFilePath(bndExtSubFilesReferenceFilePath);

            var modelDefinition = new WaterFlowFMModelDefinition();

            var bndExtForceFile = new BndExtForceFile();    
            bndExtForceFile.Read(absoluteBndExtFilePath, absoluteBndExtSubFilesReferenceFilePath, modelDefinition);

            // When
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                string saveBndExtFilePath = Path.Combine(tempDir, bndExtFilePath);
                string saveBndExtSubFilesReferenceFilePath = Path.Combine(tempDir, bndExtSubFilesReferenceFilePath);

                bndExtForceFile.Write(saveBndExtFilePath, saveBndExtSubFilesReferenceFilePath, modelDefinition);

                Assert.True(File.Exists(saveBndExtFilePath));

                using (var file = new StreamReader(saveBndExtFilePath))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.StartsWith("locationFile") || line.StartsWith("forcingFile") || line.StartsWith("discharge"))
                        {
                            CheckIfSubFilesMentionedInBndExtForceFileAreWrittenRelativeToThisFile(line, saveBndExtSubFilesReferenceFilePath);
                        }
                    }

                    file.Close();
                }
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenBoundaryConditionsAndLateralsInTheSameFile_WhenReadingAndWritingThisFile_ShouldBeWrittenToTheSameFile()
        {
            string bndExtFilePath = TestHelper.GetTestFilePath(@"BcFiles\BoundariesAndLateralsInSameFile\MixedQuantities.ext");

            var bndExtForceFile = new BndExtForceFile();
            var modelDefinition = new WaterFlowFMModelDefinition();

            bndExtForceFile.Read(bndExtFilePath, bndExtFilePath, modelDefinition);
            
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                string saveBndExtFilePath = Path.Combine(tempDir, "MixedQuantities.ext");
                string saveBndBcFilePath = Path.Combine(tempDir, "MixedQuantities.bc");

                bndExtForceFile.Write(saveBndExtFilePath, saveBndExtFilePath, modelDefinition);

                string[] saveBndExtFileContents = File.ReadAllLines(saveBndBcFilePath);
                Assert.That(saveBndExtFileContents, Has.One.Matches<string>(x => x.Contains("Boundary1_0001")));
                Assert.That(saveBndExtFileContents, Has.One.Matches<string>(x => x.Contains("LateralDischarge")));
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FileWithLateralDefinition_AddsTheLateralDefinitionToTheModelDefinition()
        {
            // Setup
            var extFileLines = new[]
            {
                "[lateral] = ",
                "id = some_id",
                "type = discharge",
                "locationType = 2d",
                "numCoordinates = 3",
                "xCoordinates = 1.23 2.34 3.45",
                "yCoordinates = 4.56 5.67 6.78", 
                "discharge = lateral_discharge.bc",
            };
            
            var bcFileLines = new[]
            {
                "[forcing]",
                "Name = some_id",
                "Function = timeseries",
                "TimeInterpolation = linear",
                "Quantity = time",
                "Unit = seconds since 2023-07-31 00:00:00",
                "Quantity = lateral_discharge",
                "Unit = m3/s",
                "60  1.23",
                "120 2.34",
                "180 3.45",
            };

            var bndExtForceFile = new BndExtForceFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            
            using (var temp = new TemporaryDirectory())
            {
                string extFile = temp.CreateFile("FlowFM_bnd.ext", string.Join(Environment.NewLine, extFileLines));
                temp.CreateFile("lateral_discharge.bc", string.Join(Environment.NewLine, bcFileLines));
                
                // Call
                bndExtForceFile.Read(extFile, extFile, modelDefinition);
            }
            
            // Assert
            Assert.That(modelDefinition.Laterals, Has.Count.EqualTo(1));
            Assert.That(modelDefinition.LateralFeatures, Has.Count.EqualTo(1));

            Lateral lateral = modelDefinition.Laterals.Single();
            Feature2D lateralFeature = modelDefinition.LateralFeatures.Single();
                
            Assert.That(lateral.Feature, Is.SameAs(lateralFeature));

            Assert.That(lateralFeature.Geometry, Is.EqualTo(new Polygon(new LinearRing(new[]
            {
                new Coordinate(1.23, 4.56), 
                new Coordinate(2.34, 5.67), 
                new Coordinate(3.45, 6.78), 
                new Coordinate(1.23, 4.56),
            }))));
            
            Assert.That(lateral.Data.Discharge.Type, Is.EqualTo(LateralDischargeType.TimeSeries));
            Assert.That(lateral.Data.Discharge.TimeSeries.Time.InterpolationType, Is.EqualTo(InterpolationType.Linear));
                
            var referenceDate = new DateTime(2023, 7, 31);
            Assert.That(lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(60)], Is.EqualTo(1.23));
            Assert.That(lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(120)], Is.EqualTo(2.34));
            Assert.That(lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(180)], Is.EqualTo(3.45));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_ModelDefinitionWithLaterals_WritesLateralDataToFiles()
        {
            // Setup
            var bndExtForceFile = new BndExtForceFile();
            var modelDefinition = new WaterFlowFMModelDefinition();

            var referenceDate = new DateTime(2023, 7, 31);
            var feature = new Feature2D { 
                Name = "some_id",
                Geometry = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(1.23, 4.56), 
                    new Coordinate(2.34, 5.67), 
                    new Coordinate(3.45, 6.78), 
                    new Coordinate(1.23, 4.56),
                }))
            };
            var lateral = new Lateral { Feature = feature };
            lateral.Data.Discharge.Type = LateralDischargeType.TimeSeries;
            lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(60)] = 1.23;
            lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(120)] = 2.34;
            lateral.Data.Discharge.TimeSeries[referenceDate.AddSeconds(180)] = 3.45;
            lateral.Data.Discharge.TimeSeries.Time.InterpolationType = InterpolationType.Linear;
            
            modelDefinition.SetReferenceDateAsDateTime(referenceDate);
            modelDefinition.Laterals.Add(lateral);

            string[] extFileLines;
            string[] bcFileLines;
            using (var temp = new TemporaryDirectory())
            {
                string extFile = Path.Combine(temp.Path,  "FlowFM_bnd.ext");
                string bcFile = Path.Combine(temp.Path,  "lateral_discharge.bc");

                // Call
                bndExtForceFile.Write(extFile, extFile, modelDefinition);

                extFileLines = File.ReadAllLines(extFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                bcFileLines = File.ReadAllLines(bcFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            }

            Assert.That(extFileLines, Has.Length.EqualTo(12));
            
            AssertGeneralSection(extFileLines, bndExtForceFile);

            Assert.That(extFileLines[3], Is.EqualTo("[lateral]"));
            AssertPropertyLine(extFileLines[4], "id", "some_id");
            AssertPropertyLine(extFileLines[5], "name", "some_id");
            AssertPropertyLine(extFileLines[6], "type", "discharge");
            AssertPropertyLine(extFileLines[7], "locationType", "2d");
            AssertPropertyLine(extFileLines[8], "numCoordinates", "3");
            AssertPropertyLine(extFileLines[9], "xCoordinates", "1.2300000e+000 2.3400000e+000 3.4500000e+000");
            AssertPropertyLine(extFileLines[10], "yCoordinates", "4.5600000e+000 5.6700000e+000 6.7800000e+000");
            AssertPropertyLine(extFileLines[11], "discharge", "lateral_discharge.bc");
            
            Assert.That(bcFileLines, Has.Length.EqualTo(11));
            Assert.That(bcFileLines[0], Is.EqualTo("[forcing]"));
            AssertPropertyLine(bcFileLines[1], "name", "some_id");
            AssertPropertyLine(bcFileLines[2], "function", "timeseries");
            AssertPropertyLine(bcFileLines[3], "timeInterpolation", "linear");
            AssertPropertyLine(bcFileLines[4], "quantity", "time");
            AssertPropertyLine(bcFileLines[5], "unit", "seconds since 2023-07-31 00:00:00");
            AssertPropertyLine(bcFileLines[6], "quantity", "lateral_discharge");
            AssertPropertyLine(bcFileLines[7], "unit", "m3/s");
            AssertDataLine(bcFileLines[8], "60", "1.23");
            AssertDataLine(bcFileLines[9], "120", "2.34");
            AssertDataLine(bcFileLines[10], "180", "3.45");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnyWaterFlowFmModelDefinition_WhenWriteBndExtForceFile_ThenWrittenFileContainsVersionInformation()
        {
            //Arrange
            var bndExtForceFile = new BndExtForceFile();
            var waterFlowFMModelDefinition = new WaterFlowFMModelDefinition();
            waterFlowFMModelDefinition.BoundaryConditionSets.AddRange(new []
            {
                new BoundaryConditionSet 
                { 
                    Feature = new Feature2D
                    {
                        Name = "TestName",
                        Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray())
                    } 
                }
            });

            const string fileName = "file_bnd.ext";

            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = tempDirectory.Path +@"\"+ fileName;
                
                //Act
                bndExtForceFile.Write(filePath, filePath, waterFlowFMModelDefinition);
                
                //Assert
                string[] extFileLines = File.ReadAllLines(filePath);
                AssertGeneralSection(extFileLines, bndExtForceFile);
            }
        }
        
        private static void AssertGeneralSection(string[] extFileLines, BndExtForceFile bndExtForceFile)
        {
            Assert.That(extFileLines[0], Is.EqualTo($"[{BndExtForceFileConstants.GeneralBlockKey}]"));
            AssertPropertyLine(extFileLines[1], BndExtForceFileConstants.FileVersionKey, bndExtForceFile.FileVersion);
            AssertPropertyLine(extFileLines[2], BndExtForceFileConstants.FileTypeKey, bndExtForceFile.FileType);
        }

        private static void CheckIfSubFilesMentionedInBndExtForceFileAreWrittenRelativeToThisFile(string line, string referenceFilePath)
        {
            string[] parts = line.Split('=');
            
            string subFilePath = parts[1].Trim();
            string directoryName = Path.GetDirectoryName(referenceFilePath);
            string expectedPath = Path.Combine(directoryName, subFilePath);
            
            Assert.IsTrue(File.Exists(expectedPath), $"Expected path does not exist: '{expectedPath}'.");
        }
        
        private static void AssertDataLine(string fileLine, string timeValue, string dataValue)
        {
            string[] parts = fileLine.Split(new char[] {}, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(parts[0].Trim(), Is.EqualTo(timeValue));
            Assert.That(parts[1].Trim(), Is.EqualTo(dataValue));
        }

        private static void AssertPropertyLine(string line, string propertyName, string value)
        {
            string[] pair = line.Split('=');
            Assert.That(pair[0].Trim(), Is.EqualTo(propertyName));
            Assert.That(pair[1].Trim(), Is.EqualTo(value));
        }
    }
}
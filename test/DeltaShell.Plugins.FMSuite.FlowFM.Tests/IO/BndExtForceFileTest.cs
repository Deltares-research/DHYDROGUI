using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using Assert = NUnit.Framework.Assert;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class BndExtForceFileTest
    {
        [SetUp]
        public void Setup()
        {
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }
        private static WaterFlowFMModelDefinition CreateModelDefinitionWithTwoBoundaries()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var firstBoundary = new Feature2D
            {
                Name = "Boundary1",
                Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0*i)).ToArray())
            };
            var secondBoundary = new Feature2D
            {
                Name = "Boundary2",
                Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(10.0 * i, 0)).ToArray())
            };
            modelDefinition.Boundaries.AddRange(new[] {firstBoundary, secondBoundary});
            modelDefinition.BoundaryConditionSets.AddRange(new[]
            {
                new BoundaryConditionSet {Feature = firstBoundary}, new BoundaryConditionSet() {Feature = secondBoundary}
            });

            return modelDefinition;
        }

        private static void FillTimeSeries(IFunction function, Func<int, double> mapping, DateTime start, DateTime stop, int steps)
        {
            var deltaT = stop - start;
            var times = Enumerable.Range(0, steps).Select(i => start + new TimeSpan(i*deltaT.Ticks));
            var values = Enumerable.Range(0, steps).Select(mapping);
            FunctionHelper.SetValuesRaw(function.Arguments[0], times);
            FunctionHelper.SetValuesRaw(function.Components[0], values);
        }

        private static void CompareBoundaryConditions(IBoundaryCondition first, IBoundaryCondition second)
        {
            Assert.AreEqual(first.DataType, second.DataType, "data type");
            Assert.AreEqual(first.VariableName, second.VariableName, "quantity");
            Assert.AreEqual(first.DataPointIndices, second.DataPointIndices, "data points");

            foreach (var dataPointIndex in first.DataPointIndices)
            {
                var firstFunction = first.GetDataAtPoint(dataPointIndex);
                var secondFunction = second.GetDataAtPoint(dataPointIndex);

                if (firstFunction.Arguments[0].ValueType == typeof (double))
                {
                    var firstList = firstFunction.Arguments[0].GetValues<double>().ToList();
                    var secondList = secondFunction.Arguments[0].GetValues<double>().ToList();

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
                int c = 0;
                foreach (var component in firstFunction.Components)
                {
                    var firstList = component.GetValues<double>().ToList();
                    var secondList = secondFunction.Components[c++].GetValues<double>().ToList();

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

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category("Quarantine")]
        public void OpenBoundaryToleranceIsOnlyWrittenForEmbankments()
        {
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();
            var embankment = new Embankment()
            {
                Geometry =
                    new LineString(
                        Enumerable.Range(2, 4).Select(i => new Coordinate(2, 4.0*i)).ToArray()),
                Name = "embankment1"
            };
            modelDefinition.Embankments.Add(embankment);

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition,Enumerable.Empty<Model1DBoundaryNodeData>(), Enumerable.Empty<Model1DLateralSourceData>());

            int boundaryCounter = 0;
            int openBoundaryToleranceCounter = 0;

            using (var file = new StreamReader("testbnd.ext"))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(BndExtForceFile.BoundaryBlockKey)) boundaryCounter++;
                    if (line.Contains("OpenBoundaryTolerance")) openBoundaryToleranceCounter++;
                }
                file.Close();
            }

            Assert.AreEqual(3, boundaryCounter, "File testbnd.ext should contain 3 boundaries");
            Assert.AreEqual(1, openBoundaryToleranceCounter, "File testbnd.ext should only contain an OpenBoundaryTolerance for each embankment");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category("Quarantine")]
        public void WriteReadEmptyBoundaries()
        {
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition, Enumerable.Empty<Model1DBoundaryNodeData>(), Enumerable.Empty<Model1DLateralSourceData>());

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition, new HydroNetwork(), new HydroArea(), new EventedList<Model1DBoundaryNodeData>(), new EventedList<Model1DLateralSourceData>());

            Assert.AreEqual(2, newModelDefinition.Boundaries.Count);
            Assert.AreEqual(2, newModelDefinition.BoundaryConditionSets.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadMixedQuantities()
        {
            var testFilePath = TestHelper.GetTestFilePath("BcFiles\\MixedQuantities.ext");
            var modelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read(testFilePath, modelDefinition, new HydroNetwork(), new HydroArea(), new EventedList<Model1DBoundaryNodeData>(),new EventedList<Model1DLateralSourceData>() );

            Assert.AreEqual(1,modelDefinition.BoundaryConditionSets.Count);
            Assert.AreEqual(2, modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Count);

            var salinityBoundaryCondition = modelDefinition.BoundaryConditionSets[0].BoundaryConditions[1] as FlowBoundaryCondition;
            Assert.AreEqual(new TimeSpan(0,0,42), salinityBoundaryCondition.ThatcherHarlemanTimeLag);
            Assert.AreEqual(FlowBoundaryQuantityType.Salinity, salinityBoundaryCondition.FlowQuantity);
            Assert.AreEqual(2, salinityBoundaryCondition.GetDataAtPoint(0).GetValues().Count);
            Assert.AreEqual(2, salinityBoundaryCondition.GetDataAtPoint(1).GetValues().Count);

            var waterlevelBoundaryCondition =
                modelDefinition.BoundaryConditionSets[0].BoundaryConditions[0] as FlowBoundaryCondition;
            Assert.AreEqual(TimeSpan.Zero, waterlevelBoundaryCondition.ThatcherHarlemanTimeLag);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel, waterlevelBoundaryCondition.FlowQuantity);

            CollectionAssert.AreEquivalent(new[]{3,4}, waterlevelBoundaryCondition.GetDataAtPoint(0).GetValues<double>());
            CollectionAssert.AreEquivalent(new[]{5,6}, waterlevelBoundaryCondition.GetDataAtPoint(1).GetValues<double>());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadWaterLevelTimeSeries()
        {
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var firstBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) {Feature = modelDefinition.Boundaries[0]};
            firstBoundaryCondition.AddPoint(0);
            firstBoundaryCondition.AddPoint(5);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Add(firstBoundaryCondition);

            var startTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

            var data = firstBoundaryCondition.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.5*Math.Sin(0.2*Math.PI*i), startTime, stopTime, 5);

            data = firstBoundaryCondition.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5*Math.Sin(0.8*Math.PI*i), startTime, stopTime, 3);

            var secondBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) {Feature = modelDefinition.Boundaries[1]};

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.Add(secondBoundaryCondition);
            secondBoundaryCondition.AddPoint(9);

            data = secondBoundaryCondition.GetDataAtPoint(9);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 10);

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition, Enumerable.Empty<Model1DBoundaryNodeData>(),Enumerable.Empty<Model1DLateralSourceData>());

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition, new HydroNetwork(), new HydroArea(), new EventedList<Model1DBoundaryNodeData>(),new EventedList<Model1DLateralSourceData>());

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
        public void WriteReadThatcherHarlemanTimelag()
        {
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            TimeSpan thatcherHarlemanTimeLag = new TimeSpan(0,0,40);
            var firstBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[0], ThatcherHarlemanTimeLag = thatcherHarlemanTimeLag};
            firstBoundaryCondition.AddPoint(0);
            firstBoundaryCondition.AddPoint(5);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Add(firstBoundaryCondition);

            var startTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

            var data = firstBoundaryCondition.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = firstBoundaryCondition.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            var secondBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.Add(secondBoundaryCondition);
            secondBoundaryCondition.AddPoint(9);

            data = secondBoundaryCondition.GetDataAtPoint(9);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 10);

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition, new HydroNetwork(), new HydroArea(), new EventedList<Model1DBoundaryNodeData>(), new EventedList<Model1DLateralSourceData>());

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
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var firstBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[0] };
            firstBoundaryCondition.AddPoint(1);
            firstBoundaryCondition.AddPoint(5);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Add(firstBoundaryCondition);

            var startTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

            var data = firstBoundaryCondition.GetDataAtPoint(1);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = firstBoundaryCondition.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            var secondBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.Qh) { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.Add(secondBoundaryCondition);
            secondBoundaryCondition.AddPoint(0);

            data = secondBoundaryCondition.GetDataAtPoint(0);
            data[10.0] = new[] {0.5};
            data[15.0] = new[] {0.75};
            data[20.0] = new[] {1.25};
            data[25.0] = new[] {2.5};

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition, new HydroNetwork(), new HydroArea(), new EventedList<Model1DBoundaryNodeData>(), new EventedList<Model1DLateralSourceData>());

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
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var firstBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[0] };
            firstBoundaryCondition.AddPoint(0);
            firstBoundaryCondition.AddPoint(5);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.Add(firstBoundaryCondition);

            var startTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

            var data = firstBoundaryCondition.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = firstBoundaryCondition.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            var secondBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[1] };

            var thirdBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) {Feature = modelDefinition.Boundaries[1]};

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[]
            {secondBoundaryCondition, thirdBoundaryCondition});

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
            writer.Write("testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition);

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
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.Harmonics) { Feature = modelDefinition.Boundaries[0] };
            bc1.AddPoint(0);
            bc1.AddPoint(8);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { bc1 });

            var data = bc1.GetDataAtPoint(0);
            data[15.5] = new[] { 0.6, 120 };
            data[4.78] = new[] { 0.1, 34 };

            data = bc1.GetDataAtPoint(8);
            data[25.5] = new[] { 0.16, 20 };
            data[14.78] = new[] { 0.11, 3.4 };

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.AstroComponents) { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[] { bc2 });

            bc2.AddPoint(2);
            bc2.AddPoint(9);

            data = bc2.GetDataAtPoint(2);
            data["A0"] = new[] { 0.5, 0 };
            data["M1"] = new[] { 0.15, 120 };
            data["P1"] = new[] { 0.6, 250 };

            data = bc2.GetDataAtPoint(9);
            data["A0"] = new[] { 0.6, 0 };
            data["M1"] = new[] { 0.16, 130 };
            data["P1"] = new[] { 0.61, 260 };

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition);

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
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[0] };
            bc1.AddPoint(0);
            bc1.AddPoint(5);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.Harmonics) {Feature = modelDefinition.Boundaries[0]};
            bc2.AddPoint(0);
            bc2.AddPoint(8);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] {bc1, bc2});

            var startTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

            var data = bc1.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.2 * Math.PI * i), startTime, stopTime, 5);

            data = bc1.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.5 * Math.Sin(0.8 * Math.PI * i), startTime, stopTime, 3);

            data = bc2.GetDataAtPoint(0);
            data[15.5] = new[] {0.6, 120};
            data[4.78] = new[] {0.1, 34};

            data = bc2.GetDataAtPoint(8);
            data[25.5] = new[] {0.16, 20};
            data[14.78] = new[] {0.11, 3.4};


            var bc3 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries) { Feature = modelDefinition.Boundaries[1] };

            var bc4 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.AstroComponents) { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[] { bc3, bc4 });

            bc3.AddPoint(9);
            bc4.AddPoint(2);
            bc4.AddPoint(9);

            data = bc3.GetDataAtPoint(9);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 7);

            data = bc4.GetDataAtPoint(2);
            data["A0"] = new[] {0.5, 0};
            data["M1"] = new[] {0.15, 120};
            data["P1"] = new[] {0.6, 250};

            data = bc4.GetDataAtPoint(9);
            data["A0"] = new[] { 0.6, 0 };
            data["M1"] = new[] { 0.16, 130 };
            data["P1"] = new[] { 0.61, 260 };

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition);

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
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.HarmonicCorrection) { Feature = modelDefinition.Boundaries[0] };
            bc1.AddPoint(0);
            bc1.AddPoint(8);

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { bc1 });

            var data = bc1.GetDataAtPoint(0);
            data[15.5] = new[] { 0.6, 120, 0.99, 10 };
            data[4.78] = new[] { 0.1, 34, 1.01, 11 };

            data = bc1.GetDataAtPoint(8);
            data[25.5] = new[] { 0.16, 20, 0.98, 11 };
            data[14.78] = new[] { 0.11, 3.4, 0.87, 20 };

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.AstroCorrection) { Feature = modelDefinition.Boundaries[1] };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[] { bc2 });

            bc2.AddPoint(2);
            bc2.AddPoint(9);

            data = bc2.GetDataAtPoint(2);
            data["A0"] = new[] { 0.5, 0 , 0, 0};
            data["M1"] = new[] { 0.15, 120, 1.11, 22 };
            data["P1"] = new[] { 0.6, 250, 0.98, 12 };

            data = bc2.GetDataAtPoint(9);
            data["A0"] = new[] { 0.6, 0, 0, 0 };
            data["M1"] = new[] { 0.16, 130, 1.22, 11 };
            data["P1"] = new[] { 0.61, 260, 0.89, 34 };

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition);

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
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.HarmonicCorrection) { Feature = modelDefinition.Boundaries[0] };
            bc1.AddPoint(0);
            bc1.AddPoint(8);

            var data = bc1.GetDataAtPoint(0);
            data[15.5] = new[] { 0.6, 120, 0.99, 10 };
            data[4.78] = new[] { 0.1, 34, 1.01, 11 };

            data = bc1.GetDataAtPoint(8);
            data[25.5] = new[] { 0.16, 20, 0.98, 11 };
            data[14.78] = new[] { 0.11, 3.4, 0.87, 20 };

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = modelDefinition.Boundaries[0]
            };
            var startTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            var stopTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

            bc2.AddPoint(0);
            data = bc2.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 7);

            bc2.AddPoint(5);
            data = bc2.GetDataAtPoint(5);
            FillTimeSeries(data, i => 0.65 * Math.Sin(0.4 * Math.PI * i), startTime, stopTime, 20);

            var bc3 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.TimeSeries)
            {
                Feature = modelDefinition.Boundaries[0]
            };
            bc3.AddPoint(7);
            data = bc3.GetDataAtPoint(7);
            FillTimeSeries(data, i => 0.87/(i*i + 1), startTime, stopTime, 10);

            var bc4 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                BoundaryConditionDataType.AstroCorrection) {Feature = modelDefinition.Boundaries[0]};
            bc4.AddPoint(7);
            data = bc4.GetDataAtPoint(7);
            data["A0"] = new[] { 0.5, 0, 0, 0 };
            data["M1"] = new[] { 0.15, 120, 1.11, 22 };
            data["P1"] = new[] { 0.6, 250, 0.98, 12 };

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { bc1, bc2, bc3, bc4 });

            var bc5 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.AstroCorrection) { Feature = modelDefinition.Boundaries[1] };

            bc5.AddPoint(2);
            data = bc5.GetDataAtPoint(2);
            data["A0"] = new[] { 0.5, 0, 0, 0 };
            data["M1"] = new[] { 0.15, 120, 1.11, 22 };
            data["P1"] = new[] { 0.6, 250, 0.98, 12 };

            bc5.AddPoint(9);
            data = bc5.GetDataAtPoint(9);
            data["A0"] = new[] { 0.6, 0, 0, 0 };
            data["M1"] = new[] { 0.16, 130, 1.22, 11 };
            data["P1"] = new[] { 0.61, 260, 0.89, 34 };

            var bc6 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                BoundaryConditionDataType.HarmonicCorrection) {Feature = modelDefinition.Boundaries[1]};

            bc6.AddPoint(0);
            data = bc6.GetDataAtPoint(0);
            data[15.5] = new[] { 20, 120, 0.99, 10 };
            data[4.78] = new[] { 21, 34, 1.01, 11 };

            modelDefinition.BoundaryConditionSets[1].BoundaryConditions.AddRange(new[] { bc5, bc6 });

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition);

            var newModelDefinition = new WaterFlowFMModelDefinition();

            var reader = new BndExtForceFile();
            reader.Read("testbnd.ext", newModelDefinition);

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
        public void ReadWriteReadEmbankmentForcingsTest()
        {
            var def = new WaterFlowFMModelDefinition();
            var extPath = TestHelper.GetTestFilePath(@"banks\flooding1d2d_bnd.ext");

            new BndExtForceFile().Read(extPath, def);

            Assert.AreEqual(0, def.Boundaries.Count);
            Assert.AreEqual(0, def.BoundaryConditionSets.Count);
            Assert.AreEqual(1, def.Embankments.Count);

            const string newExtPath = "test_bnd.ext";
            new BndExtForceFile().Write(newExtPath, def);

            var newDef = new WaterFlowFMModelDefinition();
            new BndExtForceFile().Read(newExtPath, newDef);

            Assert.AreEqual(0, newDef.Boundaries.Count);
            Assert.AreEqual(0, newDef.BoundaryConditionSets.Count);
            Assert.AreEqual(1, newDef.Embankments.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteMorphBoundaryCondition()
        {
            var modelDefinition = CreateModelDefinitionWithTwoBoundaries();
            modelDefinition.ModelName = "MyModelName";

            var morbc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
                    BoundaryConditionDataType.TimeSeries)
                { Feature = modelDefinition.Boundaries[0] };

            modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { morbc1 });

            var writer = new BndExtForceFile();
            writer.Write("testbnd.ext", modelDefinition);
            Assert.IsTrue(File.Exists(modelDefinition.ModelName + BcmFile.Extension));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteSedimentConcentration()
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

            model.BoundaryConditionSets[1].BoundaryConditions.Add(bCond);

            bCond.AddPoint(0);
            var dataAtZero = bCond.GetDataAtPoint(0);
            dataAtZero[model.StartTime] = new[] { 36.0 };
            dataAtZero[model.StartTime.AddMinutes(2)] = new[] { 18.0 };

            var bndExtForceFile = new BndExtForceFile();
            var extFileName = model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).GetValueAsString();
            if (string.IsNullOrEmpty(extFileName))
                extFileName = model.ModelDefinition.ModelName + ExtForceFile.Extension;
            
            bndExtForceFile.Write(extFileName, model.ModelDefinition);

            Assert.IsTrue(File.Exists(extFileName));
            var fileText = File.ReadAllText(extFileName);
            Assert.IsTrue(fileText.Contains(BndExtForceFile.BoundaryBlockKey) && 
                          fileText.Contains(BndExtForceFile.QuantityKey + "=" + BcFileFlowBoundaryDataBuilder.ConcentrationAtBound + "frac1") &&
                          fileText.Contains(BndExtForceFile.LocationFileKey + "=" + "L1.pli") &&
                          fileText.Contains(BndExtForceFile.ForcingFileKey + "=" + "frac1.bc"));

            //check to see if only 2 boundaries are written
            Assert.AreEqual(2, new Regex(Regex.Escape(BndExtForceFile.BoundaryBlockKey)).Matches(fileText).Count);

            Assert.IsTrue(File.Exists("frac1.bc"));

            fileText = File.ReadAllText("frac1.bc");
            Assert.IsTrue(fileText.Contains(BcFileFlowBoundaryDataBuilder.ConcentrationAtBound + "frac1") &&
                          fileText.Contains("bound_0001") &&
                          fileText.Contains(new BcFile().BlockKey) &&
                          fileText.Contains(BcFile.QuantityKey) &&
                          fileText.Contains("36") &&
                          fileText.Contains("18"));
        }
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadSedimentConcentration()
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

            model.BoundaryConditionSets[1].BoundaryConditions.Add(bCond);

            bCond.AddPoint(0);
            var dataAtZero = bCond.GetDataAtPoint(0);
            dataAtZero[model.StartTime] = new[] { 36.0 };
            dataAtZero[model.StartTime.AddMinutes(2)] = new[] { 18.0 };

            var bndExtForceFile = new BndExtForceFile();
            var extFileName = model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).GetValueAsString();
            if (string.IsNullOrEmpty(extFileName))
                extFileName = model.ModelDefinition.ModelName + ExtForceFile.Extension;

            bndExtForceFile.Write(extFileName, model.ModelDefinition);
            var modelDefinition = new WaterFlowFMModelDefinition(Path.GetTempPath(),"myModel");
            bndExtForceFile = new BndExtForceFile();
            bndExtForceFile.Read(extFileName, modelDefinition);
        }

        [Test]
        public void WriteBndExtForceFileSubFilesReturnsNoItemsIfMissingName()
        {
            var bndExtForceFile = new BndExtForceFile();
            var firstBoundary = new Feature2D
            {
                Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray())
            };
            Assert.IsTrue(string.IsNullOrEmpty(firstBoundary.Name));

            var bcSet = new BoundaryConditionSet { Feature = firstBoundary };
            var resultingItems = bndExtForceFile.WriteBndExtForceFileSubFiles(string.Empty, new List<BoundaryConditionSet>{ bcSet}, DateTime.Today);

            Assert.IsFalse(resultingItems.Any());
        }

        [Test]
        [Category("Quarantine")]
        public void WriteBndExtForceFileSubFilesReturnsItemsWhenNameIsNotMissing()
        {
            var bndExtForceFile = new BndExtForceFile();
            var testFolder = Path.GetDirectoryName(Path.GetDirectoryName(TestHelper.GetTestDataDirectory())); //This will place it under /debug
            Assert.IsNotNull(testFolder);

            var fileDirectory = Path.Combine(testFolder, "WriteBndExtForceFileSubFilesReturnsItemsWhenNameIsNotMissing");
            FileUtils.DeleteIfExists(fileDirectory);

            var filePath = Path.Combine(fileDirectory, "file.ext");
            FileUtils.DeleteIfExists(filePath);

            var firstBoundary = new Feature2D
            {
                Name = "TestName",
                Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray())
            };
            var secondBoundary = new Feature2D
            {
                Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray())
            };
            Assert.IsTrue(string.IsNullOrEmpty(secondBoundary.Name));

            var bcSetOne = new BoundaryConditionSet { Feature = firstBoundary };
            var bcSetTwo = new BoundaryConditionSet { Feature = secondBoundary };

            var md = new WaterFlowFMModelDefinition();
            md.BoundaryConditionSets.AddRange(new[]{bcSetOne, bcSetTwo});

            Assert.IsFalse(File.Exists(filePath));
            bndExtForceFile.Write(filePath, md);
            Assert.IsTrue(File.Exists(filePath));

            var lines = File.ReadAllLines(filePath);
            Assert.IsNotNull(lines);

            //Make sure only one boundary has been added (double check from the test WriteBndExtForceFileSubFilesReturnsNoItemsIfMissingName above)
            Assert.AreEqual(1, lines.Count( l => l.Contains("[boundary]")));
            Assert.IsTrue(lines.Any( l => l.Contains("TestName.pli")));

            

            FileUtils.DeleteIfExists(filePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FileWithMeteoData_CorrectlyReadsTheMeteoData()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string extFileContent =
                    "[general]                            " + Environment.NewLine +
                    "    fileVersion     = 2.0            " + Environment.NewLine +
                    "    fileType        = extForce       " + Environment.NewLine +
                    "    fileType        = extForce       " + Environment.NewLine +
                    "                                     " + Environment.NewLine +
                    "[meteo]                              " + Environment.NewLine +
                    "    quantity        = rainfall_rate  " + Environment.NewLine +
                    "    forcingFileType = bcAscii        " + Environment.NewLine +
                    "    forcingFile     = FlowFM_meteo.bc";

                string bcFileContent =
                    "[General]                                            " + Environment.NewLine +
                    "    fileVersion   = 1.01                             " + Environment.NewLine +
                    "    fileType      = boundConds                       " + Environment.NewLine +
                    "                                                     " + Environment.NewLine +
                    "[forcing]                                            " + Environment.NewLine +
                    "Name              = global                           " + Environment.NewLine +
                    "Function          = timeseries                       " + Environment.NewLine +
                    "timeInterpolation = linear                           " + Environment.NewLine +
                    "Quantity          = time                             " + Environment.NewLine +
                    "Unit              = seconds since 2021-01-01 00:00:00" + Environment.NewLine +
                    "Quantity          = rainfall_rate                    " + Environment.NewLine +
                    "Unit              = mm day-1                         " + Environment.NewLine +
                    "100    1.23                                          " + Environment.NewLine +
                    "200    4.56                                          " + Environment.NewLine +
                    "300    7.89                                          ";
                
                string extFile = temp.CreateFile("FlowFM_bnd.ext", extFileContent);
                temp.CreateFile("FlowFM_meteo.bc", bcFileContent);

                var bndExtForceFile = new BndExtForceFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var hydroArea = new HydroArea();
                
                // Call
                bndExtForceFile.Read(extFile, modelDefinition, area: hydroArea);

                // Assert
                IFmMeteoField meteoData = modelDefinition.FmMeteoFields.Single();
                Assert.That(meteoData.Name, Is.EqualTo("Precipication rainfall (Global)"));
                Assert.That(meteoData.Quantity, Is.EqualTo(FmMeteoQuantity.Precipitation));
                Assert.That(meteoData.FmMeteoLocationType, Is.EqualTo(FmMeteoLocationType.Global));

                var function = (TimeSeries) meteoData.Data;
                Assert.That(function.Arguments, Has.Count.EqualTo(1));
                Assert.That(function.Components, Has.Count.EqualTo(1));
                Assert.That(function.Components[0].Name, Is.EqualTo("Precipitation"));
                
                IMultiDimensionalArray<DateTime> times = function.Time.Values;
                IMultiDimensionalArray values = function.Components[0].Values;
                Assert.That(times[0], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(100)));
                Assert.That(values[0], Is.EqualTo(1.23));
                Assert.That(times[1], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(200)));
                Assert.That(values[1], Is.EqualTo(4.56));
                Assert.That(times[2], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(300)));
                Assert.That(values[2], Is.EqualTo(7.89));
                
                Assert.That(hydroArea.RoofAreas, Is.Empty);
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FileWithMeteoDataAndRoofs_CorrectlyReadsTheMeteoDataAndRoofs()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string extFileContent =
                    "[general]                                " + Environment.NewLine +
                    "    fileVersion        = 2.0             " + Environment.NewLine +
                    "    fileType           = extForce        " + Environment.NewLine +
                    "    fileType           = extForce        " + Environment.NewLine +
                    "                                         " + Environment.NewLine +
                    "[meteo]                                  " + Environment.NewLine +
                    "    quantity           = rainfall_rate   " + Environment.NewLine +
                    "    forcingFileType    = bcAscii         " + Environment.NewLine +
                    "    forcingFile        = FlowFM_meteo.bc " + Environment.NewLine +
                    "    targetMaskFile     = FlowFM_roofs.pol" + Environment.NewLine +
                    "    targetMaskInvert   = true            " + Environment.NewLine +
                    "    interpolationMethod= nearestNb       ";

                string bcFileContent =
                    "[General]                                            " + Environment.NewLine +
                    "    fileVersion   = 1.01                             " + Environment.NewLine +
                    "    fileType      = boundConds                       " + Environment.NewLine +
                    "                                                     " + Environment.NewLine +
                    "[forcing]                                            " + Environment.NewLine +
                    "Name              = global                           " + Environment.NewLine +
                    "Function          = timeseries                       " + Environment.NewLine +
                    "timeInterpolation = linear                           " + Environment.NewLine +
                    "Quantity          = time                             " + Environment.NewLine +
                    "Unit              = seconds since 2021-01-01 00:00:00" + Environment.NewLine +
                    "Quantity          = rainfall_rate                    " + Environment.NewLine +
                    "Unit              = mm day-1                         " + Environment.NewLine +
                    "100    1.23                                          " + Environment.NewLine +
                    "200    4.56                                          " + Environment.NewLine +
                    "300    7.89                                          ";
                
                string roofFileContent =
                    "SomeRoof                          " + Environment.NewLine +
                    "    5    2                        " + Environment.NewLine +
                    "        0.0        0.0            " + Environment.NewLine +
                    "        1.0        0.0            " + Environment.NewLine +
                    "        1.0        1.0            " + Environment.NewLine +
                    "        0.0        1.0            " + Environment.NewLine +
                    "        0.0        0.0            ";
                
                string extFile = temp.CreateFile("FlowFM_bnd.ext", extFileContent);
                temp.CreateFile("FlowFM_meteo.bc", bcFileContent);
                temp.CreateFile("FlowFM_roofs.pol", roofFileContent);

                var bndExtForceFile = new BndExtForceFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var hydroArea = new HydroArea();
                
                // Call
                bndExtForceFile.Read(extFile, modelDefinition, area: hydroArea);

                // Assert
                IFmMeteoField meteoData = modelDefinition.FmMeteoFields.Single();
                Assert.That(meteoData.Name, Is.EqualTo("Precipication rainfall (Global)"));
                Assert.That(meteoData.Quantity, Is.EqualTo(FmMeteoQuantity.Precipitation));
                Assert.That(meteoData.FmMeteoLocationType, Is.EqualTo(FmMeteoLocationType.Global));

                var function = (TimeSeries) meteoData.Data;
                Assert.That(function.Arguments, Has.Count.EqualTo(1));
                Assert.That(function.Components, Has.Count.EqualTo(1));
                Assert.That(function.Components[0].Name, Is.EqualTo("Precipitation"));
                
                IMultiDimensionalArray<DateTime> times = function.Time.Values;
                IMultiDimensionalArray values = function.Components[0].Values;
                Assert.That(times[0], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(100)));
                Assert.That(values[0], Is.EqualTo(1.23));
                Assert.That(times[1], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(200)));
                Assert.That(values[1], Is.EqualTo(4.56));
                Assert.That(times[2], Is.EqualTo(new DateTime(2021, 1, 1).AddSeconds(300)));
                Assert.That(values[2], Is.EqualTo(7.89));
                
                GroupableFeature2DPolygon roof = hydroArea.RoofAreas.Single();
                Assert.That(roof.Name, Is.EqualTo("SomeRoof"));

                IGeometry geometry = roof.Geometry;
                Assert.That(geometry, Is.TypeOf<Polygon>());
                Assert.That(geometry.Coordinates, Has.Length.EqualTo(5));
                Assert.That(geometry.Coordinates[0].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[0].Y, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[1].X, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[1].Y, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[2].X, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[2].Y, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[3].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[3].Y, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[4].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[4].Y, Is.EqualTo(0.0));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FileWithRoofs_CorrectlyReadsTheRoofs()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string extFileContent =
                    "[general]                                " + Environment.NewLine +
                    "    fileVersion        = 2.0             " + Environment.NewLine +
                    "    fileType           = extForce        " + Environment.NewLine +
                    "    fileType           = extForce        " + Environment.NewLine +
                    "                                         " + Environment.NewLine +
                    "[meteo]                                  " + Environment.NewLine +
                    "    targetMaskFile     = FlowFM_roofs.pol" + Environment.NewLine +
                    "    targetMaskInvert   = true            " + Environment.NewLine +
                    "    interpolationMethod= nearestNb       ";
                
                string roofFileContent =
                    "SomeRoof                          " + Environment.NewLine +
                    "    5    2                        " + Environment.NewLine +
                    "        0.0        0.0            " + Environment.NewLine +
                    "        1.0        0.0            " + Environment.NewLine +
                    "        1.0        1.0            " + Environment.NewLine +
                    "        0.0        1.0            " + Environment.NewLine +
                    "        0.0        0.0            ";
                
                string extFile = temp.CreateFile("FlowFM_bnd.ext", extFileContent);
                temp.CreateFile("FlowFM_roofs.pol", roofFileContent);

                var bndExtForceFile = new BndExtForceFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var hydroArea = new HydroArea();
                
                // Call
                bndExtForceFile.Read(extFile, modelDefinition, area: hydroArea);

                // Assert
                Assert.That(modelDefinition.FmMeteoFields, Is.Empty);
                
                GroupableFeature2DPolygon roof = hydroArea.RoofAreas.Single();
                Assert.That(roof.Name, Is.EqualTo("SomeRoof"));

                IGeometry geometry = roof.Geometry;
                Assert.That(geometry, Is.TypeOf<Polygon>());
                Assert.That(geometry.Coordinates, Has.Length.EqualTo(5));
                Assert.That(geometry.Coordinates[0].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[0].Y, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[1].X, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[1].Y, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[2].X, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[2].Y, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[3].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[3].Y, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[4].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[4].Y, Is.EqualTo(0.0));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FileWithRoofs_PolFileDoesNotExist_ReportWarning()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string extFileContent =
                    "[general]                                " + Environment.NewLine +
                    "    fileVersion        = 2.0             " + Environment.NewLine +
                    "    fileType           = extForce        " + Environment.NewLine +
                    "    fileType           = extForce        " + Environment.NewLine +
                    "                                         " + Environment.NewLine +
                    "[meteo]                                  " + Environment.NewLine +
                    "    targetMaskFile     = FlowFM_roofs.pol" + Environment.NewLine +
                    "    targetMaskInvert   = true            " + Environment.NewLine +
                    "    interpolationMethod= nearestNb       ";
                
                string extFile = temp.CreateFile("FlowFM_bnd.ext", extFileContent);

                var bndExtForceFile = new BndExtForceFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var hydroArea = new HydroArea();
                
                // Call
                void Call ()=> bndExtForceFile.Read(extFile, modelDefinition, area: hydroArea);

                // Assert
                string[] warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToArray();
                var expWarning = $"File does not exist: {Path.Combine(temp.Path, "FlowFM_roofs.pol")}";
                
                Assert.That(warnings, Does.Contain(expWarning));
                Assert.That(hydroArea.RoofAreas, Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_WithMeteoAndRoofs_WritesCorrectFiles()
        {
            using (var temp = new TemporaryDirectory())
            {
                string extFilePath = Path.Combine(temp.Path, "FlowFM_bnd.ext");
                string bcFilePath = Path.Combine(temp.Path, "FlowFM_meteo.bc");
                string polFilePath = Path.Combine(temp.Path, "FlowFM_roofs.pol");

                var referenceDate = new DateTime(2021, 6, 24);
                
                var meteoField = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
                meteoField.Data[referenceDate.AddSeconds(100)] = 1.23;
                meteoField.Data[referenceDate.AddSeconds(200)] = 4.56;
                meteoField.Data[referenceDate.AddSeconds(300)] = 7.89;
                
                var modelDefinition = new WaterFlowFMModelDefinition {ModelName = "FlowFM"};
                modelDefinition.FmMeteoFields.Add(meteoField);
                modelDefinition.SetModelProperty(KnownProperties.RefDate, referenceDate);
                
                var coordinates = new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(1.0, 0.0),
                    new Coordinate(1.0, 1.0),
                    new Coordinate(0.0, 1.0),
                    new Coordinate(0.0, 0.0),
                };
                var geometry = Substitute.For<IGeometry>();
                geometry.Coordinates.Returns(coordinates);
                var roofArea = new GroupableFeature2DPolygon
                {
                    Geometry = geometry,
                    Name = "some_roof"
                };
                
                var bndExtForceFile = new BndExtForceFile();
                
                // Call
                bndExtForceFile.Write(extFilePath, modelDefinition, roofAreas: new[]{roofArea});
                
                // Assert
                Assert.That(extFilePath, Does.Exist);
                Assert.That(bcFilePath, Does.Exist);
                Assert.That(polFilePath, Does.Exist);

                string[][] extData = ReadData(extFilePath).ToArray();
                AssertLine(extData[3], "[meteo]");
                AssertLine(extData[4], "quantity", "rainfall_rate");
                AssertLine(extData[5], "forcingfile", "FlowFM_meteo.bc");
                AssertLine(extData[6], "forcingFileType", "bcAscii");
                AssertLine(extData[7], "targetMaskFile", "FlowFM_roofs.pol");
                AssertLine(extData[8], "targetMaskInvert", "true");
                AssertLine(extData[9], "interpolationMethod", "nearestNb");
                
                string[][] bcData = ReadData(bcFilePath).ToArray();
                AssertLine(bcData[3], "[forcing]");
                AssertLine(bcData[4], "Name", "global");
                AssertLine(bcData[5], "Function", "timeseries");
                AssertLine(bcData[6], "timeInterpolation", "linear");
                AssertLine(bcData[7], "Quantity", "time");
                AssertLine(bcData[8], "Unit", "seconds", "since", "2021-06-24", "00:00:00");
                AssertLine(bcData[9], "Quantity", "rainfall_rate");
                AssertLine(bcData[10], "Unit", "mm", "day-1");
                AssertLine(bcData[11], "100", "1.23");
                AssertLine(bcData[12], "200", "4.56");
                AssertLine(bcData[13], "300", "7.89");

                string[][] polData = ReadData(polFilePath).ToArray();
                AssertLine(polData[0], "some_roof");
                AssertLine(polData[1], "5", "2");
                AssertLine(polData[2], "0", "0");
                AssertLine(polData[3], "1", "0");
                AssertLine(polData[4], "1", "1");
                AssertLine(polData[5], "0", "1");
                AssertLine(polData[6], "0", "0");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_WithMeteo_WritesCorrectFiles()
        {
            using (var temp = new TemporaryDirectory())
            {
                string extFilePath = Path.Combine(temp.Path, "FlowFM_bnd.ext");
                string bcFilePath = Path.Combine(temp.Path, "FlowFM_meteo.bc");
                string polFilePath = Path.Combine(temp.Path, "FlowFM_roofs.pol");

                var referenceDate = new DateTime(2021, 6, 24);
                
                var meteoField = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
                meteoField.Data[referenceDate.AddSeconds(100)] = 1.23;
                meteoField.Data[referenceDate.AddSeconds(200)] = 4.56;
                meteoField.Data[referenceDate.AddSeconds(300)] = 7.89;
                
                var modelDefinition = new WaterFlowFMModelDefinition {ModelName = "FlowFM"};
                modelDefinition.FmMeteoFields.Add(meteoField);
                modelDefinition.SetModelProperty(KnownProperties.RefDate, referenceDate);
                
                var bndExtForceFile = new BndExtForceFile();
                
                // Call
                bndExtForceFile.Write(extFilePath, modelDefinition);
                
                // Assert
                Assert.That(extFilePath, Does.Exist);
                Assert.That(bcFilePath, Does.Exist);
                Assert.That(polFilePath, Does.Not.Exist);
                
                string[][] extData = ReadData(extFilePath).ToArray();
                AssertLine(extData[3], "[meteo]");
                AssertLine(extData[4], "quantity", "rainfall_rate");
                AssertLine(extData[5], "forcingfile", "FlowFM_meteo.bc");
                AssertLine(extData[6], "forcingFileType", "bcAscii");

                string[][] bcData = ReadData(bcFilePath).ToArray();
                AssertLine(bcData[3], "[forcing]");
                AssertLine(bcData[4], "Name", "global");
                AssertLine(bcData[5], "Function", "timeseries");
                AssertLine(bcData[6], "timeInterpolation", "linear");
                AssertLine(bcData[7], "Quantity", "time");
                AssertLine(bcData[8], "Unit", "seconds", "since", "2021-06-24", "00:00:00");
                AssertLine(bcData[9], "Quantity", "rainfall_rate");
                AssertLine(bcData[10], "Unit", "mm", "day-1");
                AssertLine(bcData[11], "100", "1.23");
                AssertLine(bcData[12], "200", "4.56");
                AssertLine(bcData[13], "300", "7.89");
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_WithRoofs_WritesCorrectFiles()
        {
            using (var temp = new TemporaryDirectory())
            {
                string extFilePath = Path.Combine(temp.Path, "FlowFM_bnd.ext");
                string bcFilePath = Path.Combine(temp.Path, "FlowFM_meteo.bc");
                string polFilePath = Path.Combine(temp.Path, "FlowFM_roofs.pol");
                
                var modelDefinition = new WaterFlowFMModelDefinition {ModelName = "FlowFM"};
                
                var coordinates = new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(1.0, 0.0),
                    new Coordinate(1.0, 1.0),
                    new Coordinate(0.0, 1.0),
                    new Coordinate(0.0, 0.0),
                };
                var geometry = Substitute.For<IGeometry>();
                geometry.Coordinates.Returns(coordinates);
                var roofArea = new GroupableFeature2DPolygon
                {
                    Geometry = geometry,
                    Name = "some_roof"
                };
                
                var bndExtForceFile = new BndExtForceFile();
                
                // Call
                bndExtForceFile.Write(extFilePath, modelDefinition, roofAreas: new[]{roofArea});
                
                // Assert
                Assert.That(extFilePath, Does.Exist);
                Assert.That(bcFilePath, Does.Exist);
                Assert.That(polFilePath, Does.Exist);
                
                string[][] extData = ReadData(extFilePath).ToArray();
                AssertLine(extData[3], "[meteo]");
                AssertLine(extData[4], "targetMaskFile", "FlowFM_roofs.pol");
                AssertLine(extData[5], "targetMaskInvert", "true");
                AssertLine(extData[6], "interpolationMethod", "nearestNb");
                
                string[][] polData = ReadData(polFilePath).ToArray();
                AssertLine(polData[0], "some_roof");
                AssertLine(polData[1], "5", "2");
                AssertLine(polData[2], "0", "0");
                AssertLine(polData[3], "1", "0");
                AssertLine(polData[4], "1", "1");
                AssertLine(polData[5], "0", "1");
                AssertLine(polData[6], "0", "0");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_LateralSourceWithChainageOnBranch_IsReadCorrectly()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string extFileContent =
                    "[general]                            " + Environment.NewLine +
                    "    fileVersion = 2.0                " + Environment.NewLine +
                    "    fileType    = extForce           " + Environment.NewLine +
                    "                                     " + Environment.NewLine +
                    "[Lateral]                            " + Environment.NewLine +
                    "    id          = lateral_source_id  " + Environment.NewLine +
                    "    name        = lateral_source_name" + Environment.NewLine +
                    "    branchId    = branch_id          " + Environment.NewLine +
                    "    chainage    = 12.34              " + Environment.NewLine +
                    "    discharge   = realtime           ";
                
                string extFile = temp.CreateFile("FlowFM_bnd.ext", extFileContent);

                var bndExtForceFile = new BndExtForceFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                
                var branch = Substitute.For<IBranch>();
                branch.Name = "branch_id";
                branch.BranchFeatures = new EventedList<IBranchFeature>();
                branch.Geometry = new LineString(new[]
                {
                    new Coordinate(100, 0),
                    new Coordinate(200, 0)
                });
                
                var network = Substitute.For<IHydroNetwork>();
                network.Branches = new EventedList<IBranch> {branch};
                
                var lateralSourcesData = new EventedList<Model1DLateralSourceData>();
                
                // Call
                bndExtForceFile.Read(extFile, modelDefinition, network, lateralSourcesData: lateralSourcesData);
                
                // Assert
                var lateralSource = (LateralSource) branch.BranchFeatures.Single();
                Assert.That(lateralSource.Branch, Is.SameAs(branch));
                Assert.That(lateralSource.Chainage, Is.EqualTo(12.34));
                Assert.That(lateralSource.Name, Is.EqualTo("lateral_source_id"));
                Assert.That(lateralSource.LongName, Is.EqualTo("lateral_source_name"));
                Assert.That(lateralSource.Geometry, Is.TypeOf<Point>());
                Assert.That(lateralSource.Geometry.InteriorPoint.X, Is.EqualTo(112.34));
                Assert.That(lateralSource.Geometry.InteriorPoint.Y, Is.EqualTo(0));

                Model1DLateralSourceData modeLateralSourceData = lateralSourcesData.Single();
                Assert.That(modeLateralSourceData.Feature, Is.SameAs(lateralSource));
                Assert.That(modeLateralSourceData.DataType, Is.EqualTo(Model1DLateralDataType.FlowRealTime));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(Read_LateralSourceWithNodeIdOfPipeCases))]
        public void Read_LateralSourceWithCompartmentOfPipe_IsReadCorrectly(IPipe pipe1, IPipe pipe2, string nodeId,
                                                                            IPipe expPipe, double expChainage, ICompartment expCompartment)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string extFileContent =
                    "[general]                            " + Environment.NewLine +
                    "    fileVersion = 2.0                " + Environment.NewLine +
                    "    fileType    = extForce           " + Environment.NewLine +
                    "                                     " + Environment.NewLine +
                    "[Lateral]                            " + Environment.NewLine +
                    "    id          = lateral_source_id  " + Environment.NewLine +
                    "    name        = lateral_source_name" + Environment.NewLine +
                    $"   nodeId      = {nodeId}           " + Environment.NewLine +
                    "    discharge   = realtime           ";

                string extFile = temp.CreateFile("FlowFM_bnd.ext", extFileContent);

                var bndExtForceFile = new BndExtForceFile();
                var modelDefinition = new WaterFlowFMModelDefinition();

                var network = new HydroNetwork();
                network.Branches.Add(pipe1);
                network.Branches.Add(pipe2);

                var lateralSourcesData = new EventedList<Model1DLateralSourceData>();

                // Call
                bndExtForceFile.Read(extFile, modelDefinition, network, lateralSourcesData: lateralSourcesData);

                // Assert
                var lateralSource = (LateralSource) expPipe.BranchFeatures.Single();
                Assert.That(lateralSource.Branch, Is.SameAs(expPipe));
                Assert.That(lateralSource.Chainage, Is.EqualTo(expChainage));
                Assert.That(lateralSource.Name, Is.EqualTo("lateral_source_id"));
                Assert.That(lateralSource.LongName, Is.EqualTo("lateral_source_name"));
                Assert.That(lateralSource.Geometry, Is.TypeOf<Point>());
                Assert.That(lateralSource.Geometry.InteriorPoint.X, Is.EqualTo(expPipe.Geometry.InteriorPoint.X + expChainage));
                Assert.That(lateralSource.Geometry.InteriorPoint.Y, Is.EqualTo(0));

                Model1DLateralSourceData modeLateralSourceData = lateralSourcesData.Single();
                Assert.That(modeLateralSourceData.Feature, Is.SameAs(lateralSource));
                Assert.That(modeLateralSourceData.DataType, Is.EqualTo(Model1DLateralDataType.FlowRealTime));
                Assert.That(modeLateralSourceData.Compartment, Is.SameAs(expCompartment));
            }
        }

        private static IEnumerable<TestCaseData> Read_LateralSourceWithNodeIdOfPipeCases()
        {
            // first pipe, second pipe, referenced nodeId in file, exp. pipe of lateral source, exp. chainage of lateral source, exp. compartment of lateral source data
            IPipe pipeA1 = CreatePipe(100, 0, 200, 0, "node_id_1", "node_id_2");
            IPipe pipeA2 = CreatePipe(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(pipeA1, pipeA2, "node_id_1", pipeA1, 0, pipeA1.SourceCompartment);
            
            IPipe pipeB1 = CreatePipe(100, 0, 200, 0, "node_id_1", "node_id_2");
            IPipe pipeB2 = CreatePipe(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(pipeB1, pipeB2, "node_id_2", pipeB1, 100, pipeB1.TargetCompartment);
            
            IPipe pipeC1 = CreatePipe(100, 0, 200, 0, "node_id_1", "node_id_2");
            IPipe pipeC2 = CreatePipe(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(pipeC1, pipeC2, "node_id_3", pipeC2, 100, pipeC2.TargetCompartment);
        }

        private static IPipe CreatePipe(double x1, double y1, double x2, double y2, string sourceCompartmentName, string targetCompartmentName)
        {
            var c1 = new Coordinate(x1, y1);
            var c2 = new Coordinate(x2, y2);

            var geometry = new LineString(new[]
            {
                c1,
                c2
            });

            double length = Math.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

            var pipe = new Pipe
            {
                Length = length,
                Geometry = geometry,
                SourceCompartment = Substitute.For<ICompartment>(),
                SourceCompartmentName = sourceCompartmentName,
                TargetCompartment = Substitute.For<ICompartment>(),
                TargetCompartmentName = targetCompartmentName
            };

            return pipe;
        }

        private static IEnumerable<string[]> ReadData(string filePath)
        {
            foreach (string line in File.ReadAllLines(filePath))
            {
                string[] split = line.Split(new[] {'=', ' '}, StringSplitOptions.RemoveEmptyEntries);
                if (!split.Any())
                {
                    continue;
                }
                yield return split.Select(s => s.Trim()).ToArray();
            }
        }

        private static void AssertLine(string[] data, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Assert.That(data[i], Is.EqualTo(values[i]));
            }
        }
    }
}

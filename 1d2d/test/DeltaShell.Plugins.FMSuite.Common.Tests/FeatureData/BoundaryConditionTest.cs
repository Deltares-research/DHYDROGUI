using System;
using System.Collections.Generic;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FeatureData
{
    public class TestBoundaryCondition: BoundaryCondition
    {
        public bool isHorizontallyUniform;
        public bool isVerticallyUniform;

        public TestBoundaryCondition(BoundaryConditionDataType type, bool isHorizontallyUniform, bool isVerticallyUniform) : base(type)
        {
            this.isHorizontallyUniform = isHorizontallyUniform;
            this.isVerticallyUniform = isVerticallyUniform;
        }

        public override string ProcessName
        {
            get { return "TestProcess"; }
        }

        public override string VariableName
        {
            get { return "TestVariable"; }
        }

        public override string VariableDescription
        {
            get { return "Variable for testing"; }
        }

        public override bool IsHorizontallyUniform
        {
            get { return isHorizontallyUniform; }
        }

        public override bool IsVerticallyUniform
        {
            get { return isVerticallyUniform; }
        }

        public override IUnit VariableUnit
        {
            get { return new Unit("Becquerel per nanogauss"); }
        }

        public override int VariableDimension
        {
            get { return 1; }
        }
    }

    [TestFixture]
    public class BoundaryConditionTest
    {
        [Test]
        public void AddDataPointShouldAddDataAndLayer()
        {
            var feature2D = new Feature2D
                {
                    Geometry =
                        new LineString(new []
                            {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0)})
                };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
                {
                    Feature = feature2D
                };

            boundaryCondition.DataPointIndices.Add(2);

            Assert.AreEqual(1, boundaryCondition.PointData.Count);
            Assert.AreEqual(1, boundaryCondition.PointDepthLayerDefinitions.Count);
        }

        [Test]
        public void RemoveDataPointShouldRemoveDataAndLayer()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = feature2D
            };

            boundaryCondition.DataPointIndices.Add(1);
            boundaryCondition.DataPointIndices.Remove(3);
            boundaryCondition.DataPointIndices.Remove(1);

            Assert.AreEqual(0, boundaryCondition.PointData.Count);
            Assert.AreEqual(0, boundaryCondition.PointDepthLayerDefinitions.Count);
        }

        [Test]
        public void ReplaceDataPoint()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = feature2D
            };

            boundaryCondition.DataPointIndices.Add(1);

            Assert.Throws<ArgumentException>(() => boundaryCondition.DataPointIndices.Add(4));
            Assert.Throws<ArgumentException>(() => boundaryCondition.DataPointIndices.Add(1));
        }

        [Test]
        public void AddingDepthLayerShouldKeepData()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = feature2D
            };

            boundaryCondition.AddPoint(1);
            Assert.AreEqual(VerticalProfileType.Uniform,
                            boundaryCondition.GetDepthLayerDefinitionAtPoint(1).Type);
            
            var data = boundaryCondition.GetDataAtPoint(1);
            data[new DateTime(2000, 1, 1)] = 1.0;
            data[new DateTime(2000, 1, 2)] = 2.0;
            data[new DateTime(2000, 1, 3)] = 3.0;

            boundaryCondition.PointDepthLayerDefinitions[0] =
                new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 10, 20, 30, 40);
            Assert.AreEqual(4, data.Components.Count);
        }

        [Test]
        public void RemovingDepthLayerShouldKeepData()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = feature2D
            };

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointDepthLayerDefinitions[0] =
                new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 10, 20, 30, 40);
            var data = boundaryCondition.GetDataAtPoint(1);
            data[new DateTime(2000, 1, 1)] = new[] {1.0, 2.0, 3.0, 4.0};

            boundaryCondition.PointDepthLayerDefinitions[0] =
                new VerticalProfileDefinition(VerticalProfileType.ZFromSurface, -3, -5);

            Assert.AreEqual(2, data.Components.Count);
            Assert.AreEqual(new[] {1.0}, data.Components[0].Values);
            Assert.AreEqual(new[] {2.0}, data.Components[1].Values);
        }

        [Test]
        public void TranslatingGeometryShouldKeepData()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = feature2D
            };

            boundaryCondition.AddPoint(1);
            var data = boundaryCondition.GetDataAtPoint(1);
            data[new DateTime(2000, 1, 1)] = new[] { 1.0 };

            feature2D.Geometry = new LineString(new [] { new Coordinate(0, 1), new Coordinate(1, 1), new Coordinate(2, 1) });

            Assert.AreEqual(1, boundaryCondition.PointData.Count);
            Assert.AreEqual(new[] {new DateTime(2000, 1, 1)}, boundaryCondition.PointData[0].Arguments[0].Values);
        }

        [Test]
        public void AddingCoordinateShouldKeepData()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = feature2D
            };

            boundaryCondition.AddPoint(0);
            var dataAtZero = boundaryCondition.GetDataAtPoint(0);
            dataAtZero[new DateTime(2000, 1, 1)] = new[] { 0.0 };

            boundaryCondition.AddPoint(1);
            var dataAtOne = boundaryCondition.GetDataAtPoint(1);
            dataAtOne[new DateTime(2000, 1, 1)] = new[] { 1.0 };

            feature2D.Geometry =
                new LineString(new []
                    {new Coordinate(0, 0), new Coordinate(0.5, 0), new Coordinate(1, 0), new Coordinate(2, 0)});

            Assert.AreEqual(2, boundaryCondition.PointData.Count);
            Assert.AreEqual(dataAtZero, boundaryCondition.GetDataAtPoint(0));
            Assert.AreEqual(dataAtOne, boundaryCondition.GetDataAtPoint(2));
        }
        
        [Test]
        public void RemovingCoordinateShouldKeepData()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = feature2D
            };

            boundaryCondition.AddPoint(0);
            var dataAtZero = boundaryCondition.GetDataAtPoint(0);
            dataAtZero[new DateTime(2000, 1, 1)] = new[] { 0.0 };

            boundaryCondition.AddPoint(1);
            var dataAtOne = boundaryCondition.GetDataAtPoint(1);
            dataAtOne[new DateTime(2000, 1, 1)] = new[] { 1.0 };

            feature2D.Geometry =
                new LineString(new [] { new Coordinate(1, 0), new Coordinate(2, 0) });

            Assert.AreEqual(1, boundaryCondition.PointData.Count);
            Assert.AreEqual(dataAtOne, boundaryCondition.GetDataAtPoint(0));
        }

        [Test]
        public void SwitchingBetweenDataTypesShouldNotGiveException()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = feature2D
            };

            boundaryCondition.isHorizontallyUniform = true;
            boundaryCondition.DataType = BoundaryConditionDataType.Harmonics;

            Assert.AreEqual(1, boundaryCondition.DataPointIndices.Count);
            Assert.AreEqual(1, boundaryCondition.PointData.Count);
            Assert.AreEqual(1, boundaryCondition.PointDepthLayerDefinitions.Count);

            boundaryCondition.isHorizontallyUniform = false;
            boundaryCondition.DataType = BoundaryConditionDataType.TimeSeries;

            Assert.AreEqual(1, boundaryCondition.DataPointIndices.Count);
            Assert.AreEqual(1, boundaryCondition.PointData.Count);
            Assert.AreEqual(1, boundaryCondition.PointDepthLayerDefinitions.Count);
        }

        [Test]
        public void RenameBoundaryShouldRenameDefaultSupportPointNames()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0)}),
                Name = "aap"
            };

            var boundaryConditionSet = new BoundaryConditionSet {Feature = feature2D};

            Assert.AreEqual("aap_0001", boundaryConditionSet.SupportPointNames[0]);
            Assert.AreEqual("aap_0002", boundaryConditionSet.SupportPointNames[1]);
            Assert.AreEqual("aap_0003", boundaryConditionSet.SupportPointNames[2]);

            boundaryConditionSet.SupportPointNames[1] = "noot";
            feature2D.Name = "mies";

            Assert.AreEqual("mies_0001", boundaryConditionSet.SupportPointNames[0]);
            Assert.AreEqual("noot", boundaryConditionSet.SupportPointNames[1]);
            Assert.AreEqual("mies_0003", boundaryConditionSet.SupportPointNames[2]);
        }

        [Test]
        public void GetDataPoint_WhenNoDataForPoint_NoExceptionIsGivenButReturnsNull()
        {
            var boundaryCondition = new TestBoundaryCondition(BoundaryConditionDataType.TimeSeries, false, false)
            {
                Feature = new Feature2D {Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0)})}
            };

            boundaryCondition.AddPoint(0);
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] {DateTime.Now});
            boundaryCondition.PointData[0].Components[0].SetValues(new List<double>() {0});

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData.RemoveAt(1);

            var data = boundaryCondition.GetDataAtPoint(0);
            Assert.NotNull(data);

            Assert.DoesNotThrow(() => { data = boundaryCondition.GetDataAtPoint(1); });
            Assert.IsNull(data);
        }
    }
}

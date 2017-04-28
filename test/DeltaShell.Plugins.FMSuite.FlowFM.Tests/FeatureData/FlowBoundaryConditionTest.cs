using System;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class FlowBoundaryConditionTest
    {
        private static BoundaryConditionDataType DetermineDataType(IFunction function)
        {
            if (function.Arguments.Count == 0)
            {
                return BoundaryConditionDataType.Constant;
            }
            var argument = function.Arguments.Last();
            if (argument.ValueType == typeof (DateTime))
            {
                return BoundaryConditionDataType.TimeSeries;
            }
            if (argument.ValueType == typeof (string))
            {
                return function.Components.Count == 4
                           ? BoundaryConditionDataType.AstroCorrection
                           : BoundaryConditionDataType.AstroComponents;
            }
            if (argument.ValueType == typeof (double))
            {
                if (function.Components.Count == 1)
                {
                    return BoundaryConditionDataType.Qh;
                }
                else
                {
                    return function.Components.Count == 4
                           ? BoundaryConditionDataType.HarmonicCorrection
                           : BoundaryConditionDataType.Harmonics;
                }
            }
            throw new ArgumentException("Unable to determine data type of given boundary condition data.");
        }

        [Test]
        public void TestCurrentsSupportedForcingAndInterpolationTypes()
        {
            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, BoundaryConditionDataType.TimeSeries);

            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Logarithmic));
            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Linear));
            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Step));
            Assert.IsFalse(data.SupportsReflection); // not supported yet by kernel.
            Assert.IsFalse(data.IsHorizontallyUniform);
        }

        [Test]
        public void TestRiemannSupportedForcingAndInterpolationTypes()
        {
            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.TimeSeries);

            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Uniform));
            Assert.IsFalse(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Logarithmic));
            Assert.IsFalse(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Linear));
            Assert.IsFalse(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Step));
            Assert.IsFalse(data.SupportsReflection);
            Assert.IsFalse(data.IsHorizontallyUniform);
        }

        [Test]
        public void DataSyncingWithDataPointAdded()
        {
            var feature2D = new Feature2D
                {
                    Geometry =
                        new LineString(new []
                            {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0)})
                };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, BoundaryConditionDataType.TimeSeries)
                {
                    Feature = feature2D
                };
            
            data.DataPointIndices.Add(2);
            
            Assert.AreEqual(1, data.PointData.Count);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, DetermineDataType(data.PointData[0]));
            Assert.IsNull(data.GetDataAtPoint(0));
            Assert.IsNotNull(data.GetDataAtPoint(2));
            Assert.AreEqual(1, data.PointDepthLayerDefinitions.Count);
            Assert.AreEqual(VerticalProfileType.Uniform, data.PointDepthLayerDefinitions[0].Type);
        }

        [Test]
        public void DataSyncingWithDataPointRemoved()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature2D
            };

            data.AddPoint(0);
            data.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 30,
                                                                               40, 30);
            data.AddPoint(2);
            data.DataPointIndices.Remove(2);

            Assert.AreEqual(1, data.PointData.Count);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, DetermineDataType(data.PointData[0]));
            Assert.IsNull(data.GetDataAtPoint(2));
            Assert.IsNotNull(data.GetDataAtPoint(0));
            Assert.AreEqual(1, data.PointDepthLayerDefinitions.Count);
            Assert.AreEqual(VerticalProfileType.PercentageFromBed,data.PointDepthLayerDefinitions[0].Type);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SettingMultipleLayersForWaterLevelGivesException()
        {
            var feature2D = new Feature2D
                {
                    Geometry =
                        new LineString(new []
                            {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0)})
                };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                 BoundaryConditionDataType.TimeSeries)
                {
                    Feature = feature2D
                };

            data.AddPoint(0);

            data.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 30,
                                                                               40, 30);
        }

        [Test]
        public void CheckKeepTopLayer()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                 BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature2D
            };
            data.AddPoint(0);
            data.AddPoint(1);
            data.AddPoint(2);

            data.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.Uniform);
            data.PointDepthLayerDefinitions[1] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 30,
                                                                               70);
            data.PointDepthLayerDefinitions[2] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 30,
                                                                               40, 30);
            
            var function1 = data.GetDataAtPoint(0);
            function1[new DateTime(2000, 1, 1)] = 20.00;
            
            var function2 = data.GetDataAtPoint(1);            
            function2[new DateTime(2001, 1, 1)] = new[] {20.01, 40.02};
            function2[new DateTime(2002, 1, 1)] = new[] {20.02, 40.04};
            
            var function3 = data.GetDataAtPoint(2);
            function3[new DateTime(2003, 1, 1)] = new[] {20.03, 40.06, 60.09};

            data.KeepTopLayer();

            function1 = data.GetDataAtPoint(0);
            Assert.AreEqual(20.00, function1[new DateTime(2000, 1, 1)]);
            
            function2 = data.GetDataAtPoint(1);
            Assert.AreEqual(20.01, function2[new DateTime(2001, 1, 1)]);
            Assert.AreEqual(20.02, function2[new DateTime(2002, 1, 1)]);

            function3 = data.GetDataAtPoint(2);
            Assert.AreEqual(20.03, function3[new DateTime(2003, 1, 1)]);
        }


        [Test]
        public void CheckKeepBottomLayer()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                 BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature2D
            };
            data.AddPoint(0);
            data.AddPoint(1);
            data.AddPoint(2);

            data.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.Uniform);
            data.PointDepthLayerDefinitions[1] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 30,
                                                                               70);
            data.PointDepthLayerDefinitions[2] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 30,
                                                                               40, 30);
            
            var function1 = data.GetDataAtPoint(0);
            function1[new DateTime(2000, 1, 1)] = 20.00;

            var function2 = data.GetDataAtPoint(1);
            function2[new DateTime(2001, 1, 1)] = new[] { 20.01, 40.02 };
            function2[new DateTime(2002, 1, 1)] = new[] { 20.02, 40.04 };

            var function3 = data.GetDataAtPoint(2);
            function3[new DateTime(2003, 1, 1)] = new[] { 20.03, 40.06, 60.09 };

            data.KeepBottomLayer();

            function1 = data.GetDataAtPoint(0);
            Assert.AreEqual(20.00, function1[new DateTime(2000, 1, 1)]);

            function2 = data.GetDataAtPoint(1);
            Assert.AreEqual(40.02, function2[new DateTime(2001, 1, 1)]);
            Assert.AreEqual(40.04, function2[new DateTime(2002, 1, 1)]);

            function3 = data.GetDataAtPoint(2);
            Assert.AreEqual(40.06, function3[new DateTime(2003, 1, 1)]);
        }
    }
}

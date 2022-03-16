using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Units;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class FlowBoundaryConditionTest
    {
        [Test]
        public void GetFlowBoundaryReflectionUnitTest()
        {
            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries);
            Assert.AreEqual("", bc1.ReflectionUnit.Name);
            Assert.AreEqual("s²", bc1.ReflectionUnit.Symbol);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.TimeSeries);
            Assert.AreEqual("time", bc2.ReflectionUnit.Name);
            Assert.AreEqual("s", bc2.ReflectionUnit.Symbol);

            var bc3 = new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries);
            try
            {
                IUnit _ = bc3.ReflectionUnit;
                Assert.Fail("Get reflection unit should have thrown an exception");
            }
            catch (ArgumentOutOfRangeException)
            {
                // this should be hit, everything is okay.
            }
            catch (Exception e)
            {
                Assert.Fail("Not the right kind of exception {0}", e);
            }
        }

        [Test]
        public void RemoveSedimentFractionFromNullFunctionTest()
        {
            var flowCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries);
            try
            {
                flowCondition.RemoveSedimentFractionFromFunction(null, "TestFraction");
            }
            catch (Exception e)
            {
                Assert.Fail("Removing sediment franction from null function should not fail. {0}", e);
            }
        }

        [Test]
        public void RemoveSedimentFractionFromFunctionTest()
        {
            var flowCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries)
            {
                Feature = new Feature2D
                {
                    Name = "bnd1",
                    Geometry =
                        new LineString(new[]
                        {
                            new Coordinate(0, 0),
                            new Coordinate(0, 10)
                        })
                }
            };
            flowCondition.SedimentFractionNames = new List<string>() {"TestFraction"};
            flowCondition.AddPoint(0);
            var startDateTime = new DateTime(1981, 8, 30);
            var endDateTime = new DateTime(1981, 8, 31);
            flowCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                startDateTime,
                endDateTime
            });
            flowCondition.PointData[0][startDateTime] = 0.5;
            flowCondition.PointData[0][endDateTime] = 0.6;

            try
            {
                flowCondition.RemoveSedimentFractionFromFunction(flowCondition.PointData[0], "TestFraction");
            }
            catch (Exception e)
            {
                Assert.Fail("Removing sediment franction from null function should not fail. {0}", e);
            }
        }

        [Test]
        public void GetSupporteDataTypesForQuantityIsEmptyForNoBedLevelAndBedLevelFixed()
        {
            var expectedSupportedDataTypes = new List<BoundaryConditionDataType>() {BoundaryConditionDataType.Empty};

            Assert.AreEqual(expectedSupportedDataTypes, FlowBoundaryCondition.GetSupportedDataTypesForQuantity(FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint));
            Assert.AreEqual(expectedSupportedDataTypes, FlowBoundaryCondition.GetSupportedDataTypesForQuantity(FlowBoundaryQuantityType.MorphologyBedLevelFixed));
        }

        [Test]
        public void TestCurrentsSupportedForcingAndInterpolationTypes()
        {
            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                 BoundaryConditionDataType.TimeSeries);

            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Logarithmic));
            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Linear));
            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Step));
            Assert.IsFalse(data.SupportsReflection); // not supported yet by kernel.
            Assert.IsFalse(data.IsHorizontallyUniform);
        }

        [Test]
        public void TestRiemannSupportedForcingAndInterpolationTypes()
        {
            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Riemann,
                                                 BoundaryConditionDataType.TimeSeries);

            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Uniform));
            Assert.IsFalse(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Logarithmic));
            Assert.IsFalse(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Linear));
            Assert.IsFalse(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Step));
            Assert.IsFalse(data.SupportsReflection);
            Assert.IsFalse(data.IsHorizontallyUniform);
        }

        [Test]
        public void TestSedimentSupportedForcingAndInterpolationTypes()
        {
            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration,
                                                 BoundaryConditionDataType.TimeSeries);

            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Logarithmic));
            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Linear));
            Assert.IsTrue(data.SupportedVerticalInterpolationTypes.Contains(VerticalInterpolationType.Step));
            Assert.IsFalse(data.SupportsReflection); // not supported yet by kernel.
            Assert.IsFalse(data.IsHorizontallyUniform);
        }

        [Test]
        public void DataSyncingWithDataPointAdded()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                 BoundaryConditionDataType.TimeSeries) {Feature = feature2D};

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
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                 BoundaryConditionDataType.TimeSeries) {Feature = feature2D};

            data.AddPoint(0);
            data.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed,
                                                                               30,
                                                                               40, 30);
            data.AddPoint(2);
            data.DataPointIndices.Remove(2);

            Assert.AreEqual(1, data.PointData.Count);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, DetermineDataType(data.PointData[0]));
            Assert.IsNull(data.GetDataAtPoint(2));
            Assert.IsNotNull(data.GetDataAtPoint(0));
            Assert.AreEqual(1, data.PointDepthLayerDefinitions.Count);
            Assert.AreEqual(VerticalProfileType.PercentageFromBed, data.PointDepthLayerDefinitions[0].Type);
        }

        [Test]
        public void SettingMultipleLayersForWaterLevelGivesException()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                 BoundaryConditionDataType.TimeSeries) {Feature = feature2D};

            data.AddPoint(0);

            Assert.That(() => data.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed,
                                                                                                 30,
                                                                                                 40, 30),
                        Throws.ArgumentException);
        }

        [Test]
        public void CheckKeepTopLayer()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                 BoundaryConditionDataType.TimeSeries) {Feature = feature2D};
            data.AddPoint(0);
            data.AddPoint(1);
            data.AddPoint(2);

            data.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.Uniform);
            data.PointDepthLayerDefinitions[1] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed,
                                                                               30,
                                                                               70);
            data.PointDepthLayerDefinitions[2] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed,
                                                                               30,
                                                                               40, 30);

            IFunction function1 = data.GetDataAtPoint(0);
            function1[new DateTime(2000, 1, 1)] = 20.00;

            IFunction function2 = data.GetDataAtPoint(1);
            function2[new DateTime(2001, 1, 1)] = new[]
            {
                20.01,
                40.02
            };
            function2[new DateTime(2002, 1, 1)] = new[]
            {
                20.02,
                40.04
            };

            IFunction function3 = data.GetDataAtPoint(2);
            function3[new DateTime(2003, 1, 1)] = new[]
            {
                20.03,
                40.06,
                60.09
            };

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
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                 BoundaryConditionDataType.TimeSeries) {Feature = feature2D};
            data.AddPoint(0);
            data.AddPoint(1);
            data.AddPoint(2);

            data.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.Uniform);
            data.PointDepthLayerDefinitions[1] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed,
                                                                               30,
                                                                               70);
            data.PointDepthLayerDefinitions[2] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed,
                                                                               30,
                                                                               40, 30);

            IFunction function1 = data.GetDataAtPoint(0);
            function1[new DateTime(2000, 1, 1)] = 20.00;

            IFunction function2 = data.GetDataAtPoint(1);
            function2[new DateTime(2001, 1, 1)] = new[]
            {
                20.01,
                40.02
            };
            function2[new DateTime(2002, 1, 1)] = new[]
            {
                20.02,
                40.04
            };

            IFunction function3 = data.GetDataAtPoint(2);
            function3[new DateTime(2003, 1, 1)] = new[]
            {
                20.03,
                40.06,
                60.09
            };

            data.KeepBottomLayer();

            function1 = data.GetDataAtPoint(0);
            Assert.AreEqual(20.00, function1[new DateTime(2000, 1, 1)]);

            function2 = data.GetDataAtPoint(1);
            Assert.AreEqual(40.02, function2[new DateTime(2001, 1, 1)]);
            Assert.AreEqual(40.04, function2[new DateTime(2002, 1, 1)]);

            function3 = data.GetDataAtPoint(2);
            Assert.AreEqual(40.06, function3[new DateTime(2003, 1, 1)]);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void MultipleFractionsInChart()
        {
            var feature2D = new Feature2D
            {
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var data = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport,
                                                 BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature2D,
                SedimentFractionNames = new List<string>
                {
                    "abc",
                    "def"
                }
            };

            var function = TypeUtils.CallPrivateMethod<IFunction>(data, "CreateFunction");
            var view = new FunctionView {Data = function};

            function[DateTime.Now] = new[]
            {
                1.0,
                2.0
            };
            function[DateTime.Now.AddHours(1)] = new[]
            {
                2.0,
                3.0
            };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void MorphologyBoundaryConditionHasGeneratedDataTest()
        {
            var condition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed,
                                                      BoundaryConditionDataType.TimeSeries)
            {
                Feature = new Feature2D
                {
                    Name = "bnd1",
                    Geometry =
                        new LineString(new[]
                        {
                            new Coordinate(0, 0),
                            new Coordinate(0, 10)
                        })
                }
            };
            Assert.IsFalse(FlowBoundaryCondition.MorphologyBoundaryConditionHasGeneratedData(condition));
            condition.AddPoint(0);
            var startDateTime = new DateTime(1981, 8, 30);
            var endDateTime = new DateTime(1981, 8, 31);
            condition.PointData[0].Arguments[0].SetValues(new[]
            {
                startDateTime,
                endDateTime
            });
            condition.PointData[0][startDateTime] = 0.5;
            condition.PointData[0][endDateTime] = 0.6;
            Assert.IsTrue(FlowBoundaryCondition.MorphologyBoundaryConditionHasGeneratedData(condition));
            TypeUtils.SetPrivatePropertyValue(condition, "FlowQuantity", FlowBoundaryQuantityType.Discharge);
            Assert.IsFalse(FlowBoundaryCondition.MorphologyBoundaryConditionHasGeneratedData(condition));
            TypeUtils.SetPrivatePropertyValue(condition, "FlowQuantity",
                                              FlowBoundaryQuantityType.MorphologyBedLevelFixed);
            Assert.IsTrue(FlowBoundaryCondition.MorphologyBoundaryConditionHasGeneratedData(condition));
            condition.DataType = BoundaryConditionDataType.Empty;
            Assert.IsFalse(FlowBoundaryCondition.MorphologyBoundaryConditionHasGeneratedData(condition));
        }

        [Test]
        [TestCase(FlowBoundaryQuantityType.Riemann, "meters", "m")]
        public void VariableUnit_WithVariousFlowBoundaryType_ReturnsExpectedUnit(FlowBoundaryQuantityType quantityType,
                                                                                 string unitDescription,
                                                                                 string unit)
        {
            // Setup
            var random = new Random(21);
            var condition = new FlowBoundaryCondition(quantityType,
                                                      random.NextEnumValue<BoundaryConditionDataType>());

            // Call
            IUnit variableUnit = condition.VariableUnit;

            // Assert
            Assert.That(variableUnit, Is.TypeOf<Unit>());
            Assert.That(variableUnit.Name, Is.EqualTo(unitDescription));
            Assert.That(variableUnit.Symbol, Is.EqualTo(unit));
        }

        private static BoundaryConditionDataType DetermineDataType(IFunction function)
        {
            if (function.Arguments.Count == 0)
            {
                return BoundaryConditionDataType.Constant;
            }

            IVariable argument = function.Arguments.Last();
            if (argument.ValueType == typeof(DateTime))
            {
                return BoundaryConditionDataType.TimeSeries;
            }

            if (argument.ValueType == typeof(string))
            {
                return function.Components.Count == 4
                           ? BoundaryConditionDataType.AstroCorrection
                           : BoundaryConditionDataType.AstroComponents;
            }

            if (argument.ValueType == typeof(double))
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

        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, "meters", "m")]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, "meters", "m")]
        [TestCase(FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint, "", "-")]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelFixed, "", "-")]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLoadTransport, "cubic meters per second per meter", "m3/s/m")]
        public void MorphologyVariableUnitTest(FlowBoundaryQuantityType type, string name_expectation, string symbol_expectation)
        {
            IUnit unit = new FlowBoundaryCondition(type, BoundaryConditionDataType.TimeSeries).VariableUnit;
            Assert.That(unit.Name, Is.EqualTo(name_expectation));
            Assert.That(unit.Symbol, Is.EqualTo(symbol_expectation));
        }
    }
}
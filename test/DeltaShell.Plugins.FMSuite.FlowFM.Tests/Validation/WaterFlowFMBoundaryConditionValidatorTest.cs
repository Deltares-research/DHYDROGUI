using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMBoundaryConditionValidatorTest
    {
        [Test]
        public void TestEmptyBoundaryConditionSet()
        {
            WaterFlowFMModel model = CreateValidModel();

            model.Boundaries.Add(new Feature2D {Name = "myBoundary"});

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.IsTrue(report.ContainsError("Boundary 'myBoundary' does not contain a boundary condition"));
        }

        [Test]
        public void TestEmptyBoundaryCondition()
        {
            WaterFlowFMModel model = CreateValidModel();

            var boundary = new Feature2D {Name = "myBoundary"};
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.That(report.ContainsError("No data defined for boundary condition 'Water level' at boundary 'myBoundary'"));
        }

        [Test]
        public void TestEmptyBoundaryConditionTimeSeries()
        {
            WaterFlowFMModel model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            model.BoundaryConditions.First().AddPoint(1);

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        public void TestTooShortBoundaryConditionTimeSeries()
        {
            WaterFlowFMModel model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            model.BoundaryConditions.First().AddPoint(1);

            IFunction timeSeries = model.BoundaryConditions.First().GetDataAtPoint(1);
            timeSeries[model.StartTime + new TimeSpan(0, 1, 0, 0)] = 0.5;
            timeSeries[model.StopTime] = 0.5;

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        [TestCaseSource(nameof(TimeSeriesSpansModelTimeSeriesWithTimeZones))]
        public void GivenTimeSeriesThatSpansModelTimeSeriesWithTimeZones_whenValidating_ThenReturnNoErrors(TimeSpan modelTimeZone, TimeSpan boundaryConditionTimeZone)
        {
            WaterFlowFMModel model = CreateModelWithTimeZones(boundaryConditionTimeZone, modelTimeZone);

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        [TestCaseSource(nameof(TimeSeriesDoesNotSpanModelTimeSeriesWithTimeZones))]
        public void GivenTimeSeriesThatDoesNotSpanModelTimeSeriesWithTimeZones_whenValidating_ThenReturnOneError(TimeSpan modelTimeZone, TimeSpan boundaryConditionTimeZone)
        {
            WaterFlowFMModel model = CreateModelWithTimeZones(boundaryConditionTimeZone, modelTimeZone);

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            string expectedMessage = string.Format(
                "Time series does not span model run interval for {0} at point {1}.",
                model.BoundaryConditions.First().VariableDescription, "boundary_0002");

            Assert.AreEqual(1, report.ErrorCount);
            Assert.AreEqual(report.Issues.First().Message, expectedMessage);
        }

        private static WaterFlowFMModel CreateModelWithTimeZones(TimeSpan boundaryConditionTimeZone, TimeSpan modelTimeZone)
        {
            WaterFlowFMModel model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            model.Boundaries.Add(boundary);

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries) { Feature = boundary };

            model.BoundaryConditionSets[0].BoundaryConditions.Add(flowBoundaryCondition);
            model.BoundaryConditions.First().AddPoint(1);
            flowBoundaryCondition.TimeZone = boundaryConditionTimeZone;
            SetModelTimeZone(model, modelTimeZone);

            IFunction timeSeries = model.BoundaryConditions.First().GetDataAtPoint(1);
            timeSeries[model.StartTime] = 0.5;
            timeSeries[model.StopTime] = 0.5;
            return model;
        }

        private static void SetModelTimeZone(WaterFlowFMModel model, TimeSpan modelTimeZone)
        {
            model.ModelDefinition.GetModelProperty(KnownProperties.TZone).Value = modelTimeZone.TotalHours;
        }

        [Test]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLoadTransport)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void TestMorphologyBoundaryConditionOnlyWithOneTimeSeries(FlowBoundaryQuantityType quantityType)
        {
            WaterFlowFMModel model = CreateValidModel();

            model.ModelDefinition.UseMorphologySediment = true;
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction {Name = "testFrac"});

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries) {Feature = boundary};
            flowBoundaryCondition.DataPointIndices.Add(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                model.StartTime,
                model.StopTime
            });

            var morphologyBoundaryCondition = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> {"testFrac"}
            };

            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                morphologyBoundaryCondition,
                flowBoundaryCondition
            });

            morphologyBoundaryCondition.AddPoint(1);
            IFunction timeSeriesP1 = morphologyBoundaryCondition.GetDataAtPoint(1);

            timeSeriesP1[model.StartTime] = 0.5;
            timeSeriesP1[model.StopTime] = 0.5;

            /* Check everything went alright just with one data point */
            Assert.AreEqual(1, morphologyBoundaryCondition.PointData.Count);
            Assert.IsNotNull(morphologyBoundaryCondition.GetDataAtPoint(0)); // data for morphology is on all data points the same (Horizontally Uniform)
            Assert.IsNotNull(morphologyBoundaryCondition.GetDataAtPoint(1));

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);

            /* Now add a second data point and expect the validation to fail.*/
            morphologyBoundaryCondition.AddPoint(0);
            IFunction timeSeriesP0 = morphologyBoundaryCondition.GetDataAtPoint(0);
            timeSeriesP0[model.StartTime] = 0.5;
            timeSeriesP0[model.StopTime] = 0.5;

            Assert.AreEqual(2, morphologyBoundaryCondition.PointData.Count);
            Assert.IsNotNull(morphologyBoundaryCondition.GetDataAtPoint(0));
            Assert.IsNotNull(morphologyBoundaryCondition.GetDataAtPoint(1));

            report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLoadTransport)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void TestMorphologyBoundaryConditionWithEmptyTimeSeriesIsValid(FlowBoundaryQuantityType quantityType)
        {
            WaterFlowFMModel model = CreateValidMorphologyModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };

            var morphologyBoundary = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> {"testFrac"}
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries) {Feature = boundary};
            AddPointDataToBoundaryCondition(flowBoundaryCondition, model);

            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                morphologyBoundary,
                flowBoundaryCondition
            });

            morphologyBoundary.AddPoint(1);
            IFunction timeSeriesP1 = morphologyBoundary.GetDataAtPoint(1);

            timeSeriesP1[model.StartTime] = 0.5;
            timeSeriesP1[model.StopTime] = 0.5;

            /* Check everything went alright just with one data point */
            Assert.AreEqual(1, morphologyBoundary.PointData.Count);
            Assert.IsNotNull(morphologyBoundary.GetDataAtPoint(0));
            Assert.IsNotNull(morphologyBoundary.GetDataAtPoint(1));

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);

            /* Now add a second data point and expect the validation not to fail.*/
            morphologyBoundary.AddPoint(0);

            Assert.AreEqual(2, morphologyBoundary.PointData.Count);
            Assert.IsNotNull(morphologyBoundary.GetDataAtPoint(0));
            Assert.IsNotNull(morphologyBoundary.GetDataAtPoint(1));

            report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLoadTransport)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void TestMorphologyBoundaryConditionOnlyAllowsOneCondition(FlowBoundaryQuantityType quantityType)
        {
            WaterFlowFMModel model = CreateValidMorphologyModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };

            model.Boundaries.Add(boundary);
            IEventedList<IBoundaryCondition> bcSet = model.BoundaryConditionSets[0].BoundaryConditions;

            var morphologyBoundaryCondition = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> {"testFrac"}
            };
            AddPointDataToBoundaryCondition(morphologyBoundaryCondition, model);

            var flowBoundaryCondition1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries) {Feature = boundary};
            AddPointDataToBoundaryCondition(flowBoundaryCondition1, model);

            bcSet.AddRange(new[]
            {
                morphologyBoundaryCondition,
                flowBoundaryCondition1
            });

            Assert.AreEqual(2, bcSet.Count);
            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);

            /* Add a second condition */
            var flowBoundaryCondition2 = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> {"testFrac"}
            };
            AddPointDataToBoundaryCondition(flowBoundaryCondition2, model);
            bcSet.Add(flowBoundaryCondition2);

            Assert.AreEqual(3, bcSet.Count);
            report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        public void TestValidBoundaryConditionTimeSeries()
        {
            WaterFlowFMModel model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            model.BoundaryConditions.First().AddPoint(1);

            IFunction timeSeries = model.BoundaryConditions.First().GetDataAtPoint(1);

            timeSeries[model.StartTime] = 0.5;
            timeSeries[model.StopTime] = 0.5;

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        public void TestInvalidBoundaryConditionCombination()
        {
            WaterFlowFMModel model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                                          BoundaryConditionDataType.TimeSeries) {Feature = boundary});

            List<IBoundaryCondition> boundaryConditions = model.BoundaryConditions.ToList();

            boundaryConditions[0].AddPoint(1);
            boundaryConditions[1].AddPoint(1);

            IFunction waterLevelTimeSeries = boundaryConditions[0].GetDataAtPoint(1);

            waterLevelTimeSeries[model.StartTime] = 0.5;
            waterLevelTimeSeries[model.StopTime] = 0.5;

            IFunction dischargeTimeSeries = boundaryConditions[1].GetDataAtPoint(1);

            dischargeTimeSeries[model.StartTime] = 1.5;
            dischargeTimeSeries[model.StopTime] = 1.5;

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        public void TestMorphologyBoundaryConditionWithoutHydroBoundaryConditionShouldCreateValidationError()
        {
            WaterFlowFMModel model = CreateValidMorphologyModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };

            var morphologyBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> {"testFrac"}
            };
            AddPointDataToBoundaryCondition(morphologyBoundaryCondition, model);

            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                morphologyBoundaryCondition
            });

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First(i => i.Severity == ValidationSeverity.Error).Message,
                        Is.EqualTo(Resources.WaterFlowFMBoundaryConditionValidator_ValidateMorphologyBoundaryHaveHydroBoundaries_Morphology_boundary_condition_must_have_a_Hydro_boundary_condition_));
        }

        [Test]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLoadTransport)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void MorphologyBoundaryConditionCannotHaveMoreThanOnePointWithGeneratedDataTest(FlowBoundaryQuantityType quantityType)
        {
            WaterFlowFMModel model = CreateValidMorphologyModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            model.Boundaries.Add(boundary);

            IEventedList<IBoundaryCondition> bcSet = model.BoundaryConditionSets[0].BoundaryConditions;
            var morBC = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> {"testFrac"}
            };
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries) {Feature = boundary};
            AddPointDataToBoundaryCondition(flowBoundaryCondition, model);

            bcSet.Add(morBC);
            bcSet.Add(flowBoundaryCondition);

            model.BoundaryConditions.First().AddPoint(0);
            model.BoundaryConditions.First().AddPoint(1);
            Assert.AreEqual(2, morBC.PointData.Count);

            /* If we add values to a second point it should fail*/
            var startDateTime = new DateTime(1981, 8, 30);
            var endDateTime = new DateTime(1981, 8, 31);

            /* Add values to the first and second point */
            morBC.PointData[0].Arguments[0].SetValues(new[]
            {
                startDateTime,
                endDateTime
            });
            morBC.PointData[0][startDateTime] = 0.5;
            morBC.PointData[0][endDateTime] = 0.6;

            morBC.PointData[1].Arguments[0].SetValues(new[]
            {
                startDateTime,
                endDateTime
            });
            morBC.PointData[1][startDateTime] = 0.5;
            morBC.PointData[1][endDateTime] = 0.6;

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First(i => i.Severity == ValidationSeverity.Error).Message,
                        Is.EqualTo(Resources.WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_A_morphology_boundary_condition_cannot_have_more_than_one_point_with_generated_data_));
        }

        [Test]
        public void TimeSeriesCannotContainNegativeValuesForStrictlyPositiveBoundaryConditionsTest()
        {
            WaterFlowFMModel model = CreateValidMorphologyModel();
            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            model.Boundaries.Add(boundary);

            var bc = new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string>() {"testFrac"}
            };

            model.BoundaryConditionSets[0].BoundaryConditions.Add(bc);
            model.BoundaryConditions.First().AddPoint(1);

            /* Set negative values for a 'strictly positive boundary' */
            IFunction timeSeries = model.BoundaryConditions.First().GetDataAtPoint(1);
            timeSeries[model.StartTime] = -0.5;
            timeSeries[model.StopTime] = -0.5;

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First(i => i.Severity == ValidationSeverity.Error).Message,
                        Is.EqualTo(string.Format(
                                       Resources.WaterFlowFMBoundaryConditionValidator_ValidateBoundaryConditionPointIndex_Time_series_contains_forbidden_negative_values_for__0__at_point__1_,
                                       bc.VariableDescription, model.BoundaryConditionSets[0].SupportPointNames[1])));
        }

        [Test]
        public void CustomSupportNamesNotSupportedByKernelTest()
        {
            WaterFlowFMModel model = CreateValidModel();
            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            /* Replace the name of a given support point */
            string expectedName = model.BoundaryConditionSets[0].SupportPointNames[0];
            model.BoundaryConditionSets[0].SupportPointNames[0] = "ThisIsNotAValidName";

            model.BoundaryConditions.First().AddPoint(0);
            IFunction timeSeries = model.BoundaryConditions.First().GetDataAtPoint(0);
            timeSeries[model.StartTime] = 0.5;
            timeSeries[model.StopTime] = 0.5;

            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First(i => i.Severity == ValidationSeverity.Error).Message,
                        Is.EqualTo(string.Format(
                                       Resources.WaterFlowFMBoundaryConditionValidator_ValidateSupportPointNames_Custom_support_point_name__0__is_not_yet_supported_by_the_dflow_fm_kernel__please_change_it_to__1_,
                                       model.BoundaryConditionSets[0].SupportPointNames[0], expectedName)));
        }
        
        
        [Test]
        [TestCaseSource(nameof(TimeZonesOutSideOfRange))]
        public void GivenTimeZoneOutSideRange_WhenValidating_ThenReturnMessageTimeZoneOutSideOfRange(TimeSpan timeZoneOutOfRange)
        {
            //Arrange
            WaterFlowFMModel model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            model.Boundaries.Add(boundary);
            
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries) {Feature = boundary};
            AddPointDataToBoundaryCondition(flowBoundaryCondition, model);
            flowBoundaryCondition.TimeZone = timeZoneOutOfRange;
            SetModelTimeZone(model, timeZoneOutOfRange);
            
            
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[]
            {
                flowBoundaryCondition
            });

            string expectedReportMessage = string.Format(Resources.WaterFlowFMBoundaryConditionValidator_ValidateBoundaryConditionTimeZone_Time_zone_of_boundary_condition___0___falls_outside_of_allowed_range__12_00_and__12_00, flowBoundaryCondition.VariableDescription);
            
            //Act
            ValidationReport report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            
            //Assert
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First().Message, Is.EqualTo(expectedReportMessage));
        }

        private static IEnumerable<TestCaseData> TimeSeriesSpansModelTimeSeriesWithTimeZones()
        {
            yield return new TestCaseData(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0));
            yield return new TestCaseData(new TimeSpan(-1, 0, 0), new TimeSpan(-1, 0, 0));
            yield return new TestCaseData(new TimeSpan(1, 0, 0), new TimeSpan(1, 0, 0));
        }

        private static IEnumerable<TestCaseData> TimeSeriesDoesNotSpanModelTimeSeriesWithTimeZones()
        {
            yield return new TestCaseData(new TimeSpan(0, 0, 0), new TimeSpan(-1, 0, 0));
            yield return new TestCaseData(new TimeSpan(0, 0, 0), new TimeSpan(1, 0, 0));
            yield return new TestCaseData(new TimeSpan(-1, 0, 0), new TimeSpan(0, 0, 0));
            yield return new TestCaseData(new TimeSpan(1, 0, 0), new TimeSpan(0, 0, 0));
            yield return new TestCaseData(new TimeSpan(1, 0, 0), new TimeSpan(-1, 0, 0));
            yield return new TestCaseData(new TimeSpan(-1, 0, 0), new TimeSpan(1, 0, 0));
        }
        
        private static IEnumerable<TestCaseData> TimeZonesOutSideOfRange()
        {
            yield return new TestCaseData(new TimeSpan(12,1,0));
            yield return new TestCaseData(new TimeSpan(-12,-1,0));
        }

        private static WaterFlowFMModel CreateValidModel()
        {
            return new WaterFlowFMModel
            {
                TimeStep = new TimeSpan(0, 0, 1, 0),
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 2),
                OutputTimeStep = new TimeSpan(0, 0, 2, 0)
            };
        }

        private static WaterFlowFMModel CreateValidMorphologyModel()
        {
            WaterFlowFMModel model = CreateValidModel();
            model.ModelDefinition.UseMorphologySediment = true;
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction {Name = "testFrac"});
            return model;
        }

        private static void AddPointDataToBoundaryCondition(FlowBoundaryCondition boundaryCondition, WaterFlowFMModel fmModel)
        {
            boundaryCondition.DataPointIndices.Add(0);
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                fmModel.StartTime,
                fmModel.StopTime
            });
        }
    }
}
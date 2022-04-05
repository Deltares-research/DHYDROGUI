using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
            var model = CreateValidModel();
            model.ModelDefinition.UseMorphologySediment = true;
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction {Name = "testFrac"});
            return model;
        }

        [Test]
        public void TestEmptyBoundaryConditionSet()
        {
            var model = CreateValidModel();
            
            model.Boundaries.Add(new Feature2D {Name = "myBoundary"});

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.IsTrue(report.ContainsError("Boundary 'myBoundary' does not contain a boundary condition"));
        }

        [Test]
        public void TestEmptyBoundaryCondition()
        {
            var model = CreateValidModel();

            var boundary = new Feature2D { Name = "myBoundary" };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.That(report.ContainsError("No data defined for boundary condition 'Water level' at boundary 'myBoundary'"));
        }

        [Test]
        public void TestEmptyBoundaryConditionTimeSeries()
        {
            var model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(1, 0)})
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));
            
            model.BoundaryConditions.First().AddPoint(1);

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        public void TestTooShortBoundaryConditionTimeSeries()
        {
            var model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            model.BoundaryConditions.First().AddPoint(1);
            
            var timeSeries = model.BoundaryConditions.First().GetDataAtPoint(1);            
            timeSeries[model.StartTime + new TimeSpan(0, 1, 0, 0)] = 0.5;
            timeSeries[model.StopTime] = 0.5;

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLoadTransport)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void TestMorphologyBoundaryConditionOnlyWithOneTimeSeries(FlowBoundaryQuantityType quantityType)
        {
            var model = CreateValidModel();

            model.ModelDefinition.UseMorphologySediment = true;
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction { Name = "testFrac"});

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary
            };
            flowBoundaryCondition.DataPointIndices.Add(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new [] { model.StartTime, model.StopTime });

            var morphologyBoundaryCondition = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> { "testFrac" }
            };

            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new [] { morphologyBoundaryCondition, flowBoundaryCondition});

            morphologyBoundaryCondition.AddPoint(1);
            var timeSeriesP1 = morphologyBoundaryCondition.GetDataAtPoint(1);

            timeSeriesP1[model.StartTime] = 0.5;
            timeSeriesP1[model.StopTime] = 0.5;

            /* Check everything went alright just with one data point */
            Assert.AreEqual(1, morphologyBoundaryCondition.PointData.Count);
            Assert.IsNotNull(morphologyBoundaryCondition.GetDataAtPoint(0)); // data for morphology is on all data points the same (Horizontally Uniform)
            Assert.IsNotNull(morphologyBoundaryCondition.GetDataAtPoint(1));

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);

            /* Now add a second data point and expect the validation to fail.*/
            morphologyBoundaryCondition.AddPoint(0);
            var timeSeriesP0 = morphologyBoundaryCondition.GetDataAtPoint(0);
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
            var model = CreateValidMorphologyModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var morphologyBoundary = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> { "testFrac" }
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
            };
            AddPointDataToBoundaryCondition(flowBoundaryCondition, model);

            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { morphologyBoundary, flowBoundaryCondition });

            morphologyBoundary.AddPoint(1);
            var timeSeriesP1 = morphologyBoundary.GetDataAtPoint(1);

            timeSeriesP1[model.StartTime] = 0.5;
            timeSeriesP1[model.StopTime] = 0.5;

            /* Check everything went alright just with one data point */
            Assert.AreEqual(1, morphologyBoundary.PointData.Count);
            Assert.IsNotNull(morphologyBoundary.GetDataAtPoint(0));
            Assert.IsNotNull(morphologyBoundary.GetDataAtPoint(1));

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
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
            var model = CreateValidMorphologyModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            model.Boundaries.Add(boundary);
            var bcSet = model.BoundaryConditionSets[0].BoundaryConditions;

            var morphologyBoundaryCondition = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> { "testFrac" }
            };
            AddPointDataToBoundaryCondition(morphologyBoundaryCondition, model);

            var flowBoundaryCondition1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary
            };
            AddPointDataToBoundaryCondition(flowBoundaryCondition1, model);

            bcSet.AddRange(new [] { morphologyBoundaryCondition, flowBoundaryCondition1});

            Assert.AreEqual(2, bcSet.Count);
            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);

            /* Add a second condition */
            var flowBoundaryCondition2 = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> { "testFrac" }
            };
            AddPointDataToBoundaryCondition(flowBoundaryCondition2, model);
            bcSet.Add(flowBoundaryCondition2);

            Assert.AreEqual(3, bcSet.Count);
            report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
        }

        private static void AddPointDataToBoundaryCondition(FlowBoundaryCondition boundaryCondition, WaterFlowFMModel fmModel)
        {
            boundaryCondition.DataPointIndices.Add(0);
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] {fmModel.StartTime, fmModel.StopTime});
        }

        [Test]
        public void TestValidBoundaryConditionTimeSeries()
        {
            var model = CreateValidModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            model.BoundaryConditions.First().AddPoint(1);

            var timeSeries = model.BoundaryConditions.First().GetDataAtPoint(1);

            timeSeries[model.StartTime] = 0.5;
            timeSeries[model.StopTime] = 0.5;

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        public void TestInvalidBoundaryConditionCombination()
        {
            var model = CreateValidModel();

            var boundary = new Feature2D
                {
                    Name = "boundary", Geometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(1, 0)})
                };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                    BoundaryConditionDataType.TimeSeries)
                {
                    Feature = boundary
                });

            var boundaryConditions = model.BoundaryConditions.ToList();

            boundaryConditions[0].AddPoint(1);
            boundaryConditions[1].AddPoint(1);

            var waterLevelTimeSeries = boundaryConditions[0].GetDataAtPoint(1);

            waterLevelTimeSeries[model.StartTime] = 0.5;
            waterLevelTimeSeries[model.StopTime] = 0.5;

            var dischargeTimeSeries = boundaryConditions[1].GetDataAtPoint(1);

            dischargeTimeSeries[model.StartTime] = 1.5;
            dischargeTimeSeries[model.StopTime] = 1.5;

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        public void TestMorphologyBoundaryConditionWithoutHydroBoundaryConditionShouldCreateValidationError()
        {
            var model = CreateValidMorphologyModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var morphologyBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> { "testFrac" }
            };
            AddPointDataToBoundaryCondition(morphologyBoundaryCondition, model);

            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { morphologyBoundaryCondition });

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
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
            var model = CreateValidMorphologyModel();

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            model.Boundaries.Add(boundary);

            var bcSet = model.BoundaryConditionSets[0].BoundaryConditions;
            var morBC = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string> { "testFrac" }
            };
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary
            };
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
            morBC.PointData[0].Arguments[0].SetValues(new[] { startDateTime, endDateTime });
            morBC.PointData[0][startDateTime] = 0.5;
            morBC.PointData[0][endDateTime] = 0.6;

            morBC.PointData[1].Arguments[0].SetValues(new[] { startDateTime, endDateTime });
            morBC.PointData[1][startDateTime] = 0.5;
            morBC.PointData[1][endDateTime] = 0.6;

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First(i => i.Severity == ValidationSeverity.Error).Message,
                Is.EqualTo(Resources.WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_A_morphology_boundary_condition_cannot_have_more_than_one_point_with_generated_data_));
        }

        [Test]
        public void TimeSeriesCannotContainNegativeValuesForStrictlyPositiveBoundaryConditionsTest()
        {
            var model = CreateValidMorphologyModel();
            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            model.Boundaries.Add(boundary);

            var bc = new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string>() { "testFrac" }
            };

            model.BoundaryConditionSets[0].BoundaryConditions.Add(bc);
            model.BoundaryConditions.First().AddPoint(1);

            /* Set negative values for a 'strictly positive boundary' */
            var timeSeries = model.BoundaryConditions.First().GetDataAtPoint(1);
            timeSeries[model.StartTime] = -0.5;
            timeSeries[model.StopTime] = -0.5;

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First(i => i.Severity == ValidationSeverity.Error).Message,
                Is.EqualTo( String.Format(
                    Resources.WaterFlowFMBoundaryConditionValidator_ValidateBoundaryConditionPointIndex_Time_series_contains_forbidden_negative_values_for__0__at_point__1_,
                    bc.VariableDescription, model.BoundaryConditionSets[0].SupportPointNames[1])));
        }

        [Test]
        public void CustomSupportNamesNotSupportedByKernelTest()
        {
            var model = CreateValidModel();
            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));
            
            /* Replace the name of a given support point */
            var expectedName = model.BoundaryConditionSets[0].SupportPointNames[0];
            model.BoundaryConditionSets[0].SupportPointNames[0] = "ThisIsNotAValidName";

            model.BoundaryConditions.First().AddPoint(0);
            var timeSeries = model.BoundaryConditions.First().GetDataAtPoint(0);
            timeSeries[model.StartTime] = 0.5;
            timeSeries[model.StopTime] = 0.5;

            const int expectedIndex = 1;
            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First(i => i.Severity == ValidationSeverity.Error).Message,
                Is.EqualTo(String.Format(
                    Resources.WaterFlowFMBoundaryConditionValidator_ValidateSupportPointNames_Custom_support_point_names_are_not_supported_by_gui,
                    expectedIndex, model.BoundaryConditionSets[0].SupportPointNames[0], expectedName)));
        }
    }
}
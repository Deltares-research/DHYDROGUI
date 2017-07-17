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
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
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

        [Test]
        public void TestEmptyBoundaryConditionSet()
        {
            var model = CreateValidModel();
            
            model.Boundaries.Add(new Feature2D {Name = "boundary"});

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(1, report.WarningCount);
        }

        [Test]
        public void TestEmptyBoundaryCondition()
        {
            var model = CreateValidModel();

            var boundary = new Feature2D { Name = "boundary" };
            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                FlowBoundaryConditionFactory.CreateBoundaryCondition(boundary));

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);

            Assert.AreEqual(1, report.WarningCount);
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

            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction(){ Name = "testFrac"});

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var flowBoundary = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
            };

            var morphologyBoundary = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string>() { "testFrac" }
            };

            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new [] { morphologyBoundary, flowBoundary});

            morphologyBoundary.AddPoint(1);
            var timeSeriesP1 = morphologyBoundary.GetDataAtPoint(1);

            timeSeriesP1[model.StartTime] = 0.5;
            timeSeriesP1[model.StopTime] = 0.5;

            /* Check everything went alright just with one data point */
            Assert.AreEqual(1, morphologyBoundary.PointData.Count);
            Assert.IsNotNull(morphologyBoundary.GetDataAtPoint(0)); // data for morphology is on all data points the same (Horizontally Uniform)
            Assert.IsNotNull(morphologyBoundary.GetDataAtPoint(1));

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);

            /* Now add a second data point and expect the validation to fail.*/
            morphologyBoundary.AddPoint(0);
            var timeSeriesP0 = morphologyBoundary.GetDataAtPoint(0);
            timeSeriesP0[model.StartTime] = 0.5;
            timeSeriesP0[model.StopTime] = 0.5;

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
        public void TestMorphologyBoundaryConditionWithEmptyTimeSeriesIsValid(FlowBoundaryQuantityType quantityType)
        {
            var model = CreateValidModel();

            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction() { Name = "testFrac" });

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var morphologyBoundary = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string>() { "testFrac" }
            };

            var flowBoundary1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
            };


            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { morphologyBoundary, flowBoundary1 });

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

            /* Now add a second data point and expect the validation to fail.*/
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
            var model = CreateValidModel();

            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction() { Name = "testFrac" });

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            model.Boundaries.Add(boundary);
            var bcSet = model.BoundaryConditionSets[0].BoundaryConditions;

            var morphologyBoundary1 = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string>() { "testFrac" }
            };

            var flowBoundary1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
            };
            
            bcSet.AddRange(new [] { morphologyBoundary1, flowBoundary1});

            Assert.AreEqual(2, bcSet.Count);
            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);

            /* Add a second condition */
            var flowBoundary2 = new FlowBoundaryCondition(quantityType, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string>() { "testFrac" }
            };
            bcSet.Add(flowBoundary2);

            Assert.AreEqual(3, bcSet.Count);
            report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
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
            var model = CreateValidModel();

            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction() { Name = "testFrac" });

            var boundary = new Feature2D
            {
                Name = "boundary",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var morphologyBoundary = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport, BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionNames = new List<string>() { "testFrac" }
            };

            model.Boundaries.Add(boundary);
            model.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { morphologyBoundary });

            var report = WaterFlowFMBoundaryConditionValidator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.Issues.First(i => i.Severity == ValidationSeverity.Error).Message,
                Is.EqualTo(Resources.WaterFlowFMBoundaryConditionValidator_ValidateMorphologyBoundaryHaveHydroBoundaries_Morphology_boundary_condition_must_have_a_Hydro_boundary_condition_));

        }
    }
}

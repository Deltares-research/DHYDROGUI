using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
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
    }
}

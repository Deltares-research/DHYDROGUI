using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveTimePointValidatorTest
    {
        private WaveModel WaveModel;

        [SetUp]
        public void Initialize()
        {
            WaveModel = new WaveModel();
        }

        [Test]
        public void GivenWaveModelWithNoTimePointsDefinedAndNotCoupledToFlow_WhenValidating_ThenValidationErrorIsGiven()
        {
            WaveModel.IsCoupledToFlow = false;

            var validationReport = WaveTimePointValidator.Validate(WaveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Equals("No time points defined"));
        }

        [Test]
        public void GivenWaveModelWithNoTimePointsDefinedAndCoupledToFlow_WhenValidating_ThenValidationErrorIsNotGiven()
        {
            WaveModel.IsCoupledToFlow = true;

            var validationReport = WaveTimePointValidator.Validate(WaveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0), "A validation error(s) is given");
        }

        [Test]
        public void GivenWaveModelWithStartTimePrecedingTheReferenceTime_WhenValidating_ThenValidationErrorIsGiven()
        {
            var timePoint = new DateTime(2000, 01, 01);
            WaveModel.TimePointData = new WaveInputFieldData();
            var timePoints = new List<DateTime> {timePoint};
            var timePointData = WaveModel.TimePointData;
            timePointData.InputFields.Arguments[0].AddValues(timePoints);
            Assert.That(timePointData.TimePoints, Is.Not.Empty);

            var validationReport = WaveTimePointValidator.Validate(WaveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Equals("Model start time precedes reference time"));
        }

        [Test]
        public void GivenWaveModelWithStartTimePrecedingTheReferenceTime_WhenValidating_ThenValidationErrorIsNotGiven()
        {
            var timeToBeAddedToReferenceTime = 1;
            SetupModelWithTimePoints(timeToBeAddedToReferenceTime);

            var validationReport = WaveTimePointValidator.Validate(WaveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void GivenWaveModelWithMultipleBoundaryConditionStartTimesPrecedingWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsGiven()
        {
            //Given
            var addedTimeForModelStartTime = 2;
            SetupModelWithTimePoints(addedTimeForModelStartTime);
 
            var boundaryCondition = CreateBoundaryCondition();
            AddBoundaryCondition(boundaryCondition);

            var boundaryConditionWithTimeSeries = WaveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            var pointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            pointData[0].Arguments[0].SetValues(new[]
                {WaveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1)});

            //When
            var validationReport = WaveTimePointValidator.Validate(WaveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");
            var errorCount = 1;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
            Assert.That(validationReport.AllErrors.ElementAt(0).Message, Is.EqualTo("Model start time does not precede any of Boundary Condition time points."));
        }


        [Test]
        public void GivenWaveModelWithBoundaryConditionStartTimesNotPrecedingWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsNotGiven()
        {
            //Given
            var addedTimeForModelStartTime = 2;
            SetupModelWithTimePoints(addedTimeForModelStartTime);

            var boundaryCondition = CreateBoundaryCondition();
            AddBoundaryCondition(boundaryCondition);

            var boundaryConditionWithTimeSeries = WaveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            var pointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            pointData[0].Arguments[0].SetValues(new[]
                {WaveModel.ModelDefinition.ModelReferenceDateTime.AddYears(3)});

            //When
            var validationReport = WaveTimePointValidator.Validate(WaveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");
            var errorCount = 0;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
        }

        [Test]
        public void GivenWaveModelWithAtLeastOneBoundaryConditionStartTimesNotPrecedingWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsNotGiven()
        {
            //Given
            var additionalTime = 2;
            SetupModelWithTimePoints(additionalTime);

            var boundaryCondition = CreateBoundaryCondition();
            boundaryCondition.AddPoint(0);
            boundaryCondition.AddPoint(1);
            WaveModel.ModelDefinition.BoundaryConditions.Add(boundaryCondition);

            var boundaryConditionWithTimeSeries = WaveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            var precedingPointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            precedingPointData[0].Arguments[0].SetValues(new[] { WaveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1) });

            var nonPrecedingPointData = boundaryConditionWithTimeSeries.PointData[1].Components;
            nonPrecedingPointData[0].Arguments[0].SetValues(new[] { WaveModel.ModelDefinition.ModelReferenceDateTime.AddYears(3) });
           
            //When
            var validationReport = WaveTimePointValidator.Validate(WaveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");
            var errorCount = 0;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0), $"Total amount of errors is not equal to {errorCount}");
        }

        [Test]
        public void GivenWaveModelWithBoundaryConditionStartTimesEqualToWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsNotGiven()
        {        
            //Given
            var additionalTime = 2;
            SetupModelWithTimePoints(additionalTime);

            var boundaryCondition = CreateBoundaryCondition();
            boundaryCondition.AddPoint(0);
            boundaryCondition.AddPoint(1);
            WaveModel.ModelDefinition.BoundaryConditions.Add(boundaryCondition);

            var boundaryConditionWithTimeSeries = WaveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            var precedingPointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            precedingPointData[0].Arguments[0].SetValues(new[] { WaveModel.ModelDefinition.ModelReferenceDateTime.AddYears(2) });

            //When
            var validationReport = WaveTimePointValidator.Validate(WaveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");
            var errorCount = 0;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
        }

        #region Helper methods

        private static WaveBoundaryCondition CreateBoundaryCondition()
        {
            var featureWithTwoPoints = new Feature2D
                { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) }) };
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying
            };
            return boundaryCondition;
        }

        private void AddBoundaryCondition(WaveBoundaryCondition boundaryCondition)
        {
            boundaryCondition.AddPoint(0);
            WaveModel.ModelDefinition.BoundaryConditions.Add(boundaryCondition);
        }

        private void SetupModelWithTimePoints(int addedTimeForModelStartTime)
        {
            var timePoint = WaveModel.ModelDefinition.ModelReferenceDateTime.AddYears(addedTimeForModelStartTime);
            WaveModel.TimePointData = new WaveInputFieldData();
            var timePoints = new List<DateTime> { timePoint };
            var timePointsData = WaveModel.TimePointData;
            timePointsData.InputFields.Arguments[0].AddValues(timePoints);
            Assert.That(timePointsData.TimePoints, Is.Not.Empty);
        }

        #endregion
    }
}

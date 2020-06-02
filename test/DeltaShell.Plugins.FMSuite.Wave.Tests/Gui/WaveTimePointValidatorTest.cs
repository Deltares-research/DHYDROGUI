using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
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
        private WaveModel waveModel;

        [SetUp]
        public void Initialize()
        {
            waveModel = new WaveModel();
        }

        [Test]
        public void GivenWaveModelWithNoTimePointsDefinedAndNotCoupledToFlow_WhenValidating_ThenValidationErrorIsGiven()
        {
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Equals("No time points defined"));
        }

        [Test]
        public void GivenWaveModelWithNoTimePointsDefinedAndCoupledToFlow_WhenValidating_ThenValidationErrorIsNotGiven()
        {
            waveModel.IsCoupledToFlow = true;

            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0), "A validation error(s) is given");
        }

        [Test]
        public void GivenWaveModelWithStartTimePrecedingTheReferenceTime_WhenValidating_ThenValidationErrorIsGiven()
        {
            var timePoint = new DateTime(2000, 01, 01);
            waveModel.TimePointData = new WaveInputFieldData();
            var timePoints = new List<DateTime> {timePoint};
            WaveInputFieldData timePointData = waveModel.TimePointData;
            timePointData.InputFields.Arguments[0].AddValues(timePoints);
            Assert.That(timePointData.TimePoints, Is.Not.Empty);

            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
            Assert.That(validationReport.AllErrors.ElementAt(0).Message.Equals("Model start time precedes reference time"));
        }

        [Test]
        public void GivenWaveModelWithStartTimePrecedingTheReferenceTime_WhenValidating_ThenValidationErrorIsNotGiven()
        {
            SetupModelWithTimePoints(1);

            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            Assert.That(validationReport, Is.Not.Null);
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void GivenWaveModelWithBoundaryConditionWithoutStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsNotGiven()
        {
            //Given
            SetupModelWithTimePoints(2);

            WaveBoundaryCondition boundaryCondition = CreateSpatiallyVaryingBoundaryConditionWithParameterizedTimeSeries();
            AddBoundaryCondition(boundaryCondition);

            //When
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");

            var errorCount = 0;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
        }

        [Test]
        public void GivenWaveModelWithBoundaryConditionStartTimesNotPrecedingWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsNotGiven()
        {
            //Given
            SetupModelWithTimePoints(2);

            WaveBoundaryCondition boundaryCondition = CreateSpatiallyVaryingBoundaryConditionWithParameterizedTimeSeries();
            AddBoundaryCondition(boundaryCondition);

            WaveBoundaryCondition boundaryConditionWithTimeSeries = waveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            IEventedList<IVariable> pointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            pointData[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(3)
            });

            //When
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");

            var errorCount = 0;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
        }

        [Test]
        public void GivenWaveModelWithAtLeastOneBoundaryConditionStartTimesNotPrecedingWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsNotGiven()
        {
            //Given
            SetupModelWithTimePoints(2);

            WaveBoundaryCondition boundaryCondition = CreateSpatiallyVaryingBoundaryConditionWithParameterizedTimeSeries();
            boundaryCondition.AddPoint(0);
            boundaryCondition.AddPoint(1);
            waveModel.ModelDefinition.BoundaryConditions.Add(boundaryCondition);

            WaveBoundaryCondition boundaryConditionWithTimeSeries = waveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            IEventedList<IVariable> precedingPointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            precedingPointData[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1)
            });

            IEventedList<IVariable> nonPrecedingPointData = boundaryConditionWithTimeSeries.PointData[1].Components;
            nonPrecedingPointData[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(3)
            });

            //When
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");

            var errorCount = 0;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(0), $"Total amount of errors is not equal to {errorCount}");
        }

        [Test]
        public void GivenWaveModelWithTwoBoundaryConditionStartTimesEqualToWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsNotGiven()
        {
            //Given
            SetupModelWithTimePoints(2);

            WaveBoundaryCondition boundaryCondition = CreateSpatiallyVaryingBoundaryConditionWithParameterizedTimeSeries();
            boundaryCondition.AddPoint(0);
            boundaryCondition.AddPoint(1);
            waveModel.ModelDefinition.BoundaryConditions.Add(boundaryCondition);

            WaveBoundaryCondition boundaryConditionWithTimeSeries = waveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            IEventedList<IVariable> precedingPointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            precedingPointData[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(2)
            });

            //When
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");

            var errorCount = 0;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
        }

        [Test]
        public void GivenWaveModelWithMultipleBoundaryConditionStartTimesPrecedingWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsGiven()
        {
            //Given
            SetupModelWithTimePoints(4);

            WaveBoundaryCondition boundaryCondition = CreateUniformBoundaryConditionWithParameterizedTimeSeries();
            WaveBoundaryCondition boundaryCondition2 = CreateUniformBoundaryConditionWithParameterizedTimeSeries();
            WaveBoundaryCondition boundaryCondition3 = CreateUniformBoundaryConditionWithParameterizedTimeSeries();

            AddBoundaryCondition(boundaryCondition);
            AddBoundaryCondition(boundaryCondition2);
            AddBoundaryCondition(boundaryCondition3);

            WaveBoundaryCondition boundaryConditionWithTimeSeries = waveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            WaveBoundaryCondition boundaryConditionWithTimeSeries2 = waveModel.ModelDefinition.BoundaryConditions.ElementAt(1);
            WaveBoundaryCondition boundaryConditionWithTimeSeries3 = waveModel.ModelDefinition.BoundaryConditions.ElementAt(2);

            IEventedList<IVariable> pointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            pointData[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1)
            });

            IEventedList<IVariable> pointData2 = boundaryConditionWithTimeSeries2.PointData[0].Components;
            pointData2[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(2)
            });

            IEventedList<IVariable> pointData3 = boundaryConditionWithTimeSeries3.PointData[0].Components;
            pointData3[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(3)
            });

            //When
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");

            string boundaryConditionName1 = waveModel.BoundaryConditions.ElementAt(0).Name;
            string boundaryConditionName2 = waveModel.BoundaryConditions.ElementAt(1).Name;
            string boundaryConditionName3 = waveModel.BoundaryConditions.ElementAt(2).Name;
            var errorCount = 3;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
            Assert.That(validationReport.AllErrors.ElementAt(0).Message, Is.EqualTo($"Model start time does not precede any of the time points of {boundaryConditionName1}"));
            Assert.That(validationReport.AllErrors.ElementAt(1).Message, Is.EqualTo($"Model start time does not precede any of the time points of {boundaryConditionName2}"));
            Assert.That(validationReport.AllErrors.ElementAt(2).Message, Is.EqualTo($"Model start time does not precede any of the time points of {boundaryConditionName3}"));
        }

        [Test]
        public void GivenWaveModelWithTwoBoundaryConditionStartTimesPrecedingWaveModelStartTimeWithOnlyOneMissingBoundaryConditionStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsGiven()
        {
            //Given
            SetupModelWithTimePoints(3);

            WaveBoundaryCondition boundaryCondition = CreateUniformBoundaryConditionWithParameterizedTimeSeries();
            WaveBoundaryCondition boundaryCondition2 = CreateUniformBoundaryConditionWithParameterizedTimeSeries();

            AddBoundaryCondition(boundaryCondition);
            AddBoundaryCondition(boundaryCondition2);

            WaveBoundaryCondition boundaryConditionWithTimeSeries = waveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            WaveBoundaryCondition boundaryConditionWithTimeSeries2 = waveModel.ModelDefinition.BoundaryConditions.ElementAt(1);

            IEventedList<IVariable> pointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            pointData[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1)
            });

            IEventedList<IVariable> pointData2 = boundaryConditionWithTimeSeries2.PointData[0].Components;
            pointData2[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(2)
            });

            //When
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");

            string boundaryConditionName1 = waveModel.BoundaryConditions.ElementAt(0).Name;
            string boundaryConditionName2 = waveModel.BoundaryConditions.ElementAt(1).Name;
            var errorCount = 2;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
            Assert.That(validationReport.AllErrors.ElementAt(0).Message, Is.EqualTo($"Model start time does not precede any of the time points of {boundaryConditionName1}"));
            Assert.That(validationReport.AllErrors.ElementAt(1).Message, Is.EqualTo($"Model start time does not precede any of the time points of {boundaryConditionName2}"));
        }

        [Test]
        public void GivenWaveModelWithTwoBoundaryConditionStartTimesPrecedingWaveModelStartTimeWithOneNotPrecedingStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsGiven()
        {
            //Given
            SetupModelWithTimePoints(3);

            WaveBoundaryCondition boundaryCondition = CreateUniformBoundaryConditionWithParameterizedTimeSeries();
            WaveBoundaryCondition boundaryCondition2 = CreateUniformBoundaryConditionWithParameterizedTimeSeries();

            AddBoundaryCondition(boundaryCondition);
            AddBoundaryCondition(boundaryCondition2);

            WaveBoundaryCondition boundaryConditionWithTimeSeries = waveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            WaveBoundaryCondition boundaryConditionWithTimeSeries2 = waveModel.ModelDefinition.BoundaryConditions.ElementAt(1);

            IEventedList<IVariable> pointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            pointData[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1)
            });

            IEventedList<IVariable> pointData2 = boundaryConditionWithTimeSeries2.PointData[0].Components;
            pointData2[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(4)
            });

            //When
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");

            string boundaryConditionName2 = waveModel.BoundaryConditions.ElementAt(1).Name;
            var errorCount = 1;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
            Assert.That(validationReport.AllErrors.ElementAt(0).Message, Is.EqualTo($"Model start time does not precede any of the time points of {boundaryConditionName2}"));
        }

        [Test]
        public void GivenWaveModelWithDifferentBoundaryConditionDataTypeStartTimesPrecedingWaveModelStartTime_WhenValidatingModelTimePoints_ThenValidationErrorIsGiven()
        {
            //Given
            SetupModelWithTimePoints(2);

            WaveBoundaryCondition boundaryConditionDataType1 = CreateSpatiallyVaryingBoundaryConditionWithParameterizedTimeSeries();
            WaveBoundaryCondition boundaryConditionDataType2 = CreateBoundaryConditionWitOtherDataType();
            AddBoundaryCondition(boundaryConditionDataType1);
            AddBoundaryCondition(boundaryConditionDataType2);

            WaveBoundaryCondition boundaryConditionWithTimeSeries = waveModel.ModelDefinition.BoundaryConditions.ElementAt(0);
            IEventedList<IVariable> pointData = boundaryConditionWithTimeSeries.PointData[0].Components;
            pointData[0].Arguments[0].SetValues(new[]
            {
                waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(1)
            });

            //When
            ValidationReport validationReport = WaveTimePointValidator.Validate(waveModel);

            //Then
            Assert.That(validationReport, Is.Not.Null, "The validation report is null");

            var errorCount = 1;
            string boundaryConditionName = waveModel.BoundaryConditions.ElementAt(0).Name;
            Assert.That(validationReport.ErrorCount, Is.EqualTo(errorCount), $"Total amount of errors is not equal to {errorCount}");
            Assert.That(validationReport.AllErrors.ElementAt(0).Message, Is.EqualTo($"Model start time does not precede any of the time points of {boundaryConditionName}"));
        }

        #region Helper methods

        private static WaveBoundaryCondition CreateSpatiallyVaryingBoundaryConditionWithParameterizedTimeSeries()
        {
            var featureWithTwoPoints = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying
            };
            return boundaryCondition;
        }

        private static WaveBoundaryCondition CreateUniformBoundaryConditionWithParameterizedTimeSeries()
        {
            var featureWithTwoPoints = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform
            };
            return boundaryCondition;
        }

        private WaveBoundaryCondition CreateBoundaryConditionWitOtherDataType()
        {
            var featureWithTwoPoints = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0)
                })
            };

            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying
            };

            return boundaryCondition;
        }

        private void AddBoundaryCondition(WaveBoundaryCondition boundaryCondition)
        {
            boundaryCondition.AddPoint(0);
            waveModel.ModelDefinition.BoundaryConditions.Add(boundaryCondition);
        }

        private void SetupModelWithTimePoints(int yearsToAdd)
        {
            DateTime timePoint = waveModel.ModelDefinition.ModelReferenceDateTime.AddYears(yearsToAdd);
            waveModel.TimePointData = new WaveInputFieldData();
            var timePoints = new List<DateTime> {timePoint};
            WaveInputFieldData timePointsData = waveModel.TimePointData;
            timePointsData.InputFields.Arguments[0].AddValues(timePoints);

            Assert.That(timePointsData.TimePoints, Is.Not.Empty);
        }

        #endregion
    }
}
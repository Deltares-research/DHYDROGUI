using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveBoundaryConditionValidatorTest
    {
        private readonly Feature2D featureWithTwoPoints = new Feature2D {Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0)})};
        private readonly Feature2D featureWithThreePoints = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) }) };

        [Test]
        public void GivenNullWaveBoundaryCondition_WhenValidatingBoundaryConditions_ThenValidationReportIsEmpty()
        {
            // Given
            IList<WaveBoundaryCondition> waveBoundaryConditions  = new List<WaveBoundaryCondition> {null};

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.IsEmpty(validationIssues, "Wave boundary conditions equal to null should not be validated.");
        }

        [TestCase(0.999)]
        [TestCase(10.001)]
        public void GivenWaveBoundaryConditionWithPeakEnhancementFactorNotWithinExpectedRange_WhenValidatingBoundaryConditions_ThenValidationErrorIsReturned(double peakEnhancementFactor)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Feature = featureWithTwoPoints,
                SpectralData = {PeakEnhancementFactor = peakEnhancementFactor}
            };
            boundaryCondition.AddPoint(0);
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var expectedMessage = Resources.WaveBoundaryConditionValidator_ValidateSpectralData_Peak_Enhancement_Factor_must_be_a_value_within_the_range_1___10_;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [Test]
        public void GivenWaveBoundaryConditionThatHasNoDataPoints_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned()
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Feature = featureWithTwoPoints
            };
            var waveBoundaryConditions = new List<WaveBoundaryCondition> {boundaryCondition};
            Assert.IsEmpty(boundaryCondition.DataPointIndices);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var expectedMessage = Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_does_not_contain_a_boundary_condition;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        public void GivenSpatiallyVaryingTimeSeriesWaveBoundaryConditionThatHasNoTableEntries_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturnedForSeparateDataPoints(WaveBoundaryConditionSpatialDefinitionType spatialDefinitionType)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = spatialDefinitionType
            };
            boundaryCondition.AddPoint(0);
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_does_not_contain_a_boundary_condition;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [Test]
        public void GivenWaveBoundaryConditionThatHasGeometryWithMoreThanTwoPoints_WhenValidatingBoundaryConditions_ThenInfoMessageIsReturned()
        {
            // Given
            var boundaryCondition = (WaveBoundaryCondition)new WaveBoundaryConditionFactory().CreateBoundaryCondition(featureWithThreePoints, string.Empty, BoundaryConditionDataType.ParameterizedSpectrumConstant);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                // Pass validation on spectrum parameters
                spectrumParameters.Height = 1.0;
                spectrumParameters.Period = 1.0;
                spectrumParameters.Spreading = 1.0;
            });
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var expectedMessage = Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_internal_geometry_points;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Info, expectedMessage);
        }

        [Test]
        public void GivenWaveBoundaryConditionThatHasSpatialVaryingSpatialDefinitionType_WhenValidatingBoundaryConditions_ThenInfoMessageIsReturned()
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Feature = featureWithThreePoints,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying
            };
            boundaryCondition.AddPoint(0);
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var expectedMessage = Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_unactivated_support_points;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Info, expectedMessage);
        }

        [TestCase(0.0, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(25.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(0.0, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(25.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        public void GivenWaveBoundaryConditionThatHasADataPointWithHeightNotInExpectedRange_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double heightValue, WaveBoundaryConditionSpatialDefinitionType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Height = heightValue;
                // Pass validation for other spectrum parameters
                spectrumParameters.Period = 1.0;
                spectrumParameters.Spreading = 1.0;
            });
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Height__must_be_larger_than_0_and_smaller_or_equal_to_25_;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(0.09, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(20.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(0.09, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(20.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        public void GivenWaveBoundaryConditionThatHasADataPointWithPeriodNotInExpectedRange_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double periodValue, WaveBoundaryConditionSpatialDefinitionType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Period = periodValue;
                // Pass validation for other spectrum parameters
                spectrumParameters.Height = 1.0;
                spectrumParameters.Spreading = 1.0;
            });
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Period__must_be_a_value_within_the_range_;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(-360.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(360.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(-360.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(360.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        public void GivenWaveBoundaryConditionThatHasADataPointWithDirectionNotInExpectedRange_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double directionValue, WaveBoundaryConditionSpatialDefinitionType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Direction = directionValue;
                // Pass validation for other spectrum parameters
                spectrumParameters.Period = 1.0;
                spectrumParameters.Height = 1.0;
                spectrumParameters.Spreading = 1.0;
            });
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateSpectrumParameters_Parameter__Direction__must_be_a_value_within_the_range__360___360_;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying, 0.999)]
        [TestCase(WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying, 800.001)]
        [TestCase(WaveBoundaryConditionSpatialDefinitionType.Uniform, 0.999)]
        [TestCase(WaveBoundaryConditionSpatialDefinitionType.Uniform, 800.001)]
        public void
            GivenWaveBoundaryConditionWithPowerDirectionalSpreadingThatHasSpreadingNotWithinExpectedRange_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(
                WaveBoundaryConditionSpatialDefinitionType type, double spreadingValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type,
                DirectionalSpreadingType = WaveDirectionalSpreadingType.Power // Power directional spreading
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Spreading = spreadingValue;
                // Pass validation for other spectrum parameters
                spectrumParameters.Height = 1.0;
                spectrumParameters.Period = 1.0;
            });
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Spreading__must_be_a_value_within_the_range_1_800;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying, 1.999)]
        [TestCase(WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying, 180.001)]
        [TestCase(WaveBoundaryConditionSpatialDefinitionType.Uniform, 1.999)]
        [TestCase(WaveBoundaryConditionSpatialDefinitionType.Uniform, 180.001)]
        public void
            GivenWaveBoundaryConditionWithDegreesDirectionalSpreadingThatHasSpreadingNotWithinExpectedRange_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(
                WaveBoundaryConditionSpatialDefinitionType type, double spreadingValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type,
                DirectionalSpreadingType = WaveDirectionalSpreadingType.Degrees // Degrees directional spreading
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Spreading = spreadingValue;
                // Pass validation for other spectrum parameters
                spectrumParameters.Height = 1.0;
                spectrumParameters.Period = 1.0;
            });
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Spreading__must_be_a_value_within_the_range_2_180;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [Test]
        public void GivenWaveBoundaryConditionThatHasADataPointWithSpectrumParametersEqualToOrSmallerThanZeroAndHasDataTypeParameterizedSpectrumTimeSeries_WhenValidatingBoundaryConditions_ThenNoErrorMessageIsReturned()
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.SpectrumFromFile)
            {
                Feature = featureWithTwoPoints,
                SpectrumParameters =
                {
                    [0] = new WaveBoundaryParameters()
                }
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Height = -1.0;
                spectrumParameters.Period = -1.0;
                spectrumParameters.Spreading = -1.0;
            });
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            Assert.IsEmpty(validationReport.GetAllIssuesRecursive());
        }

        [TestCase(0.0, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(25.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(0.0, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(25.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        public void GivenParameterizedSpectrumTimeSeriesBoundaryConditionThatHasHsValueSmallerThanOrEqualToZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double heightValue, WaveBoundaryConditionSpatialDefinitionType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type
            };
            boundaryCondition.AddPoint(0);

            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] { DateTime.Now });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { heightValue }); // Hs component values
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.DirectionVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { 1.0 });
            
            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Hs__in_the_time_series_table_must_be_within_expected_range;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(0.0999, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(20.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(0.0999, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(20.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        public void GivenParameterizedSpectrumTimeSeriesBoundaryConditionThatHasTpValueSmallerThanOrEqualToZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double periodValue, WaveBoundaryConditionSpatialDefinitionType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type
            };
            boundaryCondition.AddPoint(0);

            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] { DateTime.Now });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { periodValue }); // Tp component values
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.DirectionVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { 1.0 });

            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Tp__in_the_time_series_table_must_be_within_expected_range;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(-360.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(360.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(-360.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(360.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        public void GivenParameterizedSpectrumTimeSeriesBoundaryConditionThatHasDirectionValueSmallerThanOrEqualToZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double directionValue, WaveBoundaryConditionSpatialDefinitionType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type
            };
            boundaryCondition.AddPoint(0);

            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] { DateTime.Now });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.DirectionVariableName)?.SetValues(new List<double> { directionValue }); // Direction component values
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { 1.0 });

            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Direction__in_the_time_series_table_must_be_within_expected_range;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(0.999, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(800.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(0.999, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(800.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        public void GivenParameterizedSpectrumTimeSeriesBoundaryConditionWithPowerDirectionalSpreadingThatHasSpreadingValueNotWithinExpectedRange_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double spreadingValue, WaveBoundaryConditionSpatialDefinitionType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type,
                DirectionalSpreadingType = WaveDirectionalSpreadingType.Power // Power directional spreading
            };
            boundaryCondition.AddPoint(0);

            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] { DateTime.Now });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.DirectionVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { spreadingValue }); // Spreading component values

            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Spreading__in_the_time_series_table_must_be_a_value_within_the_range_1_800;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(1.999, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(180.001, WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)]
        [TestCase(1.999, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        [TestCase(180.001, WaveBoundaryConditionSpatialDefinitionType.Uniform)]
        public void GivenParameterizedSpectrumTimeSeriesBoundaryConditionWithDegreesDirectionalSpreadingThatHasSpreadingValueNotWithinExpectedRange_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double spreadingValue, WaveBoundaryConditionSpatialDefinitionType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = type,
                DirectionalSpreadingType = WaveDirectionalSpreadingType.Degrees // Degrees directional spreading
            };
            boundaryCondition.AddPoint(0);

            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] { DateTime.Now });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.DirectionVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { spreadingValue }); // Spreading component values

            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions);

            // Then
            var precedingText = boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying ? "Point 1: " : string.Empty;
            var expectedMessage = precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Spreading__in_the_time_series_table_must_be_a_value_within_the_range_2_180;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [Test]
        public void ValidationSynchronizeTimePoints_WhenSpatialDefinitionIsSpatiallyVarying_TimePointsShouldBeEqual()
        {
            var factory = new WaveBoundaryConditionFactory();
            var boundaryCondition = (WaveBoundaryCondition) factory.CreateBoundaryCondition(featureWithTwoPoints, string.Empty,
                BoundaryConditionDataType.ParameterizedSpectrumTimeseries);
            boundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying;

            var t1 = DateTime.Now;
            var t2 = t1.AddDays(1);
            var t3 = t1.AddDays(2);

            boundaryCondition.AddPoint(0);
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] {t1, t2});
            var functionOneComponents = boundaryCondition.PointData[0].Components;
            functionOneComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> {2, 2});
            functionOneComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> {2, 2});
            functionOneComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> {2, 2});

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData[1].Arguments[0].SetValues(new[] {t1, t2});
            var functionTwoComponents = boundaryCondition.PointData[1].Components;
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> {1, 1});
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> {1, 1});
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> {1, 1});

            var waveBoundaryConditions = new List<WaveBoundaryCondition> { boundaryCondition };
            var errors = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions).AllErrors.ToArray();
            Assert.AreEqual(0, errors.Length);

            boundaryCondition.PointData[1].Arguments[0].Values.Clear();
            boundaryCondition.PointData[1].Arguments[0].SetValues(new[] {t1, t3});
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { 1, 1 });
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { 1, 1 });
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { 1, 1 });

            errors = WaveBoundaryConditionValidator.Validate(waveBoundaryConditions).AllErrors.ToArray();
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Message.Contains("Time points are not synchronized on boundary"));
        }

        private static void ContainsOnlyOneIssueWithMessage(ValidationReport validationReport, ValidationSeverity severity, string expectedMessage)
        {
            var validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            var validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(severity));
            Assert.That(validationIssue.Message, Is.EqualTo(expectedMessage));
        }
    }
}

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
        private readonly Feature2D feature = new Feature2D {Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0)})};
        private readonly Feature2D featureWithThreePoints = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) }) };

        [Test]
        public void GivenWaveModelWithWithoutDataPointIndices_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned()
        {
            // Given
            var waveModel = new WaveModel();
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Feature = feature
            };
            Assert.IsEmpty(boundaryCondition.DataPointIndices);
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var expectedMessage = Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_has_no_data_defined;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [Test]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasGeometryWithMoreThanTwoPoints_WhenValidatingBoundaryConditions_ThenInfoMessageIsReturned()
        {
            // Given
            var boundaryCondition = (WaveBoundaryCondition)new WaveBoundaryConditionFactory().CreateBoundaryCondition(featureWithThreePoints, string.Empty, BoundaryConditionDataType.ParametrizedSpectrumConstant);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                // Pass validation on spectrum parameters
                spectrumParameters.Height = 1.0;
                spectrumParameters.Period = 1.0;
                spectrumParameters.Spreading = 1.0;
            });

            var waveModel = new WaveModel();
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var expectedMessage = Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_internal_geometry_points;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Info, expectedMessage);
        }

        [Test]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasSpatialVaryingSpatialDefinitionType_WhenValidatingBoundaryConditions_ThenInfoMessageIsReturned()
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Feature = featureWithThreePoints,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying
            };
            boundaryCondition.AddPoint(0);

            var waveModel = new WaveModel();
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var expectedMessage = Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_unactivated_support_points;
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Warning, expectedMessage);
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasADataPointWithHeightEqualToOrSmallerThanZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double heightValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                Feature = feature
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Height = heightValue;
                // Pass validation for other spectrum parameters
                spectrumParameters.Period = 1.0;
                spectrumParameters.Spreading = 1.0;
            });

            var waveModel = new WaveModel();
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var expectedMessage = string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0___Parameter__Height__must_be_greater_than_0_, 1);
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasADataPointWithPeriodEqualToOrSmallerThanZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double periodValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                Feature = feature
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Period = periodValue;
                // Pass validation for other spectrum parameters
                spectrumParameters.Height = 1.0;
                spectrumParameters.Spreading = 1.0;
            });

            var waveModel = new WaveModel();
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var expectedMessage = string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0___Parameter__Period__must_be_greater_than_0_, 1);
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasADataPointWithSpreadingEqualToOrSmallerThanZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double spreadingValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                Feature = feature
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Spreading = spreadingValue;
                // Pass validation for other spectrum parameters
                spectrumParameters.Height = 1.0;
                spectrumParameters.Period = 1.0;
            });

            var waveModel = new WaveModel();
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var expectedMessage = string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0___Parameter__Spreading__must_be_greater_than_0_, 1);
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(BoundaryConditionDataType.ParametrizedSpectrumTimeseries)]
        [TestCase(BoundaryConditionDataType.SpectrumFromFile)]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasADataPointWithSpectrumParametersEqualToOrSmallerThanZeroAndHasDataTypeParametrizedSpectrumTimeSeries_WhenValidatingBoundaryConditions_ThenNoErrorMessageIsReturned(BoundaryConditionDataType type)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(type)
            {
                Feature = feature,
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

            var waveModel = new WaveModel();
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            Assert.IsEmpty(validationReport.GetAllIssuesRecursive());
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void GivenWaveModelWithParametrizedSpectrumTimeSeriesBoundaryConditionThatHasHsValueSmallerThanOrEqualToZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double heightValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumTimeseries)
            {
                Feature = feature
            };
            boundaryCondition.AddPoint(0);

            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] { DateTime.Now });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { heightValue }); // Hs component values
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.DirectionVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { 1.0 });
            
            var model = new WaveModel();
            model.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(model);

            // Then
            var expectedMessage = string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0__Values_in_column__Hs__in_the_time_series_table_must_be_greater_than_0_, 1);
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void GivenWaveModelWithParametrizedSpectrumTimeSeriesBoundaryConditionThatHasTpValueSmallerThanOrEqualToZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double periodValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumTimeseries)
            {
                Feature = feature
            };
            boundaryCondition.AddPoint(0);

            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] { DateTime.Now });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { periodValue }); // Tp component values
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.DirectionVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { 1.0 });

            var model = new WaveModel();
            model.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(model);

            // Then
            var expectedMessage = string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0__Values_in_column__Tp__in_the_time_series_table_must_be_greater_than_0_, 1);
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void GivenWaveModelWithParametrizedSpectrumTimeSeriesBoundaryConditionThatHasSpreadingValueSmallerThanOrEqualToZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double spreadingValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumTimeseries)
            {
                Feature = feature
            };
            boundaryCondition.AddPoint(0);

            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] { DateTime.Now });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.DirectionVariableName)?.SetValues(new List<double> { 1.0 });
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { spreadingValue }); // Spreading component values

            var model = new WaveModel();
            model.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(model);

            // Then
            var expectedMessage = string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0__Values_in_column__Spreading__in_the_time_series_table_must_be_greater_than_0_, 1);
            ContainsOnlyOneIssueWithMessage(validationReport, ValidationSeverity.Error, expectedMessage);
        }

        [Test]
        public void ValidationSynchronizeTimePoints_WhenSpatialDefinitionIsUniform_TimePointsShouldNotBeValidated()
        {
            var model = new WaveModel();

            var factory = new WaveBoundaryConditionFactory();
            var boundaryCondition = (WaveBoundaryCondition) factory.CreateBoundaryCondition(feature, string.Empty,
                BoundaryConditionDataType.ParametrizedSpectrumTimeseries);
            model.BoundaryConditions.Add(boundaryCondition);
            boundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform;

            boundaryCondition.AddPoint(0);
            var functionComponents = boundaryCondition.PointData[0].Components;
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] {DateTime.Now, DateTime.Now.AddDays(1)});
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> {1, 1});
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> {1, 1});
            functionComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> {1, 1});

            boundaryCondition.AddPoint(1);

            var errors = WaveBoundaryConditionValidator.Validate(model).AllErrors;
            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void ValidationSynchronizeTimePoints_WhenSpatialDefinitionIsSpatiallyVarying_TimePointsShouldBeEqual()
        {
            var model = new WaveModel();

            var factory = new WaveBoundaryConditionFactory();
            var boundaryCondition = (WaveBoundaryCondition) factory.CreateBoundaryCondition(feature, string.Empty,
                BoundaryConditionDataType.ParametrizedSpectrumTimeseries);
            model.BoundaryConditions.Add(boundaryCondition);
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

            var errors = WaveBoundaryConditionValidator.Validate(model).AllErrors;
            Assert.AreEqual(0, errors.Count());

            boundaryCondition.PointData[1].Arguments[0].Values.Clear();
            boundaryCondition.PointData[1].Arguments[0].SetValues(new[] {t1, t3});
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName)?.SetValues(new List<double> { 1, 1 });
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName)?.SetValues(new List<double> { 1, 1 });
            functionTwoComponents.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName)?.SetValues(new List<double> { 1, 1 });

            errors = WaveBoundaryConditionValidator.Validate(model).AllErrors;
            Assert.AreEqual(1, errors.Count());
            Assert.That(errors.FirstOrDefault().Message.Contains("Time points are not synchronized on boundary"));
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

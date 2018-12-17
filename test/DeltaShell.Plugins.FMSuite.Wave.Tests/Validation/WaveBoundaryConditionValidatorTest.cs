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
        [Test]
        public void GivenWaveModelWithWithoutDataPointIndices_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned()
        {
            // Given
            var waveModel = new WaveModel();
            var feature = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) }) };
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Feature = feature
            };
            Assert.IsEmpty(boundaryCondition.DataPointIndices);
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            var validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(validationIssue.Message, Is.EqualTo(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_has_no_data_defined));
        }

        [Test]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasGeometryWithMoreThanTwoPoints_WhenValidatingBoundaryConditions_ThenInfoMessageIsReturned()
        {
            // Given
            var waveModel = new WaveModel();
            var feature = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2,0) }) };
            var boundaryCondition = (WaveBoundaryCondition)new WaveBoundaryConditionFactory().CreateBoundaryCondition(feature, string.Empty, BoundaryConditionDataType.ParametrizedSpectrumConstant);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                // Pass validation on spectrum parameters
                spectrumParameters.Height = 1.0;
                spectrumParameters.Period = 1.0;
            });
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            var validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Info));
            Assert.That(validationIssue.Message, Is.EqualTo(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_internal_geometry_points));
        }

        [Test]
        public void GivenWaveModelWithWaveBoundaryConditionThatHas_WhenValidatingBoundaryConditions_ThenInfoMessageIsReturned()
        {
            // Given
            var waveModel = new WaveModel();
            var feature = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) }) };
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Feature = feature,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying
            };
            boundaryCondition.AddPoint(0);
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            var validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Warning));
            Assert.That(validationIssue.Message, Is.EqualTo(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_unactivated_support_points));
        }

        [Test]
        public void ValidationSynchronizeTimePoints_WhenSpatialDefinitionIsUniform_TimepointsShouldNotBeValidated()
        {
            var model = new WaveModel();

            var feature = new Feature2D {Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0)})};
            var factory = new WaveBoundaryConditionFactory();
            var boundaryCondition = (WaveBoundaryCondition) factory.CreateBoundaryCondition(feature, string.Empty,
                BoundaryConditionDataType.ParametrizedSpectrumTimeseries);
            model.BoundaryConditions.Add(boundaryCondition);
            boundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform;

            boundaryCondition.AddPoint(0);
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] {DateTime.Now, DateTime.Now.AddDays(1)});
            boundaryCondition.PointData[0].Components[0].SetValues(new List<double> {0, 0});

            boundaryCondition.AddPoint(1);

            var errors = WaveBoundaryConditionValidator.Validate(model).AllErrors;
            Assert.AreEqual(0, errors.Count());
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasADataPointWithHeightEqualToOrSmallerThanZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double heightValue)
        {
            // Given
            var waveModel = new WaveModel();
            var feature = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 0) }) };
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                Feature = feature,
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Height = heightValue;
                spectrumParameters.Period = 1.0; // Pass validation for period value
            });
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            var validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(validationIssue.Message, Is.EqualTo(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Parameter__Height__must_be_greater_than_0_));
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void GivenWaveModelWithWaveBoundaryConditionThatHasADataPointWithPeriodEqualToOrSmallerThanZero_WhenValidatingBoundaryConditions_ThenErrorMessageIsReturned(double periodValue)
        {
            // Given
            var waveModel = new WaveModel();
            var feature = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 0) }) };
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                Feature = feature,
            };
            boundaryCondition.AddPoint(0);
            boundaryCondition.SpectrumParameters.Values.ForEach(spectrumParameters =>
            {
                spectrumParameters.Height = 1.0;
                spectrumParameters.Period = periodValue;
            });
            waveModel.BoundaryConditions.Add(boundaryCondition);

            // When
            var validationReport = WaveBoundaryConditionValidator.Validate(waveModel);

            // Then
            var validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            var validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(validationIssue.Message, Is.EqualTo(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Parameter__Period__must_be_greater_than_0_));
        }

        [Test]
        public void ValidationSynchronizeTimePoints_WhenSpatialDefinitionIsSpatiallyVarying_TimepointsShouldBeEqual()
        {
            var model = new WaveModel();

            var feature = new Feature2D {Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0)})};
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
            boundaryCondition.PointData[0].Components[0].SetValues(new List<double> {0, 0});

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData[1].Arguments[0].SetValues(new[] {t1, t2});
            boundaryCondition.PointData[1].Components[0].SetValues(new List<double> {1, 1});

            var errors = WaveBoundaryConditionValidator.Validate(model).AllErrors;
            Assert.AreEqual(0, errors.Count());

            boundaryCondition.PointData[1].Arguments[0].Values.Clear();
            boundaryCondition.PointData[1].Arguments[0].SetValues(new[] {t1, t3});

            errors = WaveBoundaryConditionValidator.Validate(model).AllErrors;
            Assert.AreEqual(1, errors.Count());
            Assert.That(errors.FirstOrDefault().Message.Contains("Time points are not synchronized on boundary"));
        }

    }
}

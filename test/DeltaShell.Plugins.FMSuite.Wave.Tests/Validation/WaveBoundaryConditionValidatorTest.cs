using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveBoundaryConditionValidatorTest
    {

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
            boundaryCondition.PointData[0].Components[0].SetValues(new List<double>() {0, 0});

            boundaryCondition.AddPoint(1);

            var errors = WaveBoundaryConditionValidator.Validate(model).AllErrors;
            Assert.AreEqual(0, errors.Count());
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
            boundaryCondition.PointData[0].Components[0].SetValues(new List<double>() {0, 0});

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData[1].Arguments[0].SetValues(new[] {t1, t2});
            boundaryCondition.PointData[1].Components[0].SetValues(new List<double>() {1, 1});

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

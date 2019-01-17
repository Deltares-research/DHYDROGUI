using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveBoundaryConditionTest
    {
        private readonly Feature2D featureWithTwoPoints = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) }) };

        [Test]
        public void WhenInstantiatingAWaveBoundaryCondition_ThenTheDefaultPeakEnhancementFactorValueIsEqualToExpectedValue()
        {
            // When
            var waveBoundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Harmonics);

            // Then
            Assert.That(waveBoundaryCondition.PeakEnhancementFactor, Is.EqualTo(3.3));
        }

        [TestCase(WaveDirectionalSpreadingType.Power, 4.0)]
        [TestCase(WaveDirectionalSpreadingType.Degrees, 30.0)]
        public void GivenWaveBoundaryConditionWithParameterizedConstantDataType_WhenDataPointsAreAdded_ThenSpreadingValueIsDependentOnDirectionalSpreadingType
            (WaveDirectionalSpreadingType directionalSpreadingType, double expectedSpreadingValue)
        {
            // Given
            var waveBoundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = featureWithTwoPoints,
                DirectionalSpreadingType = directionalSpreadingType
            };

            // When
            waveBoundaryCondition.AddPoint(0);
            waveBoundaryCondition.AddPoint(1);

            // Then
            waveBoundaryCondition.SpectrumParameters.Values.ForEach(p => Assert.That(p.Spreading, Is.EqualTo(expectedSpreadingValue)));
        }

        [TestCase(WaveDirectionalSpreadingType.Power, WaveDirectionalSpreadingType.Degrees, 30.0)]
        [TestCase(WaveDirectionalSpreadingType.Degrees, WaveDirectionalSpreadingType.Power, 4.0)]
        public void GivenWaveBoundaryConditionWithParameterizedConstantDataType_WhenDirectionalSpreadingTypeIsChanged_ThenSpreadingValueIsDependentOnNewDirectionalSpreadingType
            (WaveDirectionalSpreadingType initialDirectionalSpreadingType, WaveDirectionalSpreadingType newDirectionalSpreadingType, double expectedSpreadingValue)
        {
            // Given
            var waveBoundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = featureWithTwoPoints,
                DirectionalSpreadingType = initialDirectionalSpreadingType
            };
            waveBoundaryCondition.AddPoint(0);
            waveBoundaryCondition.AddPoint(1);

            // When
            waveBoundaryCondition.DirectionalSpreadingType = newDirectionalSpreadingType;

            // Then
            waveBoundaryCondition.SpectrumParameters.Values.ForEach(p => Assert.That(p.Spreading, Is.EqualTo(expectedSpreadingValue)));
        }

        [TestCase(WaveDirectionalSpreadingType.Power, 4.0, WaveBoundaryCondition.PowerUnitSymbol)]
        [TestCase(WaveDirectionalSpreadingType.Degrees, 30.0, WaveBoundaryCondition.DegreesUnitSymbol)]
        public void GivenWaveBoundaryConditionWithParameterizedTimeSeriesDataType_WhenDirectionalSpreadingTypeIsChanged_ThenFunctionDefaultValuesAreAsExpected
            (WaveDirectionalSpreadingType directionalSpreadingType, double expectedDefaultSpreadingValue, string expectedDirectionUnitSymbol)
        {
            // Given
            var waveBoundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                DirectionalSpreadingType = directionalSpreadingType
            };

            // When
            waveBoundaryCondition.AddPoint(0);
            waveBoundaryCondition.AddPoint(1);

            // Then
            waveBoundaryCondition.PointData.ForEach(function =>
            {
                function.Components.Where(c => c.Name == WaveBoundaryCondition.HeightVariableName).ForEach(c => Assert.That(c.DefaultValue, Is.EqualTo(0.0)));
                function.Components.Where(c => c.Name == WaveBoundaryCondition.PeriodVariableName).ForEach(c => Assert.That(c.DefaultValue, Is.EqualTo(1.0)));
                function.Components.Where(c => c.Name == WaveBoundaryCondition.DirectionVariableName).ForEach(c =>
                {
                    Assert.That(c.Unit.Symbol == expectedDirectionUnitSymbol);
                    Assert.That(c.DefaultValue, Is.EqualTo(0.0));
                });
                function.Components.Where(c => c.Name == WaveBoundaryCondition.SpreadingVariableName).ForEach(c => Assert.That(c.DefaultValue, Is.EqualTo(expectedDefaultSpreadingValue)));
            });
        }

        [TestCase(WaveDirectionalSpreadingType.Power, WaveDirectionalSpreadingType.Degrees, 30.0, WaveBoundaryCondition.DegreesUnitSymbol)]
        [TestCase(WaveDirectionalSpreadingType.Degrees, WaveDirectionalSpreadingType.Power, 4.0, WaveBoundaryCondition.PowerUnitSymbol)]
        public void GivenWaveBoundaryConditionWithParameterizedTimeSeriesDataType_WhenDataPointsAreAdded_ThenFunctionDefaultValuesAreAsExpected
            (WaveDirectionalSpreadingType initialDirectionalSpreadingType, WaveDirectionalSpreadingType newDirectionalSpreadingType, double expectedDefaultSpreadingValue, string expectedDirectionUnitSymbol)
        {
            // Given
            var waveBoundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                DirectionalSpreadingType = initialDirectionalSpreadingType
            };
            waveBoundaryCondition.AddPoint(0);
            waveBoundaryCondition.AddPoint(1);

            // When
            waveBoundaryCondition.DirectionalSpreadingType = newDirectionalSpreadingType;

            // Then
            waveBoundaryCondition.PointData.ForEach(function =>
            {
                function.Components.Where(c => c.Name == WaveBoundaryCondition.HeightVariableName).ForEach(c => Assert.That(c.DefaultValue, Is.EqualTo(0.0)));
                function.Components.Where(c => c.Name == WaveBoundaryCondition.PeriodVariableName).ForEach(c => Assert.That(c.DefaultValue, Is.EqualTo(1.0)));
                function.Components.Where(c => c.Name == WaveBoundaryCondition.DirectionVariableName).ForEach(c =>
                {
                    Assert.That(c.Unit.Symbol == expectedDirectionUnitSymbol);
                    Assert.That(c.DefaultValue, Is.EqualTo(0.0));
                });
                function.Components.Where(c => c.Name == WaveBoundaryCondition.SpreadingVariableName).ForEach(c => Assert.That(c.DefaultValue, Is.EqualTo(expectedDefaultSpreadingValue)));
            });
        }

        [TestCase(WaveDirectionalSpreadingType.Power, 4.0)]
        [TestCase(WaveDirectionalSpreadingType.Degrees, 30.0)]
        public void GivenWaveBoundaryConditionWithUniformSpatialDefinitionType_WhenChangingDataTypeToParameterizedConstant_ThenSpectrumParameterValuesAreAsExpected
            (WaveDirectionalSpreadingType directionalSpreadingType, double expectedSpreadingValue)
        {
            // Given
            var waveBoundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = featureWithTwoPoints,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform,
                DirectionalSpreadingType = directionalSpreadingType
            };

            // When
            // Do not put the next statement in the object initializer of the WaveBoundaryCondition object.
            // We want to explicitly move from one data type to the parameterized (Constant) data type after initializing
            waveBoundaryCondition.DataType = BoundaryConditionDataType.ParameterizedSpectrumConstant;

            // Then
            waveBoundaryCondition.SpectrumParameters.Values.ForEach(p =>
            {
                Assert.That(p.Height, Is.EqualTo(0.0));
                Assert.That(p.Direction, Is.EqualTo(0.0));
                Assert.That(p.Period, Is.EqualTo(1.0));
                Assert.That(p.Spreading, Is.EqualTo(expectedSpreadingValue));
            });
        }
    }
}
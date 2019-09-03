using System;
using System.Globalization;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class MdwFileDelftIniCategoryCreatorTest
    {
        private readonly IGeometry boundaryGeometry = new LineString(new[]
        {
            new Coordinate(0.0, 1.0),
            new Coordinate(2.0, 3.0)
        });

        private Random randomNumberGenerator;

        [SetUp]
        public void Setup()
        {
            randomNumberGenerator = new Random();
        }

        [Test]
        public void GivenBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedStandardPropertyValuesIsReturned()
        {
            // Given
            const string boundaryName = "myBoundary";
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Name = "myBoundaryCondition",
                Feature = new Feature2D
                {
                    Name = boundaryName,
                    Geometry = boundaryGeometry
                }
            };

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(boundaryCondition);

            // Then
            Assert.That(category.Name, Is.EqualTo("Boundary"));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.Name), Is.EqualTo(boundaryName));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.StartCoordinateX), Is.EqualTo(GetStringValue(0.0)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.EndCoordinateX), Is.EqualTo(GetStringValue(2.0)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.StartCoordinateY), Is.EqualTo(GetStringValue(1.0)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.EndCoordinateY), Is.EqualTo(GetStringValue(3.0)));
        }

        [Test]
        [TestCase(WaveSpectrumShapeType.Gauss, "gauss")]
        [TestCase(WaveSpectrumShapeType.Jonswap, "jonswap")]
        [TestCase(WaveSpectrumShapeType.PiersonMoskowitz, "pierson-moskowitz")]
        public void GivenBoundaryConditionWithShapeType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedShapeTypeIsReturned(WaveSpectrumShapeType shapeType, string expectedValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Name = "myBoundaryCondition",
                Feature = new Feature2D
                {
                    Geometry = boundaryGeometry
                },
                ShapeType = shapeType
            };

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(boundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.ShapeType), Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(WavePeriodType.Mean, "mean")]
        [TestCase(WavePeriodType.Peak, "peak")]
        public void GivenBoundaryConditionWithPeriodType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedPeriodTypeIsReturned(WavePeriodType periodType, string expectedValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Name = "myBoundaryCondition",
                Feature = new Feature2D
                {
                    Geometry = boundaryGeometry
                },
                PeriodType = periodType
            };

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(boundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.PeriodType), Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(WaveDirectionalSpreadingType.Degrees, "degrees")]
        [TestCase(WaveDirectionalSpreadingType.Power, "power")]
        public void GivenBoundaryConditionWithDirectionalSpreadingType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedDirectionalSpreadingTypeIsReturned(WaveDirectionalSpreadingType directionalSpreadingType, string expectedValue)
        {
            // Given
            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Name = "myBoundaryCondition",
                Feature = new Feature2D
                {
                    Geometry = boundaryGeometry
                },
                DirectionalSpreadingType = directionalSpreadingType
            };

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(boundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingType), Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedPropertyValuesIsReturned()
        {
            // Given
            double peakEnhancementFactor = randomNumberGenerator.NextDouble();
            double gaussianSpreadingValue = randomNumberGenerator.NextDouble();

            var boundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Name = "myBoundaryCondition",
                Feature = new Feature2D
                {
                    Geometry = boundaryGeometry
                },
                PeakEnhancementFactor = peakEnhancementFactor,
                GaussianSpreadingValue = gaussianSpreadingValue
            };

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(boundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.PeakEnhancementFactor), Is.EqualTo(GetStringValue(peakEnhancementFactor)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.GaussianSpreading), Is.EqualTo(GetStringValue(gaussianSpreadingValue)));
        }

        private static string GetStringValue(double value)
        {
            return value.ToString("e7", CultureInfo.InvariantCulture);
        }
    }
}
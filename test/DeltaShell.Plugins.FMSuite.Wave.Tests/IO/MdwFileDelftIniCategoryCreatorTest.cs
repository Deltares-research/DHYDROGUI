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
        private WaveBoundaryCondition waveBoundaryCondition;
        private const string boundaryName = "myBoundary";
        private IGeometry boundaryGeometry;
        private double startX;
        private double endX;
        private double startY;
        private double endY;
        
        private Random randomNumberGenerator;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            randomNumberGenerator = new Random();

            startX = randomNumberGenerator.NextDouble();
            endX = randomNumberGenerator.NextDouble();
            startY = randomNumberGenerator.NextDouble();
            endY = randomNumberGenerator.NextDouble();
            boundaryGeometry = new LineString(new[]
            {
                new Coordinate(startX, startY),
                new Coordinate(endX, endY),
            });
        }

        [SetUp]
        public void TestSetup()
        {
            waveBoundaryCondition = new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Name = "myBoundaryCondition",
                Feature = new Feature2D
                {
                    Name = boundaryName,
                    Geometry = boundaryGeometry
                }
            };
        }

        [Test]
        public void GivenBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedStandardPropertyValuesIsReturned()
        {
            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.Name, Is.EqualTo("Boundary"));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.Name), Is.EqualTo(boundaryName));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.StartCoordinateX), Is.EqualTo(GetStringValue(startX)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.EndCoordinateX), Is.EqualTo(GetStringValue(endX)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.StartCoordinateY), Is.EqualTo(GetStringValue(startY)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.EndCoordinateY), Is.EqualTo(GetStringValue(endY)));
        }

        [Test]
        [TestCase(WaveSpectrumShapeType.Gauss, "gauss")]
        [TestCase(WaveSpectrumShapeType.Jonswap, "jonswap")]
        [TestCase(WaveSpectrumShapeType.PiersonMoskowitz, "pierson-moskowitz")]
        public void GivenBoundaryConditionWithShapeType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedShapeTypeIsReturned(WaveSpectrumShapeType shapeType, string expectedValue)
        {
            // Given
            waveBoundaryCondition.ShapeType = shapeType;

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.ShapeType), Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(WavePeriodType.Mean, "mean")]
        [TestCase(WavePeriodType.Peak, "peak")]
        public void GivenBoundaryConditionWithPeriodType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedPeriodTypeIsReturned(WavePeriodType periodType, string expectedValue)
        {
            // Given
            waveBoundaryCondition.PeriodType = periodType;

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.PeriodType), Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(WaveDirectionalSpreadingType.Degrees, "degrees")]
        [TestCase(WaveDirectionalSpreadingType.Power, "power")]
        public void GivenBoundaryConditionWithDirectionalSpreadingType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedDirectionalSpreadingTypeIsReturned(WaveDirectionalSpreadingType directionalSpreadingType, string expectedValue)
        {
            // Given
            waveBoundaryCondition.DirectionalSpreadingType = directionalSpreadingType;

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingType), Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedPropertyValuesIsReturned()
        {
            // Given
            double peakEnhancementFactor = randomNumberGenerator.NextDouble();
            double gaussianSpreadingValue = randomNumberGenerator.NextDouble();

            waveBoundaryCondition.PeakEnhancementFactor = peakEnhancementFactor;
            waveBoundaryCondition.GaussianSpreadingValue = gaussianSpreadingValue;

            // When
            DelftIniCategory category = MdwFileDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.TestUtils;
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
    public class WaveDelftIniCategoryCreatorTest
    {
        private const string boundaryName = "myBoundary";
        private const string spectrumFileName1 = "mySpectrumFile1.sp1";
        private const string spectrumFileName2 = "mySpectrumFile2.sp2";
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
                new Coordinate(endX, endY)
            });
        }

        [Test]
        [TestCaseSource(nameof(NonFileBasedDataTypes))]
        [TestCaseSource(nameof(UniformDefinitionDataTypes))]
        public void GivenBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedStandardPropertyValuesIsReturned(BoundaryConditionDataType dataType)
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(dataType);
            
            // When
            DelftIniCategory category = WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.Name, Is.EqualTo("Boundary"));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.Name), Is.EqualTo(boundaryName));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.StartCoordinateX),
                        Is.EqualTo(GetStringValue(startX)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.EndCoordinateX),
                        Is.EqualTo(GetStringValue(endX)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.StartCoordinateY),
                        Is.EqualTo(GetStringValue(startY)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.EndCoordinateY),
                        Is.EqualTo(GetStringValue(endY)));
        }

        [Test]
        [TestCase(WaveSpectrumShapeType.Gauss, "gauss")]
        [TestCase(WaveSpectrumShapeType.Jonswap, "jonswap")]
        [TestCase(WaveSpectrumShapeType.PiersonMoskowitz, "pierson-moskowitz")]
        public void GivenBoundaryConditionWithShapeType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedShapeTypeIsReturned(
                WaveSpectrumShapeType shapeType, string expectedValue)
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(BoundaryConditionDataType.Constant);
            waveBoundaryCondition.ShapeType = shapeType;

            // When
            DelftIniCategory category =
                WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.ShapeType), Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(WavePeriodType.Mean, "mean")]
        [TestCase(WavePeriodType.Peak, "peak")]
        public void GivenBoundaryConditionWithPeriodType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedPeriodTypeIsReturned(
                WavePeriodType periodType, string expectedValue)
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(BoundaryConditionDataType.Constant);
            waveBoundaryCondition.PeriodType = periodType;

            // When
            DelftIniCategory category =
                WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.PeriodType), Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(WaveDirectionalSpreadingType.Degrees, "degrees")]
        [TestCase(WaveDirectionalSpreadingType.Power, "power")]
        public void GivenBoundaryConditionWithDirectionalSpreadingType_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedDirectionalSpreadingTypeIsReturned(
                WaveDirectionalSpreadingType directionalSpreadingType, string expectedValue)
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(BoundaryConditionDataType.Constant);
            waveBoundaryCondition.DirectionalSpreadingType = directionalSpreadingType;

            // When
            DelftIniCategory category =
                WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingType),
                        Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCaseSource(nameof(NonFileBasedDataTypes))]
        [TestCaseSource(nameof(UniformDefinitionDataTypes))]
        public void GivenBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedPropertyValuesIsReturned(BoundaryConditionDataType dataType)
        {
            // Given
            double peakEnhancementFactor = randomNumberGenerator.NextDouble();
            double gaussianSpreadingValue = randomNumberGenerator.NextDouble();

            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(dataType);
            waveBoundaryCondition.PeakEnhancementFactor = peakEnhancementFactor;
            waveBoundaryCondition.GaussianSpreadingValue = gaussianSpreadingValue;

            // When
            DelftIniCategory category =
                WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.SpectrumSpec), Is.EqualTo("parametric"));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.PeakEnhancementFactor),
                        Is.EqualTo(GetStringValue(peakEnhancementFactor)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.GaussianSpreading),
                        Is.EqualTo(GetStringValue(gaussianSpreadingValue)));
        }

        [Test]
        public void GivenSpectrumFromFileBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedSpectrumSpecIsReturned()
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(BoundaryConditionDataType.SpectrumFromFile);

            // When
            DelftIniCategory category = WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.SpectrumSpec), Is.EqualTo("from file"));
        }

        [Test]
        public void GivenSpectrumFromFileBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedSpectrumFileNameIsReturned()
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(BoundaryConditionDataType.SpectrumFromFile);
            waveBoundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform;
            waveBoundaryCondition.SpectrumFiles[1] = "mySpectrumFile2.sp2";

            // When
            DelftIniCategory category = WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.Spectrum), Is.EqualTo(spectrumFileName1));
        }

        [Test]
        public void GivenParameterizedSpectrumConstantBoundaryCondition_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedSpectrumParameterValuesIsReturned()
        {
            // Given
            double height = randomNumberGenerator.NextDouble();
            double period = randomNumberGenerator.NextDouble();
            double direction = randomNumberGenerator.NextDouble();
            double spreading = randomNumberGenerator.NextDouble();

            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant);
            waveBoundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform;
            waveBoundaryCondition.SpectrumParameters[0].Height = height;
            waveBoundaryCondition.SpectrumParameters[0].Period = period;
            waveBoundaryCondition.SpectrumParameters[0].Direction = direction;
            waveBoundaryCondition.SpectrumParameters[0].Spreading = spreading;

            // When
            DelftIniCategory category = WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            Assert.That(category.GetPropertyValue(KnownWaveProperties.WaveHeight), Is.EqualTo(GetStringValue(height)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.Period), Is.EqualTo(GetStringValue(period)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.Direction), Is.EqualTo(GetStringValue(direction)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingValue), Is.EqualTo(GetStringValue(spreading)));
        }

        [Test]
        [TestCaseSource(nameof(NonFileBasedDataTypes))]
        [TestCaseSource(nameof(UniformDefinitionDataTypes))]
        [TestCaseSource(nameof(FileBasedDataTypes))]
        public void GivenSpatiallyVaryingBoundaryConditionWithoutDataPointIndices_WhenCreatingDelftIniCategory_ThenMessageIsLoggedAndSpectrumParametersHaveDefaultValues(BoundaryConditionDataType dataType)
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(dataType);
            waveBoundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying;
            DelftIniCategory category = null;

            // When
            void Call() => category = WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            string expectedMessage = $@"No data points found for boundary '{boundaryName}', saved boundary will default to Uniform spatial definition type";
            TestHelper.AssertLogMessageIsGenerated(Call, expectedMessage, 1);

            Assert.That(category.GetPropertyValue(KnownWaveProperties.WaveHeight), Is.EqualTo(GetStringValue(0.0)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.Period), Is.EqualTo(GetStringValue(0.0)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.Direction), Is.EqualTo(GetStringValue(0.0)));
            Assert.That(category.GetPropertyValue(KnownWaveProperties.DirectionalSpreadingValue), Is.EqualTo(GetStringValue(0.0)));
        }

        [Test]
        [TestCaseSource(nameof(NonFileBasedDataTypes))]
        [TestCaseSource(nameof(FileBasedDataTypes))]
        public void GivenSpatiallyVaryingBoundaryConditionWithTwoDataPoints_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedCondSpecAtDistValuesIsReturned(BoundaryConditionDataType dataType)
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(dataType);
            waveBoundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying;
            waveBoundaryCondition.AddPoint(0);
            waveBoundaryCondition.AddPoint(1);

            // When
            DelftIniCategory category = WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            DelftIniProperty[] properties = category.Properties.Where(p => p.Name == KnownWaveProperties.CondSpecAtDist).ToArray();
            Assert.That(properties.Length, Is.EqualTo(2));
            Assert.That(properties.First().Value, Is.EqualTo(GetStringValue(0.0)));

            double length = waveBoundaryCondition.Feature.Geometry.Length;
            Assert.That(properties.Last().Value, Is.EqualTo(GetStringValue(length)));
        }

        [Test]
        public void GivenSpatiallyVaryingFileBasedBoundaryConditionWithTwoDataPoints_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedSpectrumValuesIsReturned()
        {
            // Given
            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(BoundaryConditionDataType.SpectrumFromFile);
            waveBoundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying;
            waveBoundaryCondition.AddPoint(0);
            waveBoundaryCondition.AddPoint(1);
            waveBoundaryCondition.SpectrumFiles[0] = spectrumFileName1;
            waveBoundaryCondition.SpectrumFiles[1] = spectrumFileName2;

            // When
            DelftIniCategory category = WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            DelftIniProperty[] properties = category.Properties.Where(p => p.Name == KnownWaveProperties.Spectrum).ToArray();
            Assert.That(properties.Length, Is.EqualTo(2));
            Assert.That(properties.First().Value, Is.EqualTo(spectrumFileName1));
            Assert.That(properties.Last().Value, Is.EqualTo(spectrumFileName2));
        }

        [Test]
        public void GivenSpatiallyVaryingParameterizedSpectrumConstantBoundaryConditionWithTwoDataPoints_WhenCreatingDelftIniCategory_ThenCategoryWithExpectedSpectrumParameterValuesIsReturned()
        {
            // Given
            double height0 = randomNumberGenerator.NextDouble();
            double period0 = randomNumberGenerator.NextDouble();
            double direction0 = randomNumberGenerator.NextDouble();
            double spreading0 = randomNumberGenerator.NextDouble();

            double height1 = randomNumberGenerator.NextDouble();
            double period1 = randomNumberGenerator.NextDouble();
            double direction1 = randomNumberGenerator.NextDouble();
            double spreading1 = randomNumberGenerator.NextDouble();

            WaveBoundaryCondition waveBoundaryCondition = GetWaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant);
            waveBoundaryCondition.SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying;
            waveBoundaryCondition.AddPoint(0);
            waveBoundaryCondition.AddPoint(1);
            waveBoundaryCondition.SpectrumParameters[0] = new WaveBoundaryParameters
            {
                Height = height0,
                Period = period0,
                Direction = direction0,
                Spreading = spreading0
            };
            waveBoundaryCondition.SpectrumParameters[1] = new WaveBoundaryParameters
            {
                Height = height1,
                Period = period1,
                Direction = direction1,
                Spreading = spreading1
            };

            // When
            DelftIniCategory category = WaveDelftIniCategoryCreator.CreateBoundaryConditionCategory(waveBoundaryCondition);

            // Then
            DelftIniProperty[] heightProperties = category.Properties.Where(p => p.Name == KnownWaveProperties.WaveHeight).ToArray();
            Assert.That(heightProperties.Length, Is.EqualTo(2));
            Assert.That(heightProperties.First().Value, Is.EqualTo(GetStringValue(height0)));
            Assert.That(heightProperties.Last().Value, Is.EqualTo(GetStringValue(height1)));

            DelftIniProperty[] periodProperties = category.Properties.Where(p => p.Name == KnownWaveProperties.Period).ToArray();
            Assert.That(periodProperties.Length, Is.EqualTo(2));
            Assert.That(periodProperties.First().Value, Is.EqualTo(GetStringValue(period0)));
            Assert.That(periodProperties.Last().Value, Is.EqualTo(GetStringValue(period1)));

            DelftIniProperty[] directionProperties = category.Properties.Where(p => p.Name == KnownWaveProperties.Direction).ToArray();
            Assert.That(directionProperties.Length, Is.EqualTo(2));
            Assert.That(directionProperties.First().Value, Is.EqualTo(GetStringValue(direction0)));
            Assert.That(directionProperties.Last().Value, Is.EqualTo(GetStringValue(direction1)));

            DelftIniProperty[] spreadingProperties = category.Properties.Where(p => p.Name == KnownWaveProperties.DirectionalSpreadingValue).ToArray();
            Assert.That(spreadingProperties.Length, Is.EqualTo(2));
            Assert.That(spreadingProperties.First().Value, Is.EqualTo(GetStringValue(spreading0)));
            Assert.That(spreadingProperties.Last().Value, Is.EqualTo(GetStringValue(spreading1)));
        }

        private WaveBoundaryCondition GetWaveBoundaryCondition(BoundaryConditionDataType dataType)
        {
            var waveBoundaryCondition = new WaveBoundaryCondition(dataType)
            {
                Name = "myBoundaryCondition",
                Feature = new Feature2D
                {
                    Name = boundaryName,
                    Geometry = boundaryGeometry
                }
            };

            if (dataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                waveBoundaryCondition.SpectrumParameters[0] = new WaveBoundaryParameters();
            }

            if (dataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                waveBoundaryCondition.SpectrumFiles[0] = spectrumFileName1;
            }

            return waveBoundaryCondition;
        }

        private static string GetStringValue(double value)
        {
            return value.ToString("e7", CultureInfo.InvariantCulture);
        }

        private static IEnumerable<BoundaryConditionDataType> NonFileBasedDataTypes()
        {
            yield return BoundaryConditionDataType.Constant;
            yield return BoundaryConditionDataType.Empty;
            yield return BoundaryConditionDataType.AstroComponents;
            yield return BoundaryConditionDataType.AstroCorrection;
            yield return BoundaryConditionDataType.HarmonicCorrection;
            yield return BoundaryConditionDataType.Harmonics;
            yield return BoundaryConditionDataType.ParameterizedSpectrumConstant;
            yield return BoundaryConditionDataType.ParameterizedSpectrumTimeseries;
            yield return BoundaryConditionDataType.TimeSeries;
        }

        private static IEnumerable<BoundaryConditionDataType> UniformDefinitionDataTypes()
        {
            yield return BoundaryConditionDataType.Qh;
        }

        private static IEnumerable<BoundaryConditionDataType> FileBasedDataTypes()
        {
            yield return BoundaryConditionDataType.SpectrumFromFile;
        }
    }
}
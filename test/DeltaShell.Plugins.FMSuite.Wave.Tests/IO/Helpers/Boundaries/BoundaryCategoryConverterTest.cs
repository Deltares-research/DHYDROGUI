using System;
using System.Collections.Generic;
using System.Globalization;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class BoundaryCategoryConverterTest
    {
        private readonly Random random = new Random();

        private double RandomDouble => Math.Round(random.NextDouble(), 7);

        [Test]
        public void Converter_BoundaryCategoryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => BoundaryCategoryConverter.Convert(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategory"));
        }

        [Test]
        public void Convert_NoBoundaryCategory_ThrowsArgumentException()
        {
            // Call
            void Call() => BoundaryCategoryConverter.Convert(new DelftIniCategory("category"));

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategory"));
            Assert.That(exception.Message, Is.StringStarting("Category is not an mdw boundary category."));
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases))]
        public void Convert_ReturnsCorrectResult(string spectrumTypeStr, string shapeTypeStr,
                                                 string periodTypeStr, string spreadingTypeStr,
                                                 SpectrumType expectedSpectrumType, ShapeType expectedShapeType,
                                                 PeriodType expectedPeriodType, SpreadingType expectedSpreadingType)
        {
            // Setup
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            const string name = "boundary_name";
            const string definition = "boundary_definition";
            double startX = RandomDouble;
            double startY = RandomDouble;
            double endX = RandomDouble;
            double endY = RandomDouble;
            double distance1 = RandomDouble;
            double distance2 = RandomDouble;
            double waveHeight1 = RandomDouble;
            double waveHeight2 = RandomDouble;
            double period1 = RandomDouble;
            double period2 = RandomDouble;
            double direction1 = RandomDouble;
            double direction2 = RandomDouble;
            double spreading1 = RandomDouble;
            double spreading2 = RandomDouble;
            double peakEnhancementFactor = RandomDouble;
            double gaussianSpread = RandomDouble;

            category.AddProperty(KnownWaveProperties.Name, name);
            category.AddProperty(KnownWaveProperties.Definition, definition);
            category.AddProperty(KnownWaveProperties.StartCoordinateX, ToString(startX));
            category.AddProperty(KnownWaveProperties.StartCoordinateY, ToString(startY));
            category.AddProperty(KnownWaveProperties.EndCoordinateX, ToString(endX));
            category.AddProperty(KnownWaveProperties.EndCoordinateY, ToString(endY));
            category.AddProperty(KnownWaveProperties.SpectrumSpec, spectrumTypeStr);
            category.AddProperty(KnownWaveProperties.ShapeType, shapeTypeStr);
            category.AddProperty(KnownWaveProperties.PeriodType, periodTypeStr);
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingType, spreadingTypeStr);
            category.AddProperty(KnownWaveProperties.PeakEnhancementFactor, ToString(peakEnhancementFactor));
            category.AddProperty(KnownWaveProperties.GaussianSpreading, ToString(gaussianSpread));
            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance1));
            category.AddProperty(KnownWaveProperties.WaveHeight, ToString(waveHeight1));
            category.AddProperty(KnownWaveProperties.Period, ToString(period1));
            category.AddProperty(KnownWaveProperties.Direction, ToString(direction1));
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, ToString(spreading1));
            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance2));
            category.AddProperty(KnownWaveProperties.WaveHeight, ToString(waveHeight2));
            category.AddProperty(KnownWaveProperties.Period, ToString(period2));
            category.AddProperty(KnownWaveProperties.Direction, ToString(direction2));
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, ToString(spreading2));

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category);

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.Definition, Is.EqualTo(definition));
            Assert.That(result.XStartCoordinate, Is.EqualTo(startX));
            Assert.That(result.YStartCoordinate, Is.EqualTo(startY));
            Assert.That(result.XEndCoordinate, Is.EqualTo(endX));
            Assert.That(result.YEndCoordinate, Is.EqualTo(endY));
            Assert.That(result.SpectrumType, Is.EqualTo(expectedSpectrumType));
            Assert.That(result.ShapeType, Is.EqualTo(expectedShapeType));
            Assert.That(result.PeriodType, Is.EqualTo(expectedPeriodType));
            Assert.That(result.SpreadingType, Is.EqualTo(expectedSpreadingType));
            Assert.That(result.Distances, Is.EqualTo(Doubles(distance1, distance2)));
            Assert.That(result.WaveHeights, Is.EqualTo(Doubles(waveHeight1, waveHeight2)));
            Assert.That(result.Periods, Is.EqualTo(Doubles(period1, period2)));
            Assert.That(result.Directions, Is.EqualTo(Doubles(direction1, direction2)));
            Assert.That(result.DirectionalSpreadings, Is.EqualTo(Doubles(spreading1, spreading2)));
        }

        private static IEnumerable<TestCaseData> GetTestCases()
        {
            foreach (object[] spectrumTypeCase in SpectrumTypeTestCases())
            foreach (object[] shapeTypeCase in ShapeTypeTestCases())
            foreach (object[] periodTypeCase in PeriodTypeTestCases())
            foreach (object[] spreadingTypeCase in SpreadingTypeTestCases())
            {
                yield return new TestCaseData(
                    spectrumTypeCase[0], shapeTypeCase[0], periodTypeCase[0], spreadingTypeCase[0],
                    spectrumTypeCase[1], shapeTypeCase[1], periodTypeCase[1], spreadingTypeCase[1]);
            }
        }

        private static IEnumerable<object[]> SpectrumTypeTestCases()
        {
            yield return SubTestCase("from file", SpectrumType.FromFile);
            yield return SubTestCase("parametric", SpectrumType.Parametrized);
        }

        private static IEnumerable<object[]> ShapeTypeTestCases()
        {
            yield return SubTestCase("gauss", ShapeType.Gauss);
            yield return SubTestCase("jonswap", ShapeType.Jonswap);
            yield return SubTestCase("pierson-moskowitz", ShapeType.PiersonMoskowitz);
        }

        private static IEnumerable<object[]> PeriodTypeTestCases()
        {
            yield return SubTestCase("mean", PeriodType.Mean);
            yield return SubTestCase("peak", PeriodType.Peak);
        }

        private static IEnumerable<object[]> SpreadingTypeTestCases()
        {
            yield return SubTestCase("degrees", SpreadingType.Degrees);
            yield return SubTestCase("power", SpreadingType.Power);
        }

        private static object[] SubTestCase(string value, object expected)
        {
            return new[]
            {
                value,
                expected
            };
        }

        private static IEnumerable<double> Doubles(double a, double b)
        {
            return new[]
            {
                a,
                b
            };
        }

        private static string ToString(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
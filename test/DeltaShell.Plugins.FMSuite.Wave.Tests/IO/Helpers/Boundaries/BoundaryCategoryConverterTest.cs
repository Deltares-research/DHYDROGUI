using System;
using System.Collections.Generic;
using System.Globalization;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class BoundaryCategoryConverterTest
    {
        private readonly Random random = new Random();

        private double RandomDouble => Math.Round(random.NextDouble(), 3);
        private int RandomInt => random.Next(100);

        [Test]
        public void Convert_BoundaryCategoryNull_ThrowsArgumentNullException()
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
        [TestCaseSource(nameof(GetParameterizedTestCases))]
        public void Convert_ParameterizedImportType_ReturnsCorrectResult(string shapeTypeStr, ShapeImportType expectedShapeType,
                                                                         string periodTypeStr, PeriodImportExportType expectedPeriodType,
                                                                         string spreadingTypeStr, SpreadingImportType expectedSpreadingType,
                                                                         string definitionTypeStr, DefinitionImportType expectedDefinitionType)
        {
            // Setup
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            const string name = "boundary_name";
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
            category.AddProperty(KnownWaveProperties.Definition, definitionTypeStr);
            category.AddProperty(KnownWaveProperties.StartCoordinateX, ToString(startX));
            category.AddProperty(KnownWaveProperties.StartCoordinateY, ToString(startY));
            category.AddProperty(KnownWaveProperties.EndCoordinateX, ToString(endX));
            category.AddProperty(KnownWaveProperties.EndCoordinateY, ToString(endY));
            category.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
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
            Assert.That(result.DefinitionType, Is.EqualTo(expectedDefinitionType));
            Assert.That(result.XStartCoordinate, Is.EqualTo(startX));
            Assert.That(result.YStartCoordinate, Is.EqualTo(startY));
            Assert.That(result.XEndCoordinate, Is.EqualTo(endX));
            Assert.That(result.YEndCoordinate, Is.EqualTo(endY));
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            Assert.That(result.ShapeType, Is.EqualTo(expectedShapeType));
            Assert.That(result.PeriodType, Is.EqualTo(expectedPeriodType));
            Assert.That(result.SpreadingType, Is.EqualTo(expectedSpreadingType));
            Assert.That(result.Distances, Is.EqualTo(Doubles(distance1, distance2)));
            Assert.That(result.WaveHeights, Is.EqualTo(Doubles(waveHeight1, waveHeight2)));
            Assert.That(result.Periods, Is.EqualTo(Doubles(period1, period2)));
            Assert.That(result.Directions, Is.EqualTo(Doubles(direction1, direction2)));
            Assert.That(result.DirectionalSpreadings, Is.EqualTo(Doubles(spreading1, spreading2)));
        }

        [Test]
        [TestCaseSource(nameof(GetFromFileTestCases))]
        public void Convert_FromFileImportType_ReturnsCorrectResult(string shapeTypeStr, string periodTypeStr, string spreadingTypeStr, string definitionTypeStr, DefinitionImportType expectedDefinitionType)
        {
            // Setup
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            const string name = "boundary_name";
            double startX = RandomDouble;
            double startY = RandomDouble;
            double endX = RandomDouble;
            double endY = RandomDouble;
            double distance1 = RandomDouble;
            double distance2 = RandomDouble;
            string spectrum1 = "spectrum file " + RandomInt;
            string spectrum2 = "spectrum file " + RandomInt;
            double peakEnhancementFactor = RandomDouble;
            double gaussianSpread = RandomDouble;

            category.AddProperty(KnownWaveProperties.Name, name);
            category.AddProperty(KnownWaveProperties.Definition, definitionTypeStr);
            category.AddProperty(KnownWaveProperties.StartCoordinateX, ToString(startX));
            category.AddProperty(KnownWaveProperties.StartCoordinateY, ToString(startY));
            category.AddProperty(KnownWaveProperties.EndCoordinateX, ToString(endX));
            category.AddProperty(KnownWaveProperties.EndCoordinateY, ToString(endY));
            category.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.FromFile.GetDescription());
            category.AddProperty(KnownWaveProperties.ShapeType, shapeTypeStr);
            category.AddProperty(KnownWaveProperties.PeriodType, periodTypeStr);
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingType, spreadingTypeStr);
            category.AddProperty(KnownWaveProperties.PeakEnhancementFactor, ToString(peakEnhancementFactor));
            category.AddProperty(KnownWaveProperties.GaussianSpreading, ToString(gaussianSpread));
            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance1));
            category.AddProperty(KnownWaveProperties.Spectrum, spectrum1);
            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance2));
            category.AddProperty(KnownWaveProperties.Spectrum, spectrum2);

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category);

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(expectedDefinitionType));
            Assert.That(result.XStartCoordinate, Is.EqualTo(startX));
            Assert.That(result.YStartCoordinate, Is.EqualTo(startY));
            Assert.That(result.XEndCoordinate, Is.EqualTo(endX));
            Assert.That(result.YEndCoordinate, Is.EqualTo(endY));
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));
            Assert.That(result.ShapeType, Is.EqualTo(ShapeImportType.Gauss));
            Assert.That(result.PeriodType, Is.EqualTo(PeriodImportExportType.Mean));
            Assert.That(result.SpreadingType, Is.EqualTo(SpreadingImportType.Degrees));
            Assert.That(result.Distances, Is.EqualTo(Doubles(distance1, distance2)));
            Assert.That(result.SpectrumFiles, Is.EqualTo(new[]
            {
                spectrum1,
                spectrum2
            }));
        }

        [Test]
        public void Convert_PropertyWithDoubleValueNotFound_ReturnsCorrectResult()
        {
            // Setup
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            category.AddProperty(KnownWaveProperties.Name, "boundary_name");
            category.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            category.AddProperty(KnownWaveProperties.Definition, random.NextEnumValue<DefinitionImportType>().GetDescription());
            category.AddProperty(KnownWaveProperties.ShapeType, random.NextEnumValue<ShapeImportType>().GetDescription());
            category.AddProperty(KnownWaveProperties.PeriodType, random.NextEnumValue<PeriodImportExportType>().GetDescription());
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingType, random.NextEnumValue<SpreadingImportType>().GetDescription());

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category);

            // Assert
            Assert.That(result.XStartCoordinate, Is.NaN);
            Assert.That(result.YStartCoordinate, Is.NaN);
            Assert.That(result.XEndCoordinate, Is.NaN);
            Assert.That(result.YEndCoordinate, Is.NaN);
            Assert.That(result.Spreading, Is.NaN);
            Assert.That(result.PeakEnhancementFactor, Is.NaN);
        }

        private static IEnumerable<TestCaseData> GetParameterizedTestCases()
        {
            foreach (object[] shapeTypeCase in ShapeTypeTestCases())
            foreach (object[] periodTypeCase in PeriodTypeTestCases())
            foreach (object[] spreadingTypeCase in SpreadingTypeTestCases())
            foreach (object[] definitionTypeCase in DefinitionTypeTestCases())
            {
                yield return new TestCaseData(shapeTypeCase[0], shapeTypeCase[1],
                                              periodTypeCase[0], periodTypeCase[1],
                                              spreadingTypeCase[0], spreadingTypeCase[1],
                                              definitionTypeCase[0], definitionTypeCase[1]);
            }
        }

        private static IEnumerable<TestCaseData> GetFromFileTestCases()
        {
            foreach (object[] shapeTypeCase in ShapeTypeTestCases())
            foreach (object[] periodTypeCase in PeriodTypeTestCases())
            foreach (object[] spreadingTypeCase in SpreadingTypeTestCases())
            foreach (object[] definitionTypeCase in DefinitionTypeTestCases())
            {
                yield return new TestCaseData(shapeTypeCase[0],
                                              periodTypeCase[0],
                                              spreadingTypeCase[0],
                                              definitionTypeCase[0],
                                              definitionTypeCase[1]);
            }
        }

        private static IEnumerable<object[]> ShapeTypeTestCases()
        {
            yield return SubTestCase("gauss", ShapeImportType.Gauss);
            yield return SubTestCase("jonswap", ShapeImportType.Jonswap);
            yield return SubTestCase("pierson-moskowitz", ShapeImportType.PiersonMoskowitz);
        }

        private static IEnumerable<object[]> PeriodTypeTestCases()
        {
            yield return SubTestCase("mean", PeriodImportExportType.Mean);
            yield return SubTestCase("peak", PeriodImportExportType.Peak);
        }

        private static IEnumerable<object[]> SpreadingTypeTestCases()
        {
            yield return SubTestCase("degrees", SpreadingImportType.Degrees);
            yield return SubTestCase("power", SpreadingImportType.Power);
        }

        private static IEnumerable<object[]> DefinitionTypeTestCases()
        {
            yield return SubTestCase("xy-coordinates", DefinitionImportType.Coordinates);
            yield return SubTestCase("orientation", DefinitionImportType.Oriented);
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

        private static string ToString(double value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
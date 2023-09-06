using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class BoundaryCategoryConverterTest
    {
        private const double doublePrecision = 1E-7;
        private readonly Random random = new Random();
        private double RandomDouble => Math.Round(random.NextDouble(), 8);
        private int RandomInt => random.Next(100);

        [Test]
        public void Convert_BoundarySectionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => BoundaryCategoryConverter.Convert(null, "path");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundarySection"));
        }

        [Test]
        public void Convert_MdwDirPathNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => BoundaryCategoryConverter.Convert(new IniSection("section"), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("mdwDirPath"));
        }

        [Test]
        public void Convert_NoBoundarySection_ThrowsArgumentException()
        {
            // Call
            void Call() => BoundaryCategoryConverter.Convert(new IniSection("section"), "path");

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundarySection"));
            Assert.That(exception.Message, Does.StartWith("Section is not an mdw boundary section."));
        }

        [Test]
        [TestCaseSource(nameof(GetParameterizedCoordinatesTestCases))]
        public void Convert_ParameterizedImportTypeCoordinates_ReturnsCorrectResult(SectionTestKeyValue<ShapeImportType> shapeImportData,
                                                                                    SectionTestKeyValue<PeriodImportExportType> periodImportExportData,
                                                                                    SectionTestKeyValue<SpreadingImportType> spreadingImportData)
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);

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

            section.AddProperty(KnownWaveProperties.Name, name);
            section.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.CoordinatesDefinitionType);
            section.AddProperty(KnownWaveProperties.StartCoordinateX, ToString(startX));
            section.AddProperty(KnownWaveProperties.StartCoordinateY, ToString(startY));
            section.AddProperty(KnownWaveProperties.EndCoordinateX, ToString(endX));
            section.AddProperty(KnownWaveProperties.EndCoordinateY, ToString(endY));
            section.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            AddToSection(section, shapeImportData);
            AddToSection(section, periodImportExportData);
            AddToSection(section, spreadingImportData);
            section.AddProperty(KnownWaveProperties.PeakEnhancementFactor, ToString(peakEnhancementFactor));
            section.AddProperty(KnownWaveProperties.GaussianSpreading, ToString(gaussianSpread));
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance1));
            section.AddProperty(KnownWaveProperties.WaveHeight, ToString(waveHeight1));
            section.AddProperty(KnownWaveProperties.Period, ToString(period1));
            section.AddProperty(KnownWaveProperties.Direction, ToString(direction1));
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, ToString(spreading1));
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance2));
            section.AddProperty(KnownWaveProperties.WaveHeight, ToString(waveHeight2));
            section.AddProperty(KnownWaveProperties.Period, ToString(period2));
            section.AddProperty(KnownWaveProperties.Direction, ToString(direction2));
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, ToString(spreading2));

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(section, "path");

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.Coordinates));
            AssertRoundedValue(result.XStartCoordinate, startX);
            AssertRoundedValue(result.YStartCoordinate, startY);
            AssertRoundedValue(result.XEndCoordinate, endX);
            AssertRoundedValue(result.YEndCoordinate, endY);
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            Assert.That(result.ShapeType, Is.EqualTo(shapeImportData.ExpectedValue));
            Assert.That(result.PeriodType, Is.EqualTo(periodImportExportData.ExpectedValue));
            Assert.That(result.SpreadingType, Is.EqualTo(spreadingImportData.ExpectedValue));
            AssertRoundedValues(result.Distances, Doubles(distance1, distance2));
            Assert.That(result.WaveHeights, Is.EqualTo(Doubles(waveHeight1, waveHeight2)).Within(doublePrecision));
            Assert.That(result.Periods, Is.EqualTo(Doubles(period1, period2)).Within(doublePrecision));
            Assert.That(result.Directions, Is.EqualTo(Doubles(direction1, direction2)).Within(doublePrecision));
            Assert.That(result.DirectionalSpreadings, Is.EqualTo(Doubles(spreading1, spreading2)).Within(doublePrecision));
        }

        [Test]
        [TestCaseSource(nameof(GetParameterizedOrientedTestCases))]
        public void Convert_ParameterizedImportTypeOriented_ReturnsCorrectResult(SectionTestKeyValue<ShapeImportType> shapeImportData,
                                                                                 SectionTestKeyValue<PeriodImportExportType> periodImportExportData,
                                                                                 SectionTestKeyValue<SpreadingImportType> spreadingImportData,
                                                                                 SectionTestKeyValue<BoundaryOrientationType> orientationTypeData,
                                                                                 SectionTestKeyValue<DistanceDirType> distanceDirType)
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);

            const string name = "boundary_name";
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

            section.AddProperty(KnownWaveProperties.Name, name);
            section.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.OrientationDefinitionType);
            AddToSection(section, orientationTypeData);
            AddToSection(section, distanceDirType);
            section.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            AddToSection(section, shapeImportData);
            AddToSection(section, periodImportExportData);
            AddToSection(section, spreadingImportData);
            section.AddProperty(KnownWaveProperties.PeakEnhancementFactor, ToString(peakEnhancementFactor));
            section.AddProperty(KnownWaveProperties.GaussianSpreading, ToString(gaussianSpread));
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance1));
            section.AddProperty(KnownWaveProperties.WaveHeight, ToString(waveHeight1));
            section.AddProperty(KnownWaveProperties.Period, ToString(period1));
            section.AddProperty(KnownWaveProperties.Direction, ToString(direction1));
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, ToString(spreading1));
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance2));
            section.AddProperty(KnownWaveProperties.WaveHeight, ToString(waveHeight2));
            section.AddProperty(KnownWaveProperties.Period, ToString(period2));
            section.AddProperty(KnownWaveProperties.Direction, ToString(direction2));
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, ToString(spreading2));

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(section, "path");

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.Oriented));
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            Assert.That(result.OrientationType, Is.EqualTo(orientationTypeData.ExpectedValue));
            Assert.That(result.DistanceDirType, Is.EqualTo(distanceDirType.ExpectedValue));
            Assert.That(result.ShapeType, Is.EqualTo(shapeImportData.ExpectedValue));
            Assert.That(result.PeriodType, Is.EqualTo(periodImportExportData.ExpectedValue));
            Assert.That(result.SpreadingType, Is.EqualTo(spreadingImportData.ExpectedValue));
            AssertRoundedValues(result.Distances, Doubles(distance1, distance2));
            Assert.That(result.WaveHeights, Is.EqualTo(Doubles(waveHeight1, waveHeight2)).Within(doublePrecision));
            Assert.That(result.Periods, Is.EqualTo(Doubles(period1, period2)).Within(doublePrecision));
            Assert.That(result.Directions, Is.EqualTo(Doubles(direction1, direction2)).Within(doublePrecision));
            Assert.That(result.DirectionalSpreadings, Is.EqualTo(Doubles(spreading1, spreading2)).Within(doublePrecision));
        }

        [Test]
        [TestCaseSource(nameof(GetFromFileCoordinatesTestCases))]
        public void Convert_FromFileImportTypeCoordinates_ReturnsCorrectResult(string shapeTypeStr,
                                                                               string periodTypeStr,
                                                                               string spreadingTypeStr)
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);

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

            section.AddProperty(KnownWaveProperties.Name, name);
            section.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.CoordinatesDefinitionType);
            section.AddProperty(KnownWaveProperties.StartCoordinateX, ToString(startX));
            section.AddProperty(KnownWaveProperties.StartCoordinateY, ToString(startY));
            section.AddProperty(KnownWaveProperties.EndCoordinateX, ToString(endX));
            section.AddProperty(KnownWaveProperties.EndCoordinateY, ToString(endY));
            section.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.FromFile.GetDescription());
            section.AddProperty(KnownWaveProperties.ShapeType, shapeTypeStr);
            section.AddProperty(KnownWaveProperties.PeriodType, periodTypeStr);
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingType, spreadingTypeStr);
            section.AddProperty(KnownWaveProperties.PeakEnhancementFactor, ToString(peakEnhancementFactor));
            section.AddProperty(KnownWaveProperties.GaussianSpreading, ToString(gaussianSpread));
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance1));
            section.AddProperty(KnownWaveProperties.Spectrum, spectrum1);
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance2));
            section.AddProperty(KnownWaveProperties.Spectrum, spectrum2);

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(section, @"C:\path\to\mdw");

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.Coordinates));
            AssertRoundedValue(result.XStartCoordinate, startX);
            AssertRoundedValue(result.YStartCoordinate, startY);
            AssertRoundedValue(result.XEndCoordinate, endX);
            AssertRoundedValue(result.YEndCoordinate, endY);
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));
            Assert.That(result.ShapeType, Is.EqualTo(ShapeImportType.Gauss));
            Assert.That(result.PeriodType, Is.EqualTo(PeriodImportExportType.Mean));
            Assert.That(result.SpreadingType, Is.EqualTo(SpreadingImportType.Degrees));
            AssertRoundedValues(result.Distances, Doubles(distance1, distance2));
            Assert.That(result.SpectrumFiles, Is.EqualTo(new[]
            {
                @"C:\path\to\mdw\" + spectrum1,
                @"C:\path\to\mdw\" + spectrum2
            }));
        }

        [Test]
        [TestCaseSource(nameof(GetFromFileOrientedTestCases))]
        public void Convert_FromFileImportTypeOriented_ReturnsCorrectResult(string shapeTypeStr,
                                                                            string periodTypeStr,
                                                                            string spreadingTypeStr,
                                                                            SectionTestKeyValue<BoundaryOrientationType> orientationTypeData,
                                                                            SectionTestKeyValue<DistanceDirType> distanceDirType)
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);

            const string name = "boundary_name";
            double distance1 = RandomDouble;
            double distance2 = RandomDouble;
            string spectrum1 = "spectrum file " + RandomInt;
            string spectrum2 = "spectrum file " + RandomInt;
            double peakEnhancementFactor = RandomDouble;
            double gaussianSpread = RandomDouble;

            section.AddProperty(KnownWaveProperties.Name, name);
            section.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.OrientationDefinitionType);
            AddToSection(section, orientationTypeData);
            AddToSection(section, distanceDirType);
            section.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.FromFile.GetDescription());
            section.AddProperty(KnownWaveProperties.ShapeType, shapeTypeStr);
            section.AddProperty(KnownWaveProperties.PeriodType, periodTypeStr);
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingType, spreadingTypeStr);
            section.AddProperty(KnownWaveProperties.PeakEnhancementFactor, ToString(peakEnhancementFactor));
            section.AddProperty(KnownWaveProperties.GaussianSpreading, ToString(gaussianSpread));
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance1));
            section.AddProperty(KnownWaveProperties.Spectrum, spectrum1);
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(distance2));
            section.AddProperty(KnownWaveProperties.Spectrum, spectrum2);

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(section, @"C:\path\to\mdw");

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.Oriented));
            Assert.That(result.OrientationType, Is.EqualTo(orientationTypeData.ExpectedValue));
            Assert.That(result.DistanceDirType, Is.EqualTo(distanceDirType.ExpectedValue));
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));
            Assert.That(result.ShapeType, Is.EqualTo(ShapeImportType.Gauss));
            Assert.That(result.PeriodType, Is.EqualTo(PeriodImportExportType.Mean));
            Assert.That(result.SpreadingType, Is.EqualTo(SpreadingImportType.Degrees));
            AssertRoundedValues(result.Distances, Doubles(distance1, distance2));
            Assert.That(result.SpectrumFiles, Is.EqualTo(new[]
            {
                @"C:\path\to\mdw\" + spectrum1,
                @"C:\path\to\mdw\" + spectrum2
            }));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Convert_FileBasedBoundaryWithoutSpectrumPath_ReturnsCorrectResult(string fileName)
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);

            section.AddProperty(KnownWaveProperties.Name, "boundary_name");
            section.AddProperty(KnownWaveProperties.Definition, DefinitionImportType.Coordinates.GetDescription());
            section.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.FromFile.GetDescription());
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, RandomDouble);
            section.AddProperty(KnownWaveProperties.Spectrum, fileName);

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(section, "path");

            // Assert
            Assert.That(result.SpectrumFiles[0], Is.Empty);
        }

        [Test]
        public void Convert_PropertyWithDoubleValueNotFound_ReturnsCorrectResult()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);

            section.AddProperty(KnownWaveProperties.Name, "boundary_name");
            section.AddProperty(KnownWaveProperties.Definition, DefinitionImportType.Coordinates.GetDescription());
            section.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            section.AddProperty(KnownWaveProperties.ShapeType, random.NextEnumValue<ShapeImportType>().GetDescription());
            section.AddProperty(KnownWaveProperties.PeriodType, random.NextEnumValue<PeriodImportExportType>().GetDescription());
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingType, random.NextEnumValue<SpreadingImportType>().GetDescription());

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(section, "path");

            // Assert
            Assert.That(result.XStartCoordinate, Is.NaN);
            Assert.That(result.YStartCoordinate, Is.NaN);
            Assert.That(result.XEndCoordinate, Is.NaN);
            Assert.That(result.YEndCoordinate, Is.NaN);
            Assert.That(result.Spreading, Is.NaN);
            Assert.That(result.PeakEnhancementFactor, Is.NaN);
        }

        [Test]
        public void Convert_OrientedBoundaryButDistanceDirNotSet_ReturnsCorrectResult()
        {
            // Setup
            var section = new IniSection(KnownWaveSections.BoundarySection);

            section.AddProperty(KnownWaveProperties.Name, "boundary_name");
            section.AddProperty(KnownWaveProperties.Definition, DefinitionImportType.Oriented.GetDescription());
            section.AddProperty(KnownWaveProperties.Orientation, BoundaryOrientationType.East.GetDescription());
            section.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            section.AddProperty(KnownWaveProperties.ShapeType, random.NextEnumValue<ShapeImportType>().GetDescription());
            section.AddProperty(KnownWaveProperties.PeriodType, random.NextEnumValue<PeriodImportExportType>().GetDescription());
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingType, random.NextEnumValue<SpreadingImportType>().GetDescription());

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(section, "path");

            // Assert
            Assert.That(result.DistanceDirType, Is.EqualTo(DistanceDirType.CounterClockwise));
        }

        private static IEnumerable<TestCaseData> GetParameterizedCoordinatesTestCases() =>
            from shapeTypeCase in ShapeTypeTestCases()
            from periodTypeCase in PeriodTypeTestCases()
            from spreadingTypeCase in SpreadingTypeTestCases()
            select new TestCaseData(shapeTypeCase,
                                    periodTypeCase,
                                    spreadingTypeCase);

        private static IEnumerable<TestCaseData> GetParameterizedOrientedTestCases() =>
            from shapeTypeCase in ShapeTypeTestCases()
            from periodTypeCase in PeriodTypeTestCases()
            from spreadingTypeCase in SpreadingTypeTestCases()
            from orientationTypeCase in OrientationTypeTestCases()
            from distanceDirTypeCase in DistanceDirTypeTestCases()
            select new TestCaseData(shapeTypeCase,
                                    periodTypeCase,
                                    spreadingTypeCase,
                                    orientationTypeCase,
                                    distanceDirTypeCase);

        private static IEnumerable<TestCaseData> GetFromFileCoordinatesTestCases() =>
            from shapeTypeCase in ShapeTypeTestCases()
            from periodTypeCase in PeriodTypeTestCases()
            from spreadingTypeCase in SpreadingTypeTestCases()
            select new TestCaseData(shapeTypeCase.StringValue,
                                    periodTypeCase.StringValue,
                                    spreadingTypeCase.StringValue);

        private static IEnumerable<TestCaseData> GetFromFileOrientedTestCases() =>
            from shapeTypeCase in ShapeTypeTestCases()
            from periodTypeCase in PeriodTypeTestCases()
            from spreadingTypeCase in SpreadingTypeTestCases()
            from orientationTypeCase in OrientationTypeTestCases()
            from distanceDirTypeCase in DistanceDirTypeTestCases()
            select new TestCaseData(shapeTypeCase.StringValue,
                                    periodTypeCase.StringValue,
                                    spreadingTypeCase.StringValue,
                                    orientationTypeCase,
                                    distanceDirTypeCase);

        private static IEnumerable<SectionTestKeyValue<ShapeImportType>> ShapeTypeTestCases()
        {
            yield return new SectionTestKeyValue<ShapeImportType>(KnownWaveProperties.ShapeType,
                                                                   "gauss",
                                                                   ShapeImportType.Gauss);
            yield return new SectionTestKeyValue<ShapeImportType>(KnownWaveProperties.ShapeType,
                                                                   "jonswap",
                                                                   ShapeImportType.Jonswap);
            yield return new SectionTestKeyValue<ShapeImportType>(KnownWaveProperties.ShapeType,
                                                                   "pierson-moskowitz",
                                                                   ShapeImportType.PiersonMoskowitz);
        }

        private static IEnumerable<SectionTestKeyValue<PeriodImportExportType>> PeriodTypeTestCases()
        {
            yield return new SectionTestKeyValue<PeriodImportExportType>(KnownWaveProperties.PeriodType,
                                                                          "mean",
                                                                          PeriodImportExportType.Mean);
            yield return new SectionTestKeyValue<PeriodImportExportType>(KnownWaveProperties.PeriodType,
                                                                          "peak",
                                                                          PeriodImportExportType.Peak);
        }

        private static IEnumerable<SectionTestKeyValue<SpreadingImportType>> SpreadingTypeTestCases()
        {
            yield return new SectionTestKeyValue<SpreadingImportType>(KnownWaveProperties.DirectionalSpreadingType,
                                                                       "degrees",
                                                                       SpreadingImportType.Degrees);
            yield return new SectionTestKeyValue<SpreadingImportType>(KnownWaveProperties.DirectionalSpreadingType,
                                                                       "power",
                                                                       SpreadingImportType.Power);
        }

        private static IEnumerable<SectionTestKeyValue<BoundaryOrientationType>> OrientationTypeTestCases()
        {
            yield return new SectionTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.EastBoundaryOrientationType,
                                                                           BoundaryOrientationType.East);
            yield return new SectionTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.NorthEastBoundaryOrientationType,
                                                                           BoundaryOrientationType.NorthEast);
            yield return new SectionTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.NorthBoundaryOrientationType,
                                                                           BoundaryOrientationType.North);
            yield return new SectionTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.NorthWestBoundaryOrientationType,
                                                                           BoundaryOrientationType.NorthWest);
            yield return new SectionTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.WestBoundaryOrientationType,
                                                                           BoundaryOrientationType.West);
            yield return new SectionTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.SouthWestBoundaryOrientationType,
                                                                           BoundaryOrientationType.SouthWest);
            yield return new SectionTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.SouthBoundaryOrientationType,
                                                                           BoundaryOrientationType.South);
            yield return new SectionTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.SouthEastBoundaryOrientationType,
                                                                           BoundaryOrientationType.SouthEast);
        }

        private static IEnumerable<SectionTestKeyValue<DistanceDirType>> DistanceDirTypeTestCases()
        {
            yield return new SectionTestKeyValue<DistanceDirType>(KnownWaveProperties.DistanceDir,
                                                                   KnownWaveBoundariesFileConstants.CounterClockwiseDistanceDirType,
                                                                   DistanceDirType.CounterClockwise);
            yield return new SectionTestKeyValue<DistanceDirType>(KnownWaveProperties.DistanceDir,
                                                                   KnownWaveBoundariesFileConstants.ClockwiseDistanceDirType,
                                                                   DistanceDirType.Clockwise);
        }

        private static void AssertRoundedValue(double actual, double expected)
        {
            Assert.That(actual, Is.EqualTo(Math.Round(expected, 7, MidpointRounding.AwayFromZero)).Within(doublePrecision));
        }

        private static void AssertRoundedValues(double[] actual, double[] expected)
        {
            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            for (var i = 0; i < actual.Length; i++)
            {
                AssertRoundedValue(actual[i], expected[i]);
            }
        }

        public class SectionTestKeyValue<T>
        {
            public SectionTestKeyValue(string key, string stringValue, T expectedValue)
            {
                Key = key;
                StringValue = stringValue;
                ExpectedValue = expectedValue;
            }

            public string Key { get; }
            public string StringValue { get; }
            public T ExpectedValue { get; }
        }

        private static void AddToSection<T>(IniSection section, SectionTestKeyValue<T> keyValuePair) =>
            section.AddProperty(keyValuePair.Key, keyValuePair.StringValue);

        private static double[] Doubles(double a, double b)
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
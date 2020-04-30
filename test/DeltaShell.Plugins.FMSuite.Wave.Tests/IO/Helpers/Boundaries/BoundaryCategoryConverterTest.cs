using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            void Call() => BoundaryCategoryConverter.Convert(null, "path");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategory"));
        }

        [Test]
        public void Convert_NoBoundaryCategory_ThrowsArgumentException()
        {
            // Call
            void Call() => BoundaryCategoryConverter.Convert(new DelftIniCategory("category"), "path");

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategory"));
            Assert.That(exception.Message, Is.StringStarting("Category is not an mdw boundary category."));
        }

        [Test]
        public void Convert_SpectrumFileDefinitionType_ReturnsCorrectResult()
        {
            // Setup
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            category.AddProperty(KnownWaveProperties.Definition, DefinitionImportType.SpectrumFile.GetDescription());

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category, "path");

            // Assert
            Assert.That(result.Name, Is.Null);
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.SpectrumFile));
        }

        [Test]
        [TestCaseSource(nameof(GetParameterizedCoordinatesTestCases))]
        public void Convert_ParameterizedImportTypeCoordinates_ReturnsCorrectResult(CategoryTestKeyValue<ShapeImportType> shapeImportData,
                                                                                    CategoryTestKeyValue<PeriodImportExportType> periodImportExportData,
                                                                                    CategoryTestKeyValue<SpreadingImportType> spreadingImportData)
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
            category.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.CoordinatesDefinitionType);
            category.AddProperty(KnownWaveProperties.StartCoordinateX, ToString(startX));
            category.AddProperty(KnownWaveProperties.StartCoordinateY, ToString(startY));
            category.AddProperty(KnownWaveProperties.EndCoordinateX, ToString(endX));
            category.AddProperty(KnownWaveProperties.EndCoordinateY, ToString(endY));
            category.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            AddToCategory(category, shapeImportData); 
            AddToCategory(category, periodImportExportData); 
            AddToCategory(category, spreadingImportData); 
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
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category, "path");

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.Coordinates));
            Assert.That(result.XStartCoordinate, Is.EqualTo(startX));
            Assert.That(result.YStartCoordinate, Is.EqualTo(startY));
            Assert.That(result.XEndCoordinate, Is.EqualTo(endX));
            Assert.That(result.YEndCoordinate, Is.EqualTo(endY));
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            Assert.That(result.ShapeType, Is.EqualTo(shapeImportData.ExpectedValue));
            Assert.That(result.PeriodType, Is.EqualTo(periodImportExportData.ExpectedValue));
            Assert.That(result.SpreadingType, Is.EqualTo(spreadingImportData.ExpectedValue));
            Assert.That(result.Distances, Is.EqualTo(Doubles(distance1, distance2)));
            Assert.That(result.WaveHeights, Is.EqualTo(Doubles(waveHeight1, waveHeight2)));
            Assert.That(result.Periods, Is.EqualTo(Doubles(period1, period2)));
            Assert.That(result.Directions, Is.EqualTo(Doubles(direction1, direction2)));
            Assert.That(result.DirectionalSpreadings, Is.EqualTo(Doubles(spreading1, spreading2)));
        }

        [Test]
        [TestCaseSource(nameof(GetParameterizedOrientedTestCases))]
        public void Convert_ParameterizedImportTypeOriented_ReturnsCorrectResult(CategoryTestKeyValue<ShapeImportType> shapeImportData,
                                                                                 CategoryTestKeyValue<PeriodImportExportType> periodImportExportData,
                                                                                 CategoryTestKeyValue<SpreadingImportType> spreadingImportData,
                                                                                 CategoryTestKeyValue<BoundaryOrientationType> orientationTypeData,
                                                                                 CategoryTestKeyValue<DistanceDirType> distanceDirType)
        {
            // Setup
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

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

            category.AddProperty(KnownWaveProperties.Name, name);
            category.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.OrientationDefinitionType); 
            AddToCategory(category, orientationTypeData);
            AddToCategory(category, distanceDirType);
            category.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            AddToCategory(category, shapeImportData); 
            AddToCategory(category, periodImportExportData); 
            AddToCategory(category, spreadingImportData); 
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
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category, "path");

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.Oriented));
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            Assert.That(result.OrientationType, Is.EqualTo(orientationTypeData.ExpectedValue));
            Assert.That(result.DistanceDirType, Is.EqualTo(distanceDirType.ExpectedValue));
            Assert.That(result.ShapeType, Is.EqualTo(shapeImportData.ExpectedValue));
            Assert.That(result.PeriodType, Is.EqualTo(periodImportExportData.ExpectedValue));
            Assert.That(result.SpreadingType, Is.EqualTo(spreadingImportData.ExpectedValue));
            Assert.That(result.Distances, Is.EqualTo(Doubles(distance1, distance2)));
            Assert.That(result.WaveHeights, Is.EqualTo(Doubles(waveHeight1, waveHeight2)));
            Assert.That(result.Periods, Is.EqualTo(Doubles(period1, period2)));
            Assert.That(result.Directions, Is.EqualTo(Doubles(direction1, direction2)));
            Assert.That(result.DirectionalSpreadings, Is.EqualTo(Doubles(spreading1, spreading2)));
        }


        [Test]
        [TestCaseSource(nameof(GetFromFileCoordinatesTestCases))]
        public void Convert_FromFileImportType_Coordinates_ReturnsCorrectResult(string shapeTypeStr, 
                                                                    string periodTypeStr, 
                                                                    string spreadingTypeStr)
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
            category.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.CoordinatesDefinitionType);
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
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category, @"C:\path\to\mdw");

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.Coordinates));
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
                @"C:\path\to\mdw\" + spectrum1,
                @"C:\path\to\mdw\" + spectrum2
            }));
        }

        [Test]
        [TestCaseSource(nameof(GetFromFileOrientedTestCases))]
        public void Convert_FromFileImportType_Oriented_ReturnsCorrectResult(string shapeTypeStr, 
                                                                    string periodTypeStr, 
                                                                    string spreadingTypeStr,
                                                                    CategoryTestKeyValue<BoundaryOrientationType> orientationTypeData,
                                                                    CategoryTestKeyValue<DistanceDirType> distanceDirType)
        {
            // Setup
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            const string name = "boundary_name";
            double distance1 = RandomDouble;
            double distance2 = RandomDouble;
            string spectrum1 = "spectrum file " + RandomInt;
            string spectrum2 = "spectrum file " + RandomInt;
            double peakEnhancementFactor = RandomDouble;
            double gaussianSpread = RandomDouble;

            category.AddProperty(KnownWaveProperties.Name, name);
            category.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.OrientationDefinitionType);
            AddToCategory(category, orientationTypeData);
            AddToCategory(category, distanceDirType);
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
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category, @"C:\path\to\mdw");

            // Assert
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.DefinitionType, Is.EqualTo(DefinitionImportType.Oriented));
            Assert.That(result.OrientationType, Is.EqualTo(orientationTypeData.ExpectedValue));
            Assert.That(result.DistanceDirType, Is.EqualTo(distanceDirType.ExpectedValue));
            Assert.That(result.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));
            Assert.That(result.ShapeType, Is.EqualTo(ShapeImportType.Gauss));
            Assert.That(result.PeriodType, Is.EqualTo(PeriodImportExportType.Mean));
            Assert.That(result.SpreadingType, Is.EqualTo(SpreadingImportType.Degrees));
            Assert.That(result.Distances, Is.EqualTo(Doubles(distance1, distance2)));
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
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            category.AddProperty(KnownWaveProperties.Name, "boundary_name");
            category.AddProperty(KnownWaveProperties.Definition, DefinitionImportType.Coordinates.GetDescription());
            category.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.FromFile.GetDescription());
            category.AddProperty(KnownWaveProperties.CondSpecAtDist, RandomDouble);
            category.AddProperty(KnownWaveProperties.Spectrum, fileName);

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category, "path");

            // Assert
            Assert.That(result.SpectrumFiles[0], Is.EqualTo(" "));
        }

        [Test]
        public void Convert_PropertyWithDoubleValueNotFound_ReturnsCorrectResult()
        {
            // Setup
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            category.AddProperty(KnownWaveProperties.Name, "boundary_name");
            category.AddProperty(KnownWaveProperties.Definition, DefinitionImportType.Coordinates.GetDescription());
            category.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            category.AddProperty(KnownWaveProperties.ShapeType, random.NextEnumValue<ShapeImportType>().GetDescription());
            category.AddProperty(KnownWaveProperties.PeriodType, random.NextEnumValue<PeriodImportExportType>().GetDescription());
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingType, random.NextEnumValue<SpreadingImportType>().GetDescription());

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category, "path");

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
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

            category.AddProperty(KnownWaveProperties.Name, "boundary_name");
            category.AddProperty(KnownWaveProperties.Definition, DefinitionImportType.Oriented.GetDescription());
            category.AddProperty(KnownWaveProperties.Orientation, BoundaryOrientationType.East.GetDescription());
            category.AddProperty(KnownWaveProperties.SpectrumSpec, SpectrumImportExportType.Parametrized.GetDescription());
            category.AddProperty(KnownWaveProperties.ShapeType, random.NextEnumValue<ShapeImportType>().GetDescription());
            category.AddProperty(KnownWaveProperties.PeriodType, random.NextEnumValue<PeriodImportExportType>().GetDescription());
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingType, random.NextEnumValue<SpreadingImportType>().GetDescription());

            // Call
            BoundaryMdwBlock result = BoundaryCategoryConverter.Convert(category, "path");

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

        private static IEnumerable<CategoryTestKeyValue<ShapeImportType>> ShapeTypeTestCases()
        {
            yield return new CategoryTestKeyValue<ShapeImportType>(KnownWaveProperties.ShapeType,
                                                                   "gauss", 
                                                                   ShapeImportType.Gauss);
            yield return new CategoryTestKeyValue<ShapeImportType>(KnownWaveProperties.ShapeType,
                                                                   "jonswap", 
                                                                   ShapeImportType.Jonswap);
            yield return new CategoryTestKeyValue<ShapeImportType>(KnownWaveProperties.ShapeType,
                                                                   "pierson-moskowitz", 
                                                                   ShapeImportType.PiersonMoskowitz);
        }

        private static IEnumerable<CategoryTestKeyValue<PeriodImportExportType>> PeriodTypeTestCases()
        {
            yield return new CategoryTestKeyValue<PeriodImportExportType>(KnownWaveProperties.PeriodType, 
                                                                          "mean", 
                                                                          PeriodImportExportType.Mean);
            yield return new CategoryTestKeyValue<PeriodImportExportType>(KnownWaveProperties.PeriodType, 
                                                                          "peak", 
                                                                          PeriodImportExportType.Peak);
        }

        private static IEnumerable<CategoryTestKeyValue<SpreadingImportType>> SpreadingTypeTestCases()
        {
            yield return new CategoryTestKeyValue<SpreadingImportType>(KnownWaveProperties.DirectionalSpreadingType, 
                                                                       "degrees", 
                                                                       SpreadingImportType.Degrees);
            yield return new CategoryTestKeyValue<SpreadingImportType>(KnownWaveProperties.DirectionalSpreadingType, 
                                                                       "power", 
                                                                       SpreadingImportType.Power);
        }

        private static IEnumerable<CategoryTestKeyValue<BoundaryOrientationType>> OrientationTypeTestCases()
        {
            yield return new CategoryTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.EastBoundaryOrientationType,
                                                                           BoundaryOrientationType.East);
            yield return new CategoryTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.NorthEastBoundaryOrientationType,
                                                                           BoundaryOrientationType.NorthEast);
            yield return new CategoryTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.NorthBoundaryOrientationType,
                                                                           BoundaryOrientationType.North);
            yield return new CategoryTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.NorthWestBoundaryOrientationType,
                                                                           BoundaryOrientationType.NorthWest);
            yield return new CategoryTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.WestBoundaryOrientationType,
                                                                           BoundaryOrientationType.West);
            yield return new CategoryTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.SouthWestBoundaryOrientationType,
                                                                           BoundaryOrientationType.SouthWest);
            yield return new CategoryTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.SouthBoundaryOrientationType,
                                                                           BoundaryOrientationType.South);
            yield return new CategoryTestKeyValue<BoundaryOrientationType>(KnownWaveProperties.Orientation,
                                                                           KnownWaveBoundariesFileConstants.SouthEastBoundaryOrientationType,
                                                                           BoundaryOrientationType.SouthEast);
        }

        private static IEnumerable<CategoryTestKeyValue<DistanceDirType>> DistanceDirTypeTestCases()
        {
            yield return new CategoryTestKeyValue<DistanceDirType>(KnownWaveProperties.DistanceDir, 
                KnownWaveBoundariesFileConstants.CounterClockwiseDistanceDirType,
                DistanceDirType.CounterClockwise);
            yield return new CategoryTestKeyValue<DistanceDirType>(KnownWaveProperties.DistanceDir, 
                KnownWaveBoundariesFileConstants.ClockwiseDistanceDirType,
                DistanceDirType.Clockwise);
        }

        public class CategoryTestKeyValue<T>
        {
            public CategoryTestKeyValue(string key, string stringValue, T expectedValue)
            {
                Key = key;
                StringValue = stringValue;
                ExpectedValue = expectedValue;
            }

            public string Key { get; }
            public string StringValue { get; }
            public T ExpectedValue { get; }
        }

        private static void AddToCategory<T>(DelftIniCategory category, CategoryTestKeyValue<T> keyValuePair) =>
            category.AddProperty(keyValuePair.Key, keyValuePair.StringValue);

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
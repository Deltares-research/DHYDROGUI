using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Deserialization
{
    [TestFixture]
    public class InitialFieldParserTest
    {
        [Test]
        public void Constructor_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new InitialFieldParser(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_IniSectionNull_ThrowsArgumentNullException()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);

            // Act
            void Call()
            {
                parser.Parse(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_UnsupportedIniSection_ThrowsArgumentException()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("unsupported");

            // Act
            void Call()
            {
                parser.Parse(iniSection);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Parse_ParsesIniSectionWithoutValuesOrProperties_ReturnsInitialFieldWithDefaultValues()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.Quantity, Is.EqualTo(InitialFieldQuantity.None));
            Assert.That(result.DataFile, Is.Null);
            Assert.That(result.DataFileType, Is.EqualTo(InitialFieldDataFileType.None));
            Assert.That(result.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.None));
            Assert.That(result.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(result.AveragingType, Is.EqualTo(InitialFieldAveragingType.Mean));
            Assert.That(result.AveragingRelSize, Is.EqualTo(1.01));
            Assert.That(result.AveragingPercentile, Is.EqualTo(0.0));
            Assert.That(result.ExtrapolationMethod, Is.False);
            Assert.That(result.LocationType, Is.EqualTo(InitialFieldLocationType.All));
            Assert.That(result.Value, Is.NaN);
        }

        [Test]
        [TestCase("bedlevel", InitialFieldQuantity.BedLevel)]
        [TestCase("BEDLEVEL", InitialFieldQuantity.BedLevel)]
        [TestCase("waterlevel", InitialFieldQuantity.WaterLevel)]
        [TestCase("waterdepth", InitialFieldQuantity.WaterDepth)]
        [TestCase("InterceptionLayerThickness", InitialFieldQuantity.InterceptionLayerThickness)]
        [TestCase("PotentialEvaporation", InitialFieldQuantity.PotentialEvaporation)]
        [TestCase("InfiltrationCapacity", InitialFieldQuantity.InfiltrationCapacity)]
        [TestCase("HortonMaxInfCap", InitialFieldQuantity.HortonMaxInfCap)]
        [TestCase("HortonMinInfCap", InitialFieldQuantity.HortonMinInfCap)]
        [TestCase("HortonDecreaseRate", InitialFieldQuantity.HortonDecreaseRate)]
        [TestCase("HortonRecoveryRate", InitialFieldQuantity.HortonRecoveryRate)]
        [TestCase("frictioncoefficient", InitialFieldQuantity.FrictionCoefficient)]
        [TestCase("random", InitialFieldQuantity.None)]
        [TestCase(" ", InitialFieldQuantity.None)]
        [TestCase("", InitialFieldQuantity.None)]
        [TestCase(null, InitialFieldQuantity.None)]
        public void Parse_ParsesQuantityProperty(string propertyValue, InitialFieldQuantity expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.Quantity, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.Quantity, Is.EqualTo(expResult));
        }

        [Test]
        public void Parse_ParsesDataFileProperty()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFile, "data_file.xyz");

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.DataFile, Is.EqualTo("data_file.xyz"));
        }

        [Test]
        [TestCase("arcinfo", InitialFieldDataFileType.ArcInfo)]
        [TestCase("GeoTIFF", InitialFieldDataFileType.GeoTIFF)]
        [TestCase("geotiff", InitialFieldDataFileType.GeoTIFF)]
        [TestCase("sample", InitialFieldDataFileType.Sample)]
        [TestCase("1dField", InitialFieldDataFileType.OneDField)]
        [TestCase("polygon", InitialFieldDataFileType.Polygon)]
        [TestCase("random", InitialFieldDataFileType.None)]
        [TestCase(" ", InitialFieldDataFileType.None)]
        [TestCase("", InitialFieldDataFileType.None)]
        [TestCase(null, InitialFieldDataFileType.None)]
        public void Parse_ParsesDataFileTypeProperty(string propertyValue, InitialFieldDataFileType expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFileType, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.DataFileType, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("constant", InitialFieldInterpolationMethod.Constant)]
        [TestCase("constant", InitialFieldInterpolationMethod.Constant)]
        [TestCase("triangulation", InitialFieldInterpolationMethod.Triangulation)]
        [TestCase("averaging", InitialFieldInterpolationMethod.Averaging)]
        [TestCase("random", InitialFieldInterpolationMethod.None)]
        [TestCase(" ", InitialFieldInterpolationMethod.None)]
        [TestCase("", InitialFieldInterpolationMethod.None)]
        [TestCase(null, InitialFieldInterpolationMethod.None)]
        public void Parse_ParsesInterpolationMethodProperty(string propertyValue, InitialFieldInterpolationMethod expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.InterpolationMethod, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.InterpolationMethod, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("O", InitialFieldOperand.Override)]
        [TestCase("o", InitialFieldOperand.Override)]
        [TestCase("A", InitialFieldOperand.Append)]
        [TestCase("a", InitialFieldOperand.Append)]
        [TestCase("+", InitialFieldOperand.Add)]
        [TestCase("*", InitialFieldOperand.Multiply)]
        [TestCase("X", InitialFieldOperand.Maximum)]
        [TestCase("x", InitialFieldOperand.Maximum)]
        [TestCase("N", InitialFieldOperand.Minimum)]
        [TestCase("n", InitialFieldOperand.Minimum)]
        [TestCase("random", InitialFieldOperand.Override)]
        [TestCase(" ", InitialFieldOperand.Override)]
        [TestCase("", InitialFieldOperand.Override)]
        [TestCase(null, InitialFieldOperand.Override)]
        public void Parse_ParsesOperandProperty(string propertyValue, InitialFieldOperand expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.Operand, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.Operand, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("mean", InitialFieldAveragingType.Mean)]
        [TestCase("nearestNb", InitialFieldAveragingType.NearestNb)]
        [TestCase("nearestnb", InitialFieldAveragingType.NearestNb)]
        [TestCase("max", InitialFieldAveragingType.Max)]
        [TestCase("min", InitialFieldAveragingType.Min)]
        [TestCase("invDist", InitialFieldAveragingType.InverseDistance)]
        [TestCase("minAbs", InitialFieldAveragingType.MinAbsolute)]
        [TestCase("median", InitialFieldAveragingType.Median)]
        [TestCase("random", InitialFieldAveragingType.Mean)]
        [TestCase(" ", InitialFieldAveragingType.Mean)]
        [TestCase("", InitialFieldAveragingType.Mean)]
        [TestCase(null, InitialFieldAveragingType.Mean)]
        public void Parse_ParsesAveragingTypeProperty(string propertyValue, InitialFieldAveragingType expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingType, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.AveragingType, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("1.23", 1.23)]
        [TestCase(" ", 1.01)]
        [TestCase("", 1.01)]
        [TestCase(null, 1.01)]
        public void Parse_ParsesAveragingRelSizeProperty(string propertyValue, double? expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingRelSize, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.AveragingRelSize, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("3", 3)]
        [TestCase(" ", 1)]
        [TestCase("", 1)]
        [TestCase(null, 1)]
        public void Parse_ParsesAveragingNumMinProperty(string propertyValue, int? expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingNumMin, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.AveragingNumMin, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("2.34", 2.34)]
        [TestCase(" ", 0.0)]
        [TestCase("", 0.0)]
        [TestCase(null, 0.0)]
        public void Parse_ParsesAveragingPercentileProperty(string propertyValue, double? expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingPercentile, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.AveragingPercentile, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("yes", true)]
        [TestCase("YES", true)]
        [TestCase("no", false)]
        [TestCase("NO", false)]
        [TestCase(" ", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Parse_ParsesExtrapolationMethodProperty(string propertyValue, bool? expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.ExtrapolationMethod, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.ExtrapolationMethod, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("1D", InitialFieldLocationType.OneD)]
        [TestCase("1d", InitialFieldLocationType.OneD)]
        [TestCase("2D", InitialFieldLocationType.TwoD)]
        [TestCase("2d", InitialFieldLocationType.TwoD)]
        [TestCase("all", InitialFieldLocationType.All)]
        [TestCase("ALL", InitialFieldLocationType.All)]
        [TestCase("random", InitialFieldLocationType.All)]
        [TestCase(" ", InitialFieldLocationType.All)]
        [TestCase("", InitialFieldLocationType.All)]
        [TestCase(null, InitialFieldLocationType.All)]
        public void Parse_ParsesLocationTypeProperty(string propertyValue, InitialFieldLocationType expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.LocationType, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.LocationType, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("2.34", 2.34)]
        [TestCase(" ", double.NaN)]
        [TestCase("", double.NaN)]
        [TestCase(null, double.NaN)]
        public void Parse_ParsesValueProperty(string propertyValue, double? expResult)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new InitialFieldParser(logHandler);
            var iniSection = new IniSection("Initial");

            iniSection.AddProperty(InitialFieldFileConstants.Keys.Value, propertyValue);

            // Act
            InitialField result = parser.Parse(iniSection);

            // Assert
            Assert.That(result.Value, Is.EqualTo(expResult));
        }
    }
}
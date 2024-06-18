using System.IO;
using System.Linq;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.IO.InitialField;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
{
    [TestFixture]
    public class InitialFieldFileParserTest
    {
        private ILogHandler logHandler;
        private InitialFieldFileParser parser;

        [SetUp]
        public void SetUp()
        {
            logHandler = Substitute.For<ILogHandler>();
            parser = new InitialFieldFileParser(logHandler);
        }

        [Test]
        public void Constructor_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            void Call() => _ = new InitialFieldFileParser(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_StreamIsNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => parser.Parse(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_WithSomeIniData_ReturnsInitialFieldFileData()
        {
            // Arrange
            var iniData = new IniData();
            iniData.AddSection(GetGeneralIniSection());
            iniData.AddSection(GetValidInitialIniSection());
            iniData.AddSection(GetValidParameterIniSection());

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);

            // Assert
            Assert.That(result.InitialConditions, Has.Count.EqualTo(1));
            Assert.That(result.Parameters, Has.Count.EqualTo(1));
            AssertInitialField(result.InitialConditions.Single());
            AssertInitialField(result.Parameters.Single());
        }

        [Test]
        public void Parse_WithUnknownIniSection_LogsWarning()
        {
            // Arrange
            var iniData = new IniData();
            iniData.AddSection(new IniSection("random"));

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);

            // Assert
            Assert.That(result.InitialConditions, Is.Empty);
            Assert.That(result.Parameters, Is.Empty);
            logHandler.Received(1).ReportWarning("Section 'random' has an unknown header and cannot be parsed. Line: 1");
        }

        [Test]
        public void Parse_ParsesInitialSectionWithoutValuesOrProperties_ReturnsInitialFieldDataWithDefaultValues()
        {
            // Arrange
            var iniData = new IniData();
            iniData.AddSection("Initial");

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.Quantity, Is.EqualTo(InitialFieldQuantity.None));
            Assert.That(fieldData.DataFile, Is.Null);
            Assert.That(fieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.None));
            Assert.That(fieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.None));
            Assert.That(fieldData.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(fieldData.AveragingType, Is.EqualTo(InitialFieldAveragingType.Mean));
            Assert.That(fieldData.AveragingRelSize, Is.EqualTo(1.01));
            Assert.That(fieldData.AveragingPercentile, Is.EqualTo(0.0));
            Assert.That(fieldData.ExtrapolationMethod, Is.False);
            Assert.That(fieldData.LocationType, Is.EqualTo(InitialFieldLocationType.All));
            Assert.That(fieldData.Value, Is.NaN);
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
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Quantity, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.Quantity, Is.EqualTo(expResult));
        }

        [Test]
        public void Parse_ParsesDataFileProperty()
        {
            // Arrange
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFile, "data_file.xyz");
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.DataFile, Is.EqualTo("data_file.xyz"));
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
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFileType, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.DataFileType, Is.EqualTo(expResult));
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
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.InterpolationMethod, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.InterpolationMethod, Is.EqualTo(expResult));
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
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Operand, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.Operand, Is.EqualTo(expResult));
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
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingType, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.AveragingType, Is.EqualTo(expResult));
        }
        
        [Test]
        [TestCase("0", InitialFieldFrictionType.Chezy)]
        [TestCase("1", InitialFieldFrictionType.Manning)]
        [TestCase("2", InitialFieldFrictionType.WallLawNikuradse)]
        [TestCase("3", InitialFieldFrictionType.WhiteColebrook)]
        [TestCase("4", InitialFieldFrictionType.Manning)]
        [TestCase("-1", InitialFieldFrictionType.Manning)]
        [TestCase(" ", InitialFieldFrictionType.Manning)]
        [TestCase("", InitialFieldFrictionType.Manning)]
        [TestCase(null, InitialFieldFrictionType.Manning)]
        public void Parse_ParsesFrictionTypeProperty(string propertyValue, InitialFieldFrictionType expResult)
        {
            // Arrange
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.FrictionType, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.FrictionType, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("1.23", 1.23)]
        [TestCase(" ", 1.01)]
        [TestCase("", 1.01)]
        [TestCase(null, 1.01)]
        public void Parse_ParsesAveragingRelSizeProperty(string propertyValue, double? expResult)
        {
            // Arrange
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingRelSize, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.AveragingRelSize, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("3", 3)]
        [TestCase(" ", 1)]
        [TestCase("", 1)]
        [TestCase(null, 1)]
        public void Parse_ParsesAveragingNumMinProperty(string propertyValue, int? expResult)
        {
            // Arrange
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingNumMin, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.AveragingNumMin, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("2.34", 2.34)]
        [TestCase(" ", 0.0)]
        [TestCase("", 0.0)]
        [TestCase(null, 0.0)]
        public void Parse_ParsesAveragingPercentileProperty(string propertyValue, double? expResult)
        {
            // Arrange
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingPercentile, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.AveragingPercentile, Is.EqualTo(expResult));
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
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.ExtrapolationMethod, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.ExtrapolationMethod, Is.EqualTo(expResult));
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
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.LocationType, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.LocationType, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase("2.34", 2.34)]
        [TestCase(" ", double.NaN)]
        [TestCase("", double.NaN)]
        [TestCase(null, double.NaN)]
        public void Parse_ParsesValueProperty(string propertyValue, double? expResult)
        {
            // Arrange
            var iniData = new IniData();
            var iniSection = new IniSection("Initial");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Value, propertyValue);
            iniData.AddSection(iniSection);

            Stream stream = CreateIniDataStream(iniData);

            // Act
            InitialFieldFileData result = parser.Parse(stream);
            InitialFieldData fieldData = result.InitialConditions.SingleOrDefault();

            // Assert
            Assert.That(fieldData, Is.Not.Null);
            Assert.That(fieldData.Value, Is.EqualTo(expResult));
        }

        private static void AssertInitialField(InitialFieldData initialFieldData)
        {
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterLevel));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("water_level.xyz"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.Sample));
            Assert.That(initialFieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Triangulation));
            Assert.That(initialFieldData.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(initialFieldData.AveragingType, Is.EqualTo(InitialFieldAveragingType.Mean));
            Assert.That(initialFieldData.AveragingRelSize, Is.EqualTo(1.01));
            Assert.That(initialFieldData.AveragingNumMin, Is.EqualTo(1));
            Assert.That(initialFieldData.AveragingPercentile, Is.EqualTo(0.0));
            Assert.That(initialFieldData.ExtrapolationMethod, Is.False);
            Assert.That(initialFieldData.LocationType, Is.EqualTo(InitialFieldLocationType.All));
            Assert.That(initialFieldData.Value, Is.NaN);
        }

        private static Stream CreateIniDataStream(IniData iniData)
        {
            var stream = new MemoryStream();
            var formatter = new IniFormatter();

            formatter.Format(iniData, stream);
            stream.Position = 0;

            return stream;
        }

        private static IniSection GetValidInitialIniSection()
        {
            var iniSection = new IniSection(InitialFieldFileConstants.Headers.Initial);
            AddProperties(iniSection);

            return iniSection;
        }

        private static IniSection GetValidParameterIniSection()
        {
            var iniSection = new IniSection(InitialFieldFileConstants.Headers.Parameter);
            AddProperties(iniSection);

            return iniSection;
        }

        private static void AddProperties(IniSection iniSection)
        {
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Quantity, InitialFieldQuantity.WaterLevel.GetDescription());
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFile, "water_level.xyz");
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFileType, InitialFieldDataFileType.Sample.GetDescription());
            iniSection.AddProperty(InitialFieldFileConstants.Keys.InterpolationMethod, InitialFieldInterpolationMethod.Triangulation.GetDescription());
        }

        private static IniSection GetGeneralIniSection()
        {
            var iniSection = new IniSection(InitialFieldFileConstants.Headers.General);
            iniSection.AddProperty("fileVersion", "2.00");
            iniSection.AddProperty("fileType", "iniField");

            return iniSection;
        }
    }
}
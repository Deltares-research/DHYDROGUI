using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Deserialization
{
    [TestFixture]
    public class InitialFieldFileParserTest
    {
        [Test]
        public void Constructor_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Arrange
            var initialFieldValidator = new InitialFieldValidator(Substitute.For<ILogHandler>());

            // Act
            void Call()
            {
                new InitialFieldFileParser(null, initialFieldValidator);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_InitialFieldValidatorNull_ThrowsArgumentNullException()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();

            // Act
            void Call()
            {
                new InitialFieldFileParser(logHandler, null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_IniDataNull_ThrowsArgumentNullException()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var initialFieldValidator = new InitialFieldValidator(logHandler);
            var parser = new InitialFieldFileParser(logHandler, initialFieldValidator);

            // Act
            void Call()
            {
                parser.Parse(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_WithSomeIniData_ReturnsInitialFieldFileData()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var initialFieldValidator = new InitialFieldValidator(logHandler);
            var parser = new InitialFieldFileParser(logHandler, initialFieldValidator);

            var iniData = new IniData();
            iniData.AddSection(GetGeneralIniSection());
            iniData.AddSection(GetValidInitialIniSection());
            iniData.AddSection(GetInvalidInitialIniSection());
            iniData.AddSection(GetValidParameterIniSection());
            iniData.AddSection(GetInvalidParameterIniSection());

            // Act
            InitialFieldFileData result = parser.Parse(iniData);

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
            var logHandler = Substitute.For<ILogHandler>();
            var initialFieldValidator = new InitialFieldValidator(logHandler);
            var parser = new InitialFieldFileParser(logHandler, initialFieldValidator);

            var iniData = new IniData();
            iniData.AddSection(new IniSection("random") { LineNumber = 3 });

            // Act
            InitialFieldFileData result = parser.Parse(iniData);

            // Assert
            Assert.That(result.InitialConditions, Is.Empty);
            Assert.That(result.Parameters, Is.Empty);
            logHandler.Received(1).ReportWarning("Section 'random' has an unknown header and cannot be parsed. Line: 3");
        }

        private static void AssertInitialField(InitialField initialField)
        {
            Assert.That(initialField.Quantity, Is.EqualTo(InitialFieldQuantity.WaterLevel));
            Assert.That(initialField.DataFile, Is.EqualTo("water_level.xyz"));
            Assert.That(initialField.DataFileType, Is.EqualTo(InitialFieldDataFileType.Sample));
            Assert.That(initialField.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Triangulation));
            Assert.That(initialField.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(initialField.AveragingType, Is.EqualTo(InitialFieldAveragingType.Mean));
            Assert.That(initialField.AveragingRelSize, Is.EqualTo(1.01));
            Assert.That(initialField.AveragingNumMin, Is.EqualTo(1));
            Assert.That(initialField.AveragingPercentile, Is.EqualTo(0.0));
            Assert.That(initialField.ExtrapolationMethod, Is.False);
            Assert.That(initialField.LocationType, Is.EqualTo(InitialFieldLocationType.All));
            Assert.That(initialField.Value, Is.NaN);
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

        private static IniSection GetInvalidInitialIniSection()
        {
            return new IniSection(InitialFieldFileConstants.Headers.Initial);
        }

        private static IniSection GetInvalidParameterIniSection()
        {
            return new IniSection(InitialFieldFileConstants.Headers.Parameter);
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
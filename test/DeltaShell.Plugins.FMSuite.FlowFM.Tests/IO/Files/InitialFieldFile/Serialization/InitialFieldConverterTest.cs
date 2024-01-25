using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Serialization
{
    [TestFixture]
    public class InitialFieldConverterTest
    {
        [Test]
        public void ConvertInitialCondition_InitialConditionNull_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new InitialFieldConverter();

            // Act
            void Call()
            {
                converter.ConvertInitialCondition(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ConvertParameter_ParameterNull_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new InitialFieldConverter();

            // Act
            void Call()
            {
                converter.ConvertParameter(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ConvertInitialCondition_WithDefaultInitialField_ReturnsCorrectIniSection()
        {
            // Arrange
            var converter = new InitialFieldConverter();
            var initialField = new InitialField();

            // Act
            IniSection iniSection = converter.ConvertInitialCondition(initialField);

            // Assert
            Assert.That(iniSection.Name, Is.EqualTo("Initial"));
            Assert.That(iniSection.Properties, Has.Count.EqualTo(7));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Quantity), Is.Empty);
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile), Is.Empty);
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFileType), Is.Empty);
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod), Is.Empty);
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Operand), Is.EqualTo(InitialFieldOperand.Override.GetDescription()));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.ExtrapolationMethod), Is.EqualTo("no"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.LocationType), Is.EqualTo(InitialFieldLocationType.All.GetDescription()));
        }

        [Test]
        public void ConvertParameter_WithDefaultInitialField_ReturnsCorrectIniSection()
        {
            // Arrange
            var converter = new InitialFieldConverter();
            var initialField = new InitialField();

            // Act
            IniSection iniSection = converter.ConvertParameter(initialField);

            // Assert
            Assert.That(iniSection.Name, Is.EqualTo("Parameter"));
            Assert.That(iniSection.Properties, Has.Count.EqualTo(7));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Quantity), Is.Empty);
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile), Is.Empty);
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFileType), Is.Empty);
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod), Is.Empty);
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Operand), Is.EqualTo(InitialFieldOperand.Override.GetDescription()));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.ExtrapolationMethod), Is.EqualTo("no"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.LocationType), Is.EqualTo(InitialFieldLocationType.All.GetDescription()));
        }

        [Test]
        public void ConvertInitialCondition_WithRequiredValues_ReturnsCorrectIniSection()
        {
            // Arrange
            var converter = new InitialFieldConverter();
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            // Act
            IniSection iniSection = converter.ConvertInitialCondition(initialField);

            // Assert
            Assert.That(iniSection.Name, Is.EqualTo("Initial"));
            Assert.That(iniSection.Properties, Has.Count.EqualTo(7));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Quantity), Is.EqualTo("waterlevel"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile), Is.EqualTo("water_level.xyz"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFileType), Is.EqualTo("sample"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod), Is.EqualTo("triangulation"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Operand), Is.EqualTo("O"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.ExtrapolationMethod), Is.EqualTo("no"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.LocationType), Is.EqualTo("all"));
        }

        [Test]
        public void ConvertInitialCondition_With1DFieldDataFileType_ReturnsCorrectIniSection()
        {
            // Arrange
            var converter = new InitialFieldConverter();
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Add1DFieldDataFileType()
                                                           .Build();

            // Act
            IniSection iniSection = converter.ConvertInitialCondition(initialField);

            // Assert
            Assert.That(iniSection.Name, Is.EqualTo("Initial"));
            Assert.That(iniSection.PropertyCount, Is.EqualTo(3));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Quantity), Is.EqualTo("waterlevel"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile), Is.EqualTo("water_level.xyz"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFileType), Is.EqualTo("1dField"));
        }

        [Test]
        public void ConvertInitialCondition_WithAveragingInterpolation_ReturnsCorrectIniSection()
        {
            // Arrange
            var converter = new InitialFieldConverter();
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddAveragingInterpolation()
                                                           .Build();

            // Act
            IniSection iniSection = converter.ConvertInitialCondition(initialField);

            // Assert
            Assert.That(iniSection.Name, Is.EqualTo("Initial"));
            Assert.That(iniSection.PropertyCount, Is.EqualTo(11));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Quantity), Is.EqualTo("waterlevel"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile), Is.EqualTo("water_level.xyz"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFileType), Is.EqualTo("sample"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod), Is.EqualTo("averaging"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Operand), Is.EqualTo("O"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingType), Is.EqualTo("invDist"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingRelSize), Is.EqualTo("1.2300000e+000"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingNumMin), Is.EqualTo("2"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.AveragingPercentile), Is.EqualTo("3.4500000e+000"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.ExtrapolationMethod), Is.EqualTo("no"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.LocationType), Is.EqualTo("all"));
        }

        [Test]
        public void ConvertInitialCondition_WithPolygonDataFileType_ReturnsCorrectIniSection()
        {
            // Arrange
            var converter = new InitialFieldConverter();
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddPolygonDataFileType()
                                                           .Build();

            // Act
            IniSection iniSection = converter.ConvertInitialCondition(initialField);

            // Assert
            Assert.That(iniSection.Name, Is.EqualTo("Initial"));
            Assert.That(iniSection.PropertyCount, Is.EqualTo(8));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Quantity), Is.EqualTo("waterlevel"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFile), Is.EqualTo("water_level.xyz"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.DataFileType), Is.EqualTo("polygon"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod), Is.EqualTo("constant"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Operand), Is.EqualTo("O"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.ExtrapolationMethod), Is.EqualTo("no"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.LocationType), Is.EqualTo("all"));
            Assert.That(iniSection.GetPropertyValue(InitialFieldFileConstants.Keys.Value), Is.EqualTo("7.0000000e+000"));
        }
    }
}
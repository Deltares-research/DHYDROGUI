using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Serialization
{
    [TestFixture]
    public class InitialFieldFileConverterTest
    {
        [Test]
        public void Convert_InitialFieldFileDataNull_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new InitialFieldFileDataConverter();

            // Act
            void Call()
            {
                converter.Convert(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Convert_InitialFieldFileDataWithInitialConditionAndParameter_CreatesCorrectIniData()
        {
            // Arrange
            var converter = new InitialFieldFileDataConverter();
            var initialFieldFileData = new InitialFieldFileData();
            initialFieldFileData.AddParameter(InitialFieldBuilder.Start().AddRequiredValues().Build());
            initialFieldFileData.AddInitialCondition(InitialFieldBuilder.Start().AddRequiredValues().Build());

            // Act
            IniData result = converter.Convert(initialFieldFileData);

            // Assert
            IniSection generalSection = result.FindSection(InitialFieldFileConstants.Headers.General);
            Assert.That(generalSection, Is.Not.Null);
            Assert.That(generalSection.PropertyCount, Is.EqualTo(2));
            Assert.That(generalSection.GetPropertyValue(InitialFieldFileConstants.Keys.FileVersion), Is.EqualTo("2.00"));
            Assert.That(generalSection.GetPropertyValue(InitialFieldFileConstants.Keys.FileType), Is.EqualTo("iniField"));

            Assert.That(result.ContainsSection(InitialFieldFileConstants.Headers.Initial));
            Assert.That(result.ContainsSection(InitialFieldFileConstants.Headers.Parameter));
        }

        [Test]
        public void Convert_EmptyInitialFieldFileData_CreatesCorrectIniData()
        {
            // Arrange
            var converter = new InitialFieldFileDataConverter();
            var initialFieldFileData = new InitialFieldFileData();

            // Act
            IniData result = converter.Convert(initialFieldFileData);

            // Assert
            IniSection generalSection = result.FindSection(InitialFieldFileConstants.Headers.General);
            Assert.That(generalSection, Is.Not.Null);
            Assert.That(generalSection.PropertyCount, Is.EqualTo(2));
            Assert.That(generalSection.GetPropertyValue(InitialFieldFileConstants.Keys.FileVersion), Is.EqualTo("2.00"));
            Assert.That(generalSection.GetPropertyValue(InitialFieldFileConstants.Keys.FileType), Is.EqualTo("iniField"));

            Assert.That(result.ContainsSection(InitialFieldFileConstants.Headers.Initial), Is.False);
            Assert.That(result.ContainsSection(InitialFieldFileConstants.Headers.Parameter), Is.False);
        }
    }
}
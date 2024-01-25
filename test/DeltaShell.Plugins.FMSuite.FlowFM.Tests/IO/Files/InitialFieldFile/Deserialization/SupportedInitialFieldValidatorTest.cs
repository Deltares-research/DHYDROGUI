using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Deserialization
{
    [TestFixture]
    public class SupportedInitialFieldValidatorTest
    {
        [Test]
        public void Validate_InitialFieldNull_ThrowsArgumentNullException()
        {
            // Arrange
            var validator = new SupportedInitialFieldValidator();

            // Act
            void Call()
            {
                validator.Validate(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(nameof(GetUnsupportedQuantities))]
        public void Validate_InitialFieldWithUnsupportedQuantity_ReturnsFailResult(InitialFieldQuantity unsupportedQuantity)
        {
            // Arrange
            var validator = new SupportedInitialFieldValidator();
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().Build();
            initialField.Quantity = unsupportedQuantity;

            // Act
            ValidationResult result = validator.Validate(initialField);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Property 'quantity' contains unsupported value: {unsupportedQuantity.GetDescription()}"));
        }

        [Test]
        [TestCaseSource(nameof(GetUnsupportedDataFileTypes))]
        public void Validate_InitialFieldWithUnsupportedDataFileType_ReturnsFailResult(InitialFieldDataFileType unsupportedDataFileType)
        {
            // Arrange
            var validator = new SupportedInitialFieldValidator();
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().Build();
            initialField.DataFileType = unsupportedDataFileType;

            // Act
            ValidationResult result = validator.Validate(initialField);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Property 'dataFileType' contains unsupported value: {unsupportedDataFileType.GetDescription()}"));
        }

        [Test]
        [TestCaseSource(nameof(GetUnsupportedAveragingTypes))]
        public void Validate_InitialFieldWithUnsupportedAveragingType_ReturnsFailResult(InitialFieldAveragingType unsupportedAveragingType)
        {
            // Arrange
            var validator = new SupportedInitialFieldValidator();
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().Build();
            initialField.AveragingType = unsupportedAveragingType;

            // Act
            ValidationResult result = validator.Validate(initialField);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Property 'averagingType' contains unsupported value: {unsupportedAveragingType.GetDescription()}"));
        }

        [Test]
        public void Validate_ValidInitialField_ReturnsSuccessfulResult(
            [ValueSource(nameof(GetSupportedQuantities))]
            InitialFieldQuantity quantity,
            [ValueSource(nameof(GetSupportedDataFileTypes))]
            InitialFieldDataFileType dataFileType,
            [ValueSource(nameof(GetSupportedAveragingTypes))]
            InitialFieldAveragingType averagingType)
        {
            // Arrange
            var validator = new SupportedInitialFieldValidator();
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().Build();
            initialField.Quantity = quantity;
            initialField.DataFileType = dataFileType;
            initialField.AveragingType = averagingType;

            // Act
            ValidationResult result = validator.Validate(initialField);

            // Assert
            Assert.That(result.Valid, Is.True);
        }

        private static IEnumerable<InitialFieldQuantity> GetUnsupportedQuantities()
        {
            return Enum.GetValues(typeof(InitialFieldQuantity)).Cast<InitialFieldQuantity>()
                       .Except(GetSupportedQuantities());
        }

        private static IEnumerable<InitialFieldQuantity> GetSupportedQuantities()
        {
            yield return InitialFieldQuantity.BedLevel;
            yield return InitialFieldQuantity.WaterLevel;
            yield return InitialFieldQuantity.FrictionCoefficient;
        }

        private static IEnumerable<InitialFieldDataFileType> GetUnsupportedDataFileTypes()
        {
            yield return InitialFieldDataFileType.OneDField;
        }

        private static IEnumerable<InitialFieldDataFileType> GetSupportedDataFileTypes()
        {
            return Enum.GetValues(typeof(InitialFieldDataFileType)).Cast<InitialFieldDataFileType>()
                       .Except(GetUnsupportedDataFileTypes());
        }

        private static IEnumerable<InitialFieldAveragingType> GetUnsupportedAveragingTypes()
        {
            yield return InitialFieldAveragingType.Median;
        }

        private static IEnumerable<InitialFieldAveragingType> GetSupportedAveragingTypes()
        {
            return Enum.GetValues(typeof(InitialFieldAveragingType)).Cast<InitialFieldAveragingType>()
                       .Except(GetUnsupportedAveragingTypes());
        }
    }
}
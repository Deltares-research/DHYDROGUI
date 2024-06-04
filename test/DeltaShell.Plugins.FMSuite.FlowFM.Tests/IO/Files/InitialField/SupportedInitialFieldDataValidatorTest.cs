using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.IO.InitialField;
using DHYDRO.Common.TestUtils.IO.InitialField;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField
{
    [TestFixture]
    public class SupportedInitialFieldDataValidatorTest
    {
        [Test]
        public void Validate_InitialFieldDataNull_ThrowsArgumentNullException()
        {
            // Arrange
            var validator = new SupportedInitialFieldDataValidator();

            // Act
            void Call() => validator.Validate(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(nameof(GetUnsupportedQuantities))]
        public void Validate_InitialFieldDataWithUnsupportedQuantity_ReturnsFailResult(InitialFieldQuantity unsupportedQuantity)
        {
            // Arrange
            var validator = new SupportedInitialFieldDataValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.Quantity = unsupportedQuantity;

            // Act
            ValidationResult result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Property 'quantity' contains unsupported value: {unsupportedQuantity.GetDescription()}. Line: 1"));
        }

        [Test]
        [TestCaseSource(nameof(GetUnsupportedDataFileTypes))]
        public void Validate_InitialFieldDataWithUnsupportedDataFileType_ReturnsFailResult(InitialFieldDataFileType unsupportedDataFileType)
        {
            // Arrange
            var validator = new SupportedInitialFieldDataValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.DataFileType = unsupportedDataFileType;

            // Act
            ValidationResult result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Property 'dataFileType' contains unsupported value: {unsupportedDataFileType.GetDescription()}. Line: 1"));
        }

        [Test]
        [TestCaseSource(nameof(GetUnsupportedAveragingTypes))]
        public void Validate_InitialFieldDataWithUnsupportedAveragingType_ReturnsFailResult(InitialFieldAveragingType unsupportedAveragingType)
        {
            // Arrange
            var validator = new SupportedInitialFieldDataValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.AveragingType = unsupportedAveragingType;

            // Act
            ValidationResult result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Property 'averagingType' contains unsupported value: {unsupportedAveragingType.GetDescription()}. Line: 1"));
        }

        [Test]
        public void Validate_ValidInitialFieldData_ReturnsSuccessfulResult(
            [ValueSource(nameof(GetSupportedQuantities))]
            InitialFieldQuantity quantity,
            [ValueSource(nameof(GetSupportedDataFileTypes))]
            InitialFieldDataFileType dataFileType,
            [ValueSource(nameof(GetSupportedAveragingTypes))]
            InitialFieldAveragingType averagingType)
        {
            // Arrange
            var validator = new SupportedInitialFieldDataValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.Quantity = quantity;
            initialFieldData.DataFileType = dataFileType;
            initialFieldData.AveragingType = averagingType;

            // Act
            ValidationResult result = validator.Validate(initialFieldData);

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
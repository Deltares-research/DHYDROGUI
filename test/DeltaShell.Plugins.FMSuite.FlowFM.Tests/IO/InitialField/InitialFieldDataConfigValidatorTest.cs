using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.IO.InitialField;
using DHYDRO.Common.TestUtils.IO.InitialField;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.InitialField
{
    [TestFixture]
    public class InitialFieldDataConfigValidatorTest
    {
        private WaterFlowFMModelDefinition modelDefinition;

        [SetUp]
        public void SetUp()
        {
            modelDefinition = new WaterFlowFMModelDefinition();
        }

        [Test]
        public void Constructor_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Act
            // Assert
            Assert.That(() => new InitialFieldDataConfigValidator(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_InitialFieldDataNull_ThrowsArgumentNullException()
        {
            // Arrange
            InitialFieldDataConfigValidator validator = CreateValidator();

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
            InitialFieldDataConfigValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.Quantity = unsupportedQuantity;

            // Act
            ValidationResult result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Property 'quantity' contains unsupported value: {unsupportedQuantity.GetDescription()}."));
        }

        [Test]
        [TestCaseSource(nameof(GetUnsupportedAveragingTypes))]
        public void Validate_InitialFieldDataWithUnsupportedAveragingType_ReturnsFailResult(InitialFieldAveragingType unsupportedAveragingType)
        {
            // Arrange
            InitialFieldDataConfigValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.AveragingType = unsupportedAveragingType;

            // Act
            ValidationResult result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Property 'averagingType' contains unsupported value: {unsupportedAveragingType.GetDescription()}."));
        }
        
        [Test]
        public void Validate_InitialFieldDataWithFrictionTypeNotEqualToModelFrictionType_ReturnsFailResult()
        {
            // Arrange
            InitialFieldDataConfigValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .AddPolygonDataFileType()
                                                                       .AddWhiteColebrookFrictionType()
                                                                       .Build();

            // Act
            ValidationResult result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Friction type does not match the expected uniform model friction type."));
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
            InitialFieldDataConfigValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.Quantity = quantity;
            initialFieldData.DataFileType = dataFileType;
            initialFieldData.AveragingType = averagingType;

            // Act
            ValidationResult result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result.Valid, Is.True);
        }

        private InitialFieldDataConfigValidator CreateValidator()
        {
            return new InitialFieldDataConfigValidator(modelDefinition);
        }

        private static IEnumerable<InitialFieldQuantity> GetUnsupportedQuantities()
        {
            return Enum.GetValues(typeof(InitialFieldQuantity)).Cast<InitialFieldQuantity>()
                       .Except(GetSupportedQuantities());
        }

        private static IEnumerable<InitialFieldQuantity> GetSupportedQuantities()
        {
            return InitialFieldFileQuantities.SupportedQuantities.Keys;
        }

        private static IEnumerable<InitialFieldDataFileType> GetSupportedDataFileTypes()
        {
            return Enum.GetValues(typeof(InitialFieldDataFileType)).Cast<InitialFieldDataFileType>();
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
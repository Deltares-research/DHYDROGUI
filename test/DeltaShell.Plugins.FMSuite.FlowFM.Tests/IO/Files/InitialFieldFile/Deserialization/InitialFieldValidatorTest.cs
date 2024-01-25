using System;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Deserialization
{
    [TestFixture]
    public class InitialFieldValidatorTest
    {
        [Test]
        public void Constructor_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new InitialFieldValidator(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_InitialFieldNull_ThrowsArgumentNullException()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);

            // Act
            void Call()
            {
                validator.Validate(null, 1);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_LineNumberNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);

            // Act
            void Call()
            {
                validator.Validate(new InitialField(), -1);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Validate_WithValidInitialField_ReturnsTrue()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Validate_WithValidInitialField_WithAveragingInterpolation_ReturnsTrue(bool withCustomSettings)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddAveragingInterpolation()
                                                           .Build();

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Validate_WithValidInitialField_WithPolygonDataFileType_ReturnsTrue(bool withCustomSettings)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddPolygonDataFileType()
                                                           .Build();

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Validate_WithValidInitialField_With1DFieldDataFileType_ReturnsTrue(bool withCustomSettings)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Add1DFieldDataFileType()
                                                           .Build();

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_WithoutQuantity_ReturnsFalseAndReportsError()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            initialField.Quantity = InitialFieldQuantity.None;

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'quantity' must be provided. Line: 1");
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Validate_WithoutDataFile_ReturnsFalseAndReportsError(string dataFile)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().Build();

            initialField.DataFile = dataFile;

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'dataFile' must be provided. Line: 1");
        }

        [Test]
        public void Validate_WithoutDataFileType_ReturnsFalseAndReportsError()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            initialField.DataFileType = InitialFieldDataFileType.None;

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'dataFileType' must be provided. Line: 1");
        }

        [Test]
        public void Validate_WithoutInterpolationMethod_ReturnsFalseAndReportsError()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            initialField.InterpolationMethod = InitialFieldInterpolationMethod.None;

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'interpolationMethod' must be provided. Line: 1");
        }

        [Test]
        public void Validate_WithAveragingNumMinBelowOne_ReturnsFalseAndReportsError()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddAveragingInterpolation()
                                                           .Build();

            initialField.AveragingNumMin = 0;

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'averagingNumMin' must be 1 or higher. Line: 1");
        }

        [Test]
        public void Validate_WithConstantInterpolation_WithoutPolygonDataFileType_ReturnsFalseAndReportsError()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            initialField.InterpolationMethod = InitialFieldInterpolationMethod.Constant;

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'interpolationMethod' can only be constant when 'dataFileType' is polygon. Line: 1");
        }

        [Test]
        public void Validate_WithPolygonDataFileType_WithoutConstantIntperpolation_ReturnsFalseAndReportsError()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddPolygonDataFileType()
                                                           .Build();

            initialField.InterpolationMethod = InitialFieldInterpolationMethod.Triangulation;

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'interpolationMethod' should be constant when 'dataFileType' is polygon. Line: 1");
        }

        [Test]
        public void Validate_WithPolygonDataFileType_WithoutValue_ReturnsFalseAndReportsError()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var validator = new InitialFieldValidator(logHandler);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .AddPolygonDataFileType()
                                                           .Build();

            initialField.Value = double.NaN;

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'value' must be provided. Line: 1");
        }

        [Test]
        public void Validate_WithFailedCustomValidation_ReturnsFalseAndReportsError()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var customValidator = Substitute.For<IValidator<InitialField>>();
            var validator = new InitialFieldValidator(logHandler, customValidator);
            InitialField initialField = InitialFieldBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            customValidator.Validate(initialField).Returns(ValidationResult.Fail("error_message"));

            // Act
            bool result = validator.Validate(initialField, 1);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("error_message Line: 1");
        }
    }
}
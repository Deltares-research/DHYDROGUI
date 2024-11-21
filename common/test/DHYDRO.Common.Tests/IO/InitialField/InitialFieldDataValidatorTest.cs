using System.IO.Abstractions.TestingHelpers;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.API.Validation;
using DHYDRO.Common.IO.InitialField;
using DHYDRO.Common.TestUtils.IO.InitialField;
using NSubstitute;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
{
    [TestFixture]
    public class InitialFieldDataValidatorTest
    {
        private ILogHandler logHandler;
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            logHandler = Substitute.For<ILogHandler>();
            fileSystem = new MockFileSystem();
            fileSystem.AddEmptyFile("water_level.xyz");
        }

        [Test]
        public void Constructor_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = new InitialFieldDataValidator(null, fileSystem);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FileSystemNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = new InitialFieldDataValidator(logHandler, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_InitialFieldDataNull_ThrowsArgumentNullException()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            
            // Act
            void Call() => validator.Validate(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_WithValidInitialFieldData_ReturnsTrue()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_WithValidInitialFieldData_WithAveragingInterpolation_ReturnsTrue()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .AddAveragingInterpolation()
                                                                       .Build();

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_WithValidInitialFieldData_WithPolygonDataFileType_ReturnsTrue()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .AddPolygonDataFileType()
                                                                       .Build();

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_WithValidInitialFieldData_With1DFieldDataFileType_ReturnsTrue()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Add1DFieldDataFileType()
                                                                       .Build();

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_WithoutQuantity_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            initialFieldData.Quantity = InitialFieldQuantity.None;

            // Act
            bool result = validator.Validate(initialFieldData);

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
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            initialFieldData.DataFile = dataFile;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'dataFile' must be provided. Line: 1");
        }

        [Test]
        public void Validate_NotExistingDataFile_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            initialFieldData.DataFile = "some_file.xyz";

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Initial field data file does not exist: some_file.xyz. Line: 1");
        }
        
        [Test]
        public void Validate_ExistingParentDataDirectoryAndDataFile_ReturnsTrue()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            initialFieldData.DataFile = "some_file.xyz";
            initialFieldData.ParentDataDirectory = "data";

            fileSystem.AddEmptyFile("data/some_file.xyz");

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_WithoutDataFileType_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            initialFieldData.DataFileType = InitialFieldDataFileType.None;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'dataFileType' must be provided. Line: 1");
        }

        [Test]
        public void Validate_WithFrictionCoefficientQuantityAndLocationTypeOneD_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            initialFieldData.Quantity = InitialFieldQuantity.FrictionCoefficient;
            initialFieldData.LocationType = InitialFieldLocationType.OneD;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'locationType' contains value '1d', but this is not supported for quantity 'frictioncoefficient'. Line: 1");
        }

        [Test]
        public void Validate_WithoutInterpolationMethod_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.None;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'interpolationMethod' must be provided. Line: 1");
        }

        [Test]
        public void Validate_WithAveragingNumMinBelowOne_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .AddAveragingInterpolation()
                                                                       .Build();

            initialFieldData.AveragingNumMin = 0;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'averagingNumMin' must be 1 or higher. Line: 1");
        }

        [Test]
        public void Validate_WithConstantInterpolation_WithoutPolygonDataFileType_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();

            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Constant;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'interpolationMethod' can only be constant when 'dataFileType' is polygon. Line: 1");
        }

        [Test]
        public void Validate_WithPolygonDataFileType_WithoutConstantInterpolation_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .AddPolygonDataFileType()
                                                                       .Build();

            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Triangulation;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'interpolationMethod' should be constant when 'dataFileType' is polygon. Line: 1");
        }

        [Test]
        public void Validate_WithPolygonDataFileType_WithoutValue_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .AddPolygonDataFileType()
                                                                       .Build();

            initialFieldData.Value = double.NaN;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'value' must be provided. Line: 1");
        }

        [Test]
        public void Validate_WithFailedCustomValidation_ReturnsFalseAndReportsError()
        {
            // Arrange
            InitialFieldDataValidator validator = CreateValidator();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start()
                                                                       .AddRequiredValues()
                                                                       .Build();
            
            var fieldValidator = Substitute.For<IValidator<InitialFieldData>>();
            fieldValidator.Validate(initialFieldData).Returns(ValidationResult.Fail("error_message"));

            validator.FieldValidator = fieldValidator;

            // Act
            bool result = validator.Validate(initialFieldData);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("error_message Line: 1");
        }

        private InitialFieldDataValidator CreateValidator()
        {
            return new InitialFieldDataValidator(logHandler, fileSystem);
        }
    }
}
using System.IO.Abstractions.TestingHelpers;
using Deltares.Infrastructure.API.Validation;
using DHYDRO.Common.IO.ExtForce;
using DHYDRO.Common.TestUtils.IO.ExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.ExtForce
{
    [TestFixture]
    public class ExtForceDataValidatorTest
    {
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            fileSystem.AddEmptyFile("initialwaterlevel.xyz");
        }

        [Test]
        public void Constructor_FileSystemNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new ExtForceDataValidator(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_ExtForceDataNull_ThrowsArgumentNullException()
        {
            ExtForceDataValidator validator = CreateValidator();

            Assert.That(() => validator.Validate(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_WithValidForcingData_ReturnsValidResult()
        {
            ExtForceDataValidator validator = CreateValidator();
            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            ValidationResult result = validator.Validate(extForceData);

            Assert.That(result.Valid, Is.True);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Validate_WithoutQuantity_ReturnsInvalidResult(string quantity)
        {
            ExtForceDataValidator validator = CreateValidator();
            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            extForceData.Quantity = quantity;

            ValidationResult result = validator.Validate(extForceData);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Property 'QUANTITY' must be provided. Line: 1"));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Validate_WithoutFileName_ReturnsInvalidResult(string operand)
        {
            ExtForceDataValidator validator = CreateValidator();
            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            extForceData.FileName = operand;

            ValidationResult result = validator.Validate(extForceData);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Property 'FILENAME' must be provided. Line: 1"));
        }

        [Test]
        public void Validate_NotExistingFileName_ReturnsInvalidResult()
        {
            ExtForceDataValidator validator = CreateValidator();
            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            extForceData.FileName = "some_file.xyz";

            ValidationResult result = validator.Validate(extForceData);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Forcing file does not exist: some_file.xyz. Line: 1"));
        }

        [Test]
        public void Validate_ExistingParentDirectoryAndFileName_ReturnsTrue()
        {
            ExtForceDataValidator validator = CreateValidator();
            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            extForceData.FileName = "some_file.xyz";
            extForceData.ParentDirectory = "data";

            fileSystem.AddEmptyFile("data/some_file.xyz");

            ValidationResult result = validator.Validate(extForceData);

            Assert.That(result.Valid, Is.True);
        }

        [Test]
        public void Validate_WithoutFileType_ReturnsInvalidResult()
        {
            ExtForceDataValidator validator = CreateValidator();
            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            extForceData.FileType = null;

            ValidationResult result = validator.Validate(extForceData);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Property 'FILETYPE' must be provided. Line: 1"));
        }

        [Test]
        public void Validate_WithoutMethod_ReturnsInvalidResult()
        {
            ExtForceDataValidator validator = CreateValidator();
            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            extForceData.Method = null;

            ValidationResult result = validator.Validate(extForceData);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Property 'METHOD' must be provided. Line: 1"));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Validate_WithoutOperand_ReturnsInvalidResult(string operand)
        {
            ExtForceDataValidator validator = CreateValidator();
            ExtForceData extForceData = ExtForceDataBuilder.Start()
                                                           .AddRequiredValues()
                                                           .Build();

            extForceData.Operand = operand;

            ValidationResult result = validator.Validate(extForceData);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Property 'OPERAND' must be provided. Line: 1"));
        }

        private ExtForceDataValidator CreateValidator()
        {
            return new ExtForceDataValidator(fileSystem);
        }
    }
}
using System.IO.Abstractions.TestingHelpers;
using DHYDRO.Common.IO.BndExtForce;
using DHYDRO.Common.TestUtils.IO.BndExtForce;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceMeteoDataValidatorTest
    {
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            fileSystem.AddEmptyFile("rainschematic_v2.tim");
        }

        [Test]
        public void Validate_MeteoDataNull_ThrowsArgumentNullException()
        {
            BndExtForceMeteoDataValidator validator = CreateValidator();

            Assert.That(() => validator.Validate((BndExtForceMeteoData)null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_WithValidMeteoData_ReturnsValidResult()
        {
            BndExtForceMeteoDataValidator validator = CreateValidator();
            BndExtForceMeteoData meteoData = BndExtForceMeteoDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .Build();

            TestValidationResult<BndExtForceMeteoData> result = validator.TestValidate(meteoData);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_WithoutQuantity_ReturnsInvalidResult(string quantity)
        {
            BndExtForceMeteoDataValidator validator = CreateValidator();
            BndExtForceMeteoData meteoData = BndExtForceMeteoDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .SetQuantity(quantity)
                                                                        .Build();

            TestValidationResult<BndExtForceMeteoData> result = validator.TestValidate(meteoData);

            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                  .WithErrorMessage("Property 'quantity' must be provided. Line: 1");
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_WithoutForcingFile_ReturnsInvalidResult(string forcingFile)
        {
            BndExtForceMeteoDataValidator validator = CreateValidator();
            BndExtForceMeteoData meteoData = BndExtForceMeteoDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .SetForcingFile(forcingFile)
                                                                        .Build();

            TestValidationResult<BndExtForceMeteoData> result = validator.TestValidate(meteoData);

            result.ShouldHaveValidationErrorFor(x => x.ForcingFile)
                  .WithErrorMessage("Property 'forcingFile' must be provided. Line: 1");
        }

        [Test]
        public void Validate_NotExistingForcingFile_ReturnsInvalidResult()
        {
            BndExtForceMeteoDataValidator validator = CreateValidator();
            BndExtForceMeteoData meteoData = BndExtForceMeteoDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .SetForcingFile("some_file.bc")
                                                                        .Build();

            TestValidationResult<BndExtForceMeteoData> result = validator.TestValidate(meteoData);

            result.ShouldHaveValidationErrorFor(x => x.ForcingFile)
                  .WithErrorMessage("Forcing file does not exist: some_file.bc. Line: 1");
        }

        [Test]
        public void Validate_WithoutInterpolationMethod_ReturnsInvalidResult()
        {
            BndExtForceMeteoDataValidator validator = CreateValidator();
            BndExtForceMeteoData meteoData = BndExtForceMeteoDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .SetInterpolationMethod(BndExtForceInterpolationMethod.None)
                                                                        .Build();

            TestValidationResult<BndExtForceMeteoData> result = validator.TestValidate(meteoData);

            result.ShouldHaveValidationErrorFor(x => x.InterpolationMethod)
                  .WithErrorMessage("Property 'interpolationMethod' must be provided. Line: 1");
        }

        [Test]
        public void Validate_WithoutOperand_ReturnsInvalidResult()
        {
            BndExtForceMeteoDataValidator validator = CreateValidator();
            BndExtForceMeteoData meteoData = BndExtForceMeteoDataBuilder.Start()
                                                                        .AddRequiredValues()
                                                                        .SetOperand(BndExtForceOperand.None)
                                                                        .Build();

            TestValidationResult<BndExtForceMeteoData> result = validator.TestValidate(meteoData);

            result.ShouldHaveValidationErrorFor(x => x.Operand)
                  .WithErrorMessage("Property 'operand' must be provided. Line: 1");
        }

        private BndExtForceMeteoDataValidator CreateValidator()
        {
            return new BndExtForceMeteoDataValidator { FileSystem = fileSystem };
        }
    }
}
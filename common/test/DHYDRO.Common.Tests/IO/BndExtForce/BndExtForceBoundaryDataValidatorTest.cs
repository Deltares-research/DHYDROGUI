using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DHYDRO.Common.IO.BndExtForce;
using DHYDRO.Common.TestUtils.IO.BndExtForce;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceBoundaryDataValidatorTest
    {
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            fileSystem.AddEmptyFile("left01.pli");
            fileSystem.AddEmptyFile("discharge.bc");
        }

        [Test]
        public void Validate_BoundaryDataNull_ThrowsArgumentNullException()
        {
            BndExtForceBoundaryDataValidator validator = CreateValidator();

            Assert.That(() => validator.Validate((BndExtForceBoundaryData)null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_WithValidBoundaryData_ReturnsValidResult()
        {
            BndExtForceBoundaryDataValidator validator = CreateValidator();
            BndExtForceBoundaryData boundaryData = BndExtForceBoundaryDataBuilder.Start()
                                                                                 .AddRequiredValues()
                                                                                 .Build();

            TestValidationResult<BndExtForceBoundaryData> result = validator.TestValidate(boundaryData);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_WithoutQuantity_ReturnsValidResult(string quantity)
        {
            BndExtForceBoundaryDataValidator validator = CreateValidator();
            BndExtForceBoundaryData boundaryData = BndExtForceBoundaryDataBuilder.Start()
                                                                                 .AddRequiredValues()
                                                                                 .SetQuantity(quantity)
                                                                                 .Build();

            TestValidationResult<BndExtForceBoundaryData> result = validator.TestValidate(boundaryData);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        [TestCaseSource(nameof(GetMissingForcingFileTestCases))]
        public void Validate_WithoutForcingFiles_ReturnsValidResult(IEnumerable<string> forcingFiles)
        {
            BndExtForceBoundaryDataValidator validator = CreateValidator();
            BndExtForceBoundaryData boundaryData = BndExtForceBoundaryDataBuilder.Start()
                                                                                 .AddRequiredValues()
                                                                                 .Build();

            boundaryData.ForcingFiles = forcingFiles;

            TestValidationResult<BndExtForceBoundaryData> result = validator.TestValidate(boundaryData);

            result.ShouldNotHaveAnyValidationErrors();
        }

        private static IEnumerable<TestCaseData> GetMissingForcingFileTestCases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(Enumerable.Empty<string>());
        }

        [Test]
        public void Validate_NotExistingForcingFiles_ReturnsInvalidResult()
        {
            BndExtForceBoundaryDataValidator validator = CreateValidator();
            BndExtForceBoundaryData boundaryData = BndExtForceBoundaryDataBuilder.Start()
                                                                                 .AddRequiredValues()
                                                                                 .SetForcingFiles("some_file.bc", "some_other_file.bc")
                                                                                 .Build();

            TestValidationResult<BndExtForceBoundaryData> result = validator.TestValidate(boundaryData);

            result.ShouldHaveValidationErrorFor(x => x.ForcingFiles)
                  .WithErrorMessage("Forcing file does not exist: some_file.bc. Line: 1");
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_WithoutLocationFile_ReturnsValidResult(string locationFile)
        {
            BndExtForceBoundaryDataValidator validator = CreateValidator();
            BndExtForceBoundaryData boundaryData = BndExtForceBoundaryDataBuilder.Start()
                                                                                 .AddRequiredValues()
                                                                                 .SetLocationFile(locationFile)
                                                                                 .Build();

            TestValidationResult<BndExtForceBoundaryData> result = validator.TestValidate(boundaryData);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Validate_NotExistingLocationFile_ReturnsInvalidResult()
        {
            BndExtForceBoundaryDataValidator validator = CreateValidator();
            BndExtForceBoundaryData boundaryData = BndExtForceBoundaryDataBuilder.Start()
                                                                                 .AddRequiredValues()
                                                                                 .SetLocationFile("some_file.pli")
                                                                                 .Build();

            TestValidationResult<BndExtForceBoundaryData> result = validator.TestValidate(boundaryData);

            result.ShouldHaveValidationErrorFor(x => x.LocationFile)
                  .WithErrorMessage("Location file does not exist: some_file.pli. Line: 1");
        }

        private BndExtForceBoundaryDataValidator CreateValidator()
        {
            return new BndExtForceBoundaryDataValidator { FileSystem = fileSystem };
        }
    }
}
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using DHYDRO.Common.IO.BndExtForce;
using DHYDRO.Common.TestUtils.IO.BndExtForce;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceDischargeDataValidatorTest
    {
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            fileSystem.AddEmptyFile("discharge.bc");
        }

        [Test]
        public void Validate_DischargeDataNull_ThrowsArgumentNullException()
        {
            BndExtForceDischargeDataValidator validator = CreateValidator();

            Assert.That(() => validator.Validate((BndExtForceDischargeData)null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(nameof(GetValidDischargeDataTestCases))]
        public void Validate_WithValidDischargeData_ReturnsValidResult(BndExtForceDischargeData dischargeData)
        {
            BndExtForceDischargeDataValidator validator = CreateValidator();
            TestValidationResult<BndExtForceDischargeData> result = validator.TestValidate(dischargeData);

            result.ShouldNotHaveAnyValidationErrors();
        }

        private static IEnumerable<TestCaseData> GetValidDischargeDataTestCases()
        {
            yield return new TestCaseData(BndExtForceDischargeDataBuilder.Start().AsTimeConstant().Build()).SetName("TimeConstant");
            yield return new TestCaseData(BndExtForceDischargeDataBuilder.Start().AsTimeVarying().Build()).SetName("TimeVarying");
            yield return new TestCaseData(BndExtForceDischargeDataBuilder.Start().AsExternal().Build()).SetName("External");
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_WithoutTimeSeriesFile_ReturnsInvalidResult(string timeSeriesFile)
        {
            BndExtForceDischargeDataValidator validator = CreateValidator();
            BndExtForceDischargeData dischargeData = BndExtForceDischargeDataBuilder.Start()
                                                                                    .AsTimeVarying()
                                                                                    .SetTimeSeriesFile(timeSeriesFile)
                                                                                    .Build();

            TestValidationResult<BndExtForceDischargeData> result = validator.TestValidate(dischargeData);

            result.ShouldHaveValidationErrorFor(x => x.TimeSeriesFile)
                  .WithErrorMessage("Property 'discharge' must be provided. Line: 1");
        }

        [Test]
        public void Validate_NotExistingDischargeFile_ReturnsInvalidResult()
        {
            BndExtForceDischargeDataValidator validator = CreateValidator();
            BndExtForceDischargeData dischargeData = BndExtForceDischargeDataBuilder.Start()
                                                                                    .AsTimeVarying()
                                                                                    .SetTimeSeriesFile("some_file.bc")
                                                                                    .Build();

            TestValidationResult<BndExtForceDischargeData> result = validator.TestValidate(dischargeData);

            result.ShouldHaveValidationErrorFor(x => x.TimeSeriesFile)
                  .WithErrorMessage("Discharge file does not exist: some_file.bc. Line: 1");
        }

        private BndExtForceDischargeDataValidator CreateValidator()
        {
            return new BndExtForceDischargeDataValidator { FileSystem = fileSystem };
        }
    }
}
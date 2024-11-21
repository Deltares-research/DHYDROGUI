using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using DHYDRO.Common.IO.BndExtForce;
using DHYDRO.Common.TestUtils.IO.BndExtForce;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceLateralDataValidatorTest
    {
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            fileSystem.AddEmptyFile("BoundaryConditions.bc");
            fileSystem.AddEmptyFile("FlowFM_lateral_sources.bc");
        }

        [Test]
        public void Validate_LateralDataNull_ThrowsArgumentNullException()
        {
            BndExtForceLateralDataValidator validator = CreateValidator();

            Assert.That(() => validator.Validate((BndExtForceLateralData)null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(nameof(GetValidLateralDataTestCases))]
        public void Validate_WithValidLateralData_ReturnsValidResult(BndExtForceLateralData lateralData)
        {
            BndExtForceLateralDataValidator validator = CreateValidator();
            TestValidationResult<BndExtForceLateralData> result = validator.TestValidate(lateralData);

            result.ShouldNotHaveAnyValidationErrors();
        }

        private static IEnumerable<TestCaseData> GetValidLateralDataTestCases()
        {
            yield return new TestCaseData(BndExtForceLateralDataBuilder.Start().AddRequiredValues1D().Build()).SetName("1D");
            yield return new TestCaseData(BndExtForceLateralDataBuilder.Start().AddRequiredValues2D().Build()).SetName("2D");
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_WithoutId_ReturnsInvalidResult(string id)
        {
            BndExtForceLateralDataValidator validator = CreateValidator();
            BndExtForceLateralData lateralData = BndExtForceLateralDataBuilder.Start()
                                                                              .AddRequiredValues2D()
                                                                              .SetId(id)
                                                                              .Build();

            TestValidationResult<BndExtForceLateralData> result = validator.TestValidate(lateralData);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Property 'id' must be provided. Line: 1");
        }

        [Test]
        public void Validate_InvalidXCoordinatesCount_ReturnsInvalidResult()
        {
            BndExtForceLateralDataValidator validator = CreateValidator();
            BndExtForceLateralData lateralData = BndExtForceLateralDataBuilder.Start()
                                                                              .AddRequiredValues2D()
                                                                              .SetXCoordinates(4000.0, 5000.0)
                                                                              .Build();

            TestValidationResult<BndExtForceLateralData> result = validator.TestValidate(lateralData);

            result.ShouldHaveValidationErrorFor(x => x.XCoordinates)
                  .WithErrorMessage("X-coordinates count must be equal to the expected number of coordinates (4). Line: 1");
        }

        [Test]
        public void Validate_InvalidYCoordinatesCount_ReturnsInvalidResult()
        {
            BndExtForceLateralDataValidator validator = CreateValidator();
            BndExtForceLateralData lateralData = BndExtForceLateralDataBuilder.Start()
                                                                              .AddRequiredValues2D()
                                                                              .SetYCoordinates(100.0, 200.0)
                                                                              .Build();

            TestValidationResult<BndExtForceLateralData> result = validator.TestValidate(lateralData);

            result.ShouldHaveValidationErrorFor(x => x.YCoordinates)
                  .WithErrorMessage("Y-coordinates count must be equal to the expected number of coordinates (4). Line: 1");
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_WithoutLocationFile_ReturnsValidResult(string locationFile)
        {
            BndExtForceLateralDataValidator validator = CreateValidator();
            BndExtForceLateralData lateralData = BndExtForceLateralDataBuilder.Start()
                                                                              .AddRequiredValues2D()
                                                                              .SetLocationFile(locationFile)
                                                                              .Build();

            TestValidationResult<BndExtForceLateralData> result = validator.TestValidate(lateralData);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Validate_NotExistingLocationFile_ReturnsInvalidResult()
        {
            BndExtForceLateralDataValidator validator = CreateValidator();
            BndExtForceLateralData lateralData = BndExtForceLateralDataBuilder.Start()
                                                                              .AddRequiredValues2D()
                                                                              .SetLocationFile("some_file.pli")
                                                                              .Build();

            TestValidationResult<BndExtForceLateralData> result = validator.TestValidate(lateralData);

            result.ShouldHaveValidationErrorFor(x => x.LocationFile)
                  .WithErrorMessage("Location file does not exist: some_file.pli. Line: 1");
        }

        [Test]
        public void Validate_WithoutDischarge_ReturnsInvalidResult()
        {
            BndExtForceLateralDataValidator validator = CreateValidator();
            BndExtForceLateralData lateralData = BndExtForceLateralDataBuilder.Start()
                                                                              .AddRequiredValues2D()
                                                                              .Build();

            lateralData.Discharge = null;

            TestValidationResult<BndExtForceLateralData> result = validator.TestValidate(lateralData);

            result.ShouldHaveValidationErrorFor(x => x.Discharge)
                  .WithErrorMessage("Property 'discharge' must be provided. Line: 1");
        }

        [Test]
        public void Validate_NotExistingDischargeFile_ReturnsInvalidResult()
        {
            BndExtForceLateralDataValidator validator = CreateValidator();
            BndExtForceLateralData lateralData = BndExtForceLateralDataBuilder.Start()
                                                                              .AddRequiredValues2D()
                                                                              .Build();

            lateralData.Discharge.TimeSeriesFile = "some_file.bc";

            TestValidationResult<BndExtForceLateralData> result = validator.TestValidate(lateralData);

            result.ShouldHaveValidationErrorFor(x => x.Discharge.TimeSeriesFile)
                  .WithErrorMessage("Discharge file does not exist: some_file.bc. Line: 1");
        }

        private BndExtForceLateralDataValidator CreateValidator()
        {
            return new BndExtForceLateralDataValidator { FileSystem = fileSystem };
        }
    }
}
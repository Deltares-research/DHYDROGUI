using System.IO;
using System.IO.Abstractions.TestingHelpers;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Validation
{
    [TestFixture]
    public class LateralFileValidatorTest
    {
        private MockFileSystem fileSystem;
        private ILogHandler logHandler;
        private const string parentFilePath = "some_parent_file.txt";
        private const string referencePath = "C:\\some_data_dir";

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            logHandler = Substitute.For<ILogHandler>();
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ReferencePathIsNullOrEmpty_ThrowsArgumentException(string invalidReferencePath)
        {
            Assert.That(() => _ = new LateralFileValidator(invalidReferencePath, parentFilePath, logHandler, fileSystem), Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentFilePathIsNullOrEmpty_ThrowsArgumentException(string invalidParentFilePath)
        {
            Assert.That(() => _ = new LateralFileValidator(referencePath, invalidParentFilePath, logHandler, fileSystem), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_LogHandlerIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new LateralFileValidator(referencePath, parentFilePath, null, fileSystem), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new LateralFileValidator(referencePath, parentFilePath, logHandler, null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_LateralIsNull_ThrowsArgumentNullException()
        {
            LateralFileValidator validator = CreateValidator();
            Assert.That(() => validator.Validate(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_DischargeIsConstant_ReturnsTrue()
        {
            LateralFileValidator validator = CreateValidator();
            Steerable discharge = CreateConstantSteerable();
            LateralDTO lateral = CreateLateral(discharge);

            bool result = validator.Validate(lateral);

            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_DischargeIsExternal_ReturnsTrue()
        {
            LateralFileValidator validator = CreateValidator();
            Steerable discharge = CreateExternalSteerable();
            LateralDTO lateral = CreateLateral(discharge);

            bool result = validator.Validate(lateral);

            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_DischargeIsTimeSeries_AndFileIsValid_ReturnsTrue()
        {
            LateralFileValidator validator = CreateValidator();
            string timeSeriesFile = GetValidFile();
            Steerable discharge = CreateTimeSeriesSteerable(timeSeriesFile);
            LateralDTO lateral = CreateLateral(discharge);

            bool result = validator.Validate(lateral);

            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_DischargeTimeSeriesFileContainsInvalidCharacters_ReturnsFalseAndErrorIsLogged()
        {
            LateralFileValidator validator = CreateValidator();
            const string timeSeriesFile = "discharge?.bc";
            Steerable discharge = CreateTimeSeriesSteerable(timeSeriesFile);
            LateralDTO lateral = CreateLateral(discharge);

            bool result = validator.Validate(lateral);

            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageInvalidCharacters(lateral));
        }

        [Test]
        public void Validate_DischargeTimeSeriesFileDoesNotExist_ReturnsFalseAndErrorIsLogged()
        {
            LateralFileValidator validator = CreateValidator();
            const string timeSeriesFile = "discharge.bc";
            Steerable discharge = CreateTimeSeriesSteerable(timeSeriesFile);
            LateralDTO lateral = CreateLateral(discharge);

            bool result = validator.Validate(lateral);

            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageMissingFile(lateral));
        }

        private LateralFileValidator CreateValidator()
        {
            return new LateralFileValidator(referencePath, parentFilePath, logHandler, fileSystem);
        }

        private static LateralDTO CreateLateral(Steerable discharge)
        {
            var xCoordinates = new[] { 1.23, 3.45, 4.56 };
            var yCoordinates = new[] { 5.67, 6.78, 7.89 };

            return new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                  3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };
        }

        private static Steerable CreateConstantSteerable()
        {
            return new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 1.23
            };
        }

        private static Steerable CreateTimeSeriesSteerable(string fileName)
        {
            return new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = fileName
            };
        }

        private static Steerable CreateExternalSteerable()
        {
            return new Steerable { Mode = SteerableMode.External };
        }

        private static string GetExpectedMessageInvalidCharacters(LateralDTO lateral)
        {
            const string propertyName = "discharge";
            return string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, lateral.Discharge.TimeSeriesFilename, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lateral.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private static string GetExpectedMessageMissingFile(LateralDTO lateral)
        {
            const string propertyName = "discharge";
            string filePath = GetFullPath(lateral.Discharge.TimeSeriesFilename);
            return string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lateral.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private string GetValidFile()
        {
            string fileName = Path.GetRandomFileName();
            fileSystem.AddEmptyFile(GetFullPath(fileName));
            return fileName;
        }

        private static string GetFullPath(string fileReference) => Path.Combine(referencePath, fileReference);
    }
}
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Validation
{
    [TestFixture]
    public class LateralValidatorTest
    {
        [SetUp]
        public void SetUp()
        {
            logHandler = Substitute.For<ILogHandler>();
            fileSystem = new MockFileSystem();
            parentDataDirectory = "C:\\some_parent_data_directory";
            parentFilePath = "some_parent_file_path.ext";
        }

        private ILogHandler logHandler;
        private MockFileSystem fileSystem;
        private string parentDataDirectory;
        private string parentFilePath;

        [Test]
        public void Constructor_LogHandlerIsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralValidator(null, fileSystem, parentDataDirectory, parentFilePath);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralValidator(logHandler, null, parentDataDirectory, parentFilePath);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentDataDirectoryIsNullOrWhiteSpace_ThrowsArgumentException(string parentDataDirectoryArg)
        {
            // Call
            void Call() => new LateralValidator(logHandler, fileSystem, parentDataDirectoryArg, parentFilePath);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentFilePathIsNullOrWhiteSpace_ThrowsArgumentException(string parentFilePathArg)
        {
            // Call
            void Call() => new LateralValidator(logHandler, fileSystem, parentDataDirectory, parentFilePathArg);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Validate_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            // Call
            void Call() => lateralValidator.Validate(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Validate_MissingId_ReturnsFalseAndReportsError(string id)
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            Steerable discharge = CreateConstantSteerable(7);
            var lateralDTO = new LateralDTO(id, "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'id' must be provided. Line: 3");
        }

        [Test]
        public void Validate_UnsupportedForcingType_ReturnsFalseAndReportsError()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            Steerable discharge = CreateConstantSteerable(7);
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Unsupported, LateralLocationType.TwoD,
                                            3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'type' contains an unsupported value. Supported values: discharge. Line: 3");
        }

        [Test]
        public void Validate_UnsupportedLocationType_ReturnsFalseAndReportsError()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            Steerable discharge = CreateConstantSteerable(7);
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.Unsupported,
                                            3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'locationType' contains an unsupported value. Supported values: 2d. Line: 3");
        }

        [Test]
        public void Validate_IncompleteLocationSpecification_ReturnsFalseAndReportsError()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            Steerable discharge = CreateConstantSteerable(7);
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            3, null, null, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Properties 'numCoordinates', 'xCoordinates' and 'yCoordinates' must be provided. Line: 3");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(2)]
        public void Validate_InvalidNumCoordinates_ReturnsFalseAndReportsError(int numCoordinates)
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            Steerable discharge = CreateConstantSteerable(7);
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.Unsupported,
                                            numCoordinates, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'numCoordinates' must either be 1 (point) or any value greater than 2 (polygon). Line: 3");
        }

        [Test]
        public void Validate_IncorrectXCoordinateCount_ReturnsFalseAndReportsError()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            Steerable discharge = CreateConstantSteerable(7);
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.Unsupported,
                                            3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("The number of values of property 'xCoordinates' must be equal to the value of property 'numCoordinates'. Line: 3");
        }

        [Test]
        public void Validate_IncorrectYCoordinateCount_ReturnsFalseAndReportsError()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67 };
            Steerable discharge = CreateConstantSteerable(7);
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.Unsupported,
                                            3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("The number of values of property 'yCoordinates' must be equal to the value of property 'numCoordinates'. Line: 3");
        }

        [Test]
        public void Validate_MissingDischargeTimeSeriesFile_ReturnsFalseAndReportsError()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            Steerable discharge = CreateTimeSeriesSteerable("discharge.bc");
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            string expMessage = GetExpectedMessageMissingFile(lateralDTO);
            logHandler.Received(1).ReportError(expMessage);
        }

        [Test]
        public void Validate_DischargeTimeSeriesFileContainsInvalidCharacters_ReturnsFalseAndReportsError()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            Steerable discharge = CreateTimeSeriesSteerable("discharge?.bc");
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.False);
            string expMessage = GetExpectedMessageInvalidCharacters(lateralDTO);
            logHandler.Received(1).ReportError(expMessage);
        }

        [Test]
        public void Validate_ValidLateralDTO_ReturnsTrueAndReportsNothing()
        {
            // Setup
            LateralValidator lateralValidator = CreateValidator();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };
            Steerable discharge = CreateTimeSeriesSteerable("discharge.bc");
            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };

            AddDataFile("discharge.bc");

            // Call
            bool result = lateralValidator.Validate(lateralDTO);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }

        private LateralValidator CreateValidator()
        {
            return new LateralValidator(logHandler, fileSystem, parentDataDirectory, parentFilePath);
        }

        private static Steerable CreateConstantSteerable(double value)
        {
            return new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = value
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

        private void AddDataFile(string fileName)
        {
            string filePath = fileSystem.GetAbsolutePath(parentDataDirectory, fileName);
            fileSystem.AddEmptyFile(filePath);
        }

        private string GetExpectedMessageMissingFile(LateralDTO lateral)
        {
            const string propertyName = "discharge";
            string filePath = Path.Combine(parentDataDirectory, lateral.Discharge.TimeSeriesFilename);
            return string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lateral.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private string GetExpectedMessageInvalidCharacters(LateralDTO lateral)
        {
            const string propertyName = "discharge";
            return string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, lateral.Discharge.TimeSeriesFilename, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lateral.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }
    }
}
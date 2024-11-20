using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Validation
{
    [TestFixture]
    public class BoundaryValidatorTest
    {
        private ILogHandler logHandler;
        private MockFileSystem fileSystem;
        private string parentDataDirectory;
        private string parentFilePath;

        [SetUp]
        public void SetUp()
        {
            logHandler = Substitute.For<ILogHandler>();
            fileSystem = new MockFileSystem();
            parentDataDirectory = "C:\\some_parent_data_directory";
            parentFilePath = "some_parent_file_path.ext";
        }

        [Test]
        public void Constructor_LogHandlerIsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundaryValidator(null, fileSystem, parentDataDirectory, parentFilePath);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundaryValidator(logHandler, null, parentDataDirectory, parentFilePath);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentDataDirectoryIsNullOrWhiteSpace_ThrowsArgumentException(string parentDataDirectoryArg)
        {
            // Call
            void Call() => new BoundaryValidator(logHandler, fileSystem, parentDataDirectoryArg, parentFilePath);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentFilePathIsNullOrWhiteSpace_ThrowsArgumentException(string parentFilePathArg)
        {
            // Call
            void Call() => new BoundaryValidator(logHandler, fileSystem, parentDataDirectory, parentFilePathArg);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Validate_BoundaryDTOIsNull_ThrowsArgumentNullException()
        {
            // Setup
            BoundaryValidator validator = CreateValidator();

            // Call
            void Call() => validator.Validate(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Validate_LocationFileIsNotProvided_ReturnsFalseAndReportsError(string locationFile)
        {
            // Setup
            BoundaryValidator validator = CreateValidator();
            var boundaryDTO = new BoundaryDTO(null, locationFile, Enumerable.Empty<string>(), null) { LineNumber = 3 };

            // Call
            bool result = validator.Validate(boundaryDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError("Property 'locationFile' must be provided. Line: 3");
        }

        [Test]
        public void Validate_MissingLocationFile_ReturnsFalseAndReportsError()
        {
            // Setup
            BoundaryValidator validator = CreateValidator();
            const string locationFile = "some_location_file.pli";
            var boundaryDTO = new BoundaryDTO(null, locationFile, Enumerable.Empty<string>(), null) { LineNumber = 3 };

            // Call
            bool result = validator.Validate(boundaryDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageMissingFile("locationFile", locationFile, boundaryDTO.LineNumber));
        }

        [Test]
        public void Validate_LocationFileWithInvalidCharacters_ReturnsFalseAndReportsError()
        {
            // Setup
            BoundaryValidator validator = CreateValidator();
            const string locationFile = "some_location_file?.pli";
            var boundaryDTO = new BoundaryDTO(null, locationFile, Enumerable.Empty<string>(), null) { LineNumber = 3 };

            // Call
            bool result = validator.Validate(boundaryDTO);

            // Assert
            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageInvalidCharacters("locationFile", locationFile, boundaryDTO.LineNumber));
        }

        [Test]
        public void Validate_MissingForcingFiles_ReturnsFalseAndReportsError()
        {
            // Setup
            BoundaryValidator validator = CreateValidator();
            string locationFile = GetValidFile();
            var forcingFiles = new[] { "some_forcing_file1.bc", "some_forcing_file2.bc" };
            var boundaryDTO = new BoundaryDTO(null, locationFile, forcingFiles, null) { LineNumber = 3 };

            // Call
            bool result = validator.Validate(boundaryDTO);

            // Assert
            Assert.That(result, Is.False);
            foreach (string forcingFile in forcingFiles)
            {
                logHandler.Received(1).ReportError(GetExpectedMessageMissingFile("forcingFile", forcingFile, boundaryDTO.LineNumber));
            }
        }

        [Test]
        public void Validate_ForcingFilesWithInvalidCharacters_ReturnsFalseAndReportsError()
        {
            // Setup
            BoundaryValidator validator = CreateValidator();
            string locationFile = GetValidFile();
            var forcingFiles = new[] { "some_forcing_file1?.bc", "some_forcing_file2?.bc" };
            var boundaryDTO = new BoundaryDTO(null, locationFile, forcingFiles, null) { LineNumber = 3 };

            // Call
            bool result = validator.Validate(boundaryDTO);

            // Assert
            Assert.That(result, Is.False);
            foreach (string forcingFile in forcingFiles)
            {
                logHandler.Received(1).ReportError(GetExpectedMessageInvalidCharacters("forcingFile", forcingFile, boundaryDTO.LineNumber));
            }
        }

        [Test]
        public void Validate_WithValidBoundaryDTO_ReturnsTrueAndReportsNothing()
        {
            // Setup
            BoundaryValidator validator = CreateValidator();
            string locationFile = GetValidFile();
            string[] forcingFiles = { GetValidFile(), GetValidFile() };
            var boundaryDTO = new BoundaryDTO(null, locationFile, forcingFiles, null) { LineNumber = 3 };

            // Call
            bool result = validator.Validate(boundaryDTO);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }

        private BoundaryValidator CreateValidator()
        {
            return new BoundaryValidator(logHandler, fileSystem, parentDataDirectory, parentFilePath);
        }

        private string GetExpectedMessageInvalidCharacters(string propertyName, string fileName, int lineNumber)
        {
            return string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, fileName, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private string GetExpectedMessageMissingFile(string propertyName, string fileName, int lineNumber)
        {
            string filePath = GetFullPath(fileName);
            return string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private string GetFullPath(string fileName)
        {
            return fileSystem.GetAbsolutePath(parentDataDirectory, fileName);
        }

        private string GetValidFile()
        {
            string fileName = Path.GetRandomFileName();
            fileSystem.AddEmptyFile(GetFullPath(fileName));
            return fileName;
        }
    }
}
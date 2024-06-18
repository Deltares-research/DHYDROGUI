using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Validation
{
    [TestFixture]
    public class BoundaryFileValidatorTest
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
            Assert.That(() => _ = new BoundaryFileValidator(invalidReferencePath, parentFilePath, logHandler, fileSystem), Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentFilePathIsNullOrEmpty_ThrowsArgumentException(string invalidParentFilePath)
        {
            Assert.That(() => _ = new BoundaryFileValidator(referencePath, invalidParentFilePath, logHandler, fileSystem), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_LogHandlerIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new BoundaryFileValidator(referencePath, parentFilePath, null, fileSystem), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new BoundaryFileValidator(referencePath, parentFilePath, logHandler, null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_BoundaryIsNull_ThrowsArgumentNullException()
        {
            BoundaryFileValidator validator = CreateValidator();
            Assert.That(() => validator.Validate(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_LocationFileIsValid_AndNoForcingFiles_ReturnsTrue()
        {
            BoundaryFileValidator validator = CreateValidator();
            string locationFile = GetValidFile();
            BoundaryDTO boundary = CreateBoundary(locationFile);

            bool result = validator.Validate(boundary);

            Assert.That(result, Is.True);
        }

        [Test]
        public void Validate_AllFilesAreValid_ReturnsTrue()
        {
            BoundaryFileValidator validator = CreateValidator();
            string locationFile = GetValidFile();
            string[] forcingFiles = { GetValidFile(), GetValidFile() };
            BoundaryDTO boundary = CreateBoundary(locationFile, forcingFiles);

            bool result = validator.Validate(boundary);

            Assert.That(result, Is.True);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Validate_LocationFileIsNotProvided_ReturnsFalseAndErrorIsLogged(string locationFile)
        {
            BoundaryFileValidator validator = CreateValidator();
            BoundaryDTO boundary = CreateBoundary(locationFile);

            bool result = validator.Validate(boundary);

            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageMissingProperty("locationFile", boundary.LineNumber));
        }

        [Test]
        public void Validate_LocationFileContainsInvalidCharacters_ReturnsFalseAndErrorIsLogged()
        {
            BoundaryFileValidator validator = CreateValidator();
            const string locationFile = "location?.pli";
            string[] forcingFiles = { GetValidFile(), GetValidFile() };
            BoundaryDTO boundary = CreateBoundary(locationFile, forcingFiles);

            bool result = validator.Validate(boundary);

            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageInvalidCharacters("locationFile", locationFile, boundary.LineNumber));
        }

        [Test]
        public void Validate_LocationFileDoesNotExist_ReturnsFalseAndErrorIsLogged()
        {
            BoundaryFileValidator validator = CreateValidator();
            const string locationFile = "location.pli";
            string[] forcingFiles = { GetValidFile(), GetValidFile() };
            BoundaryDTO boundary = CreateBoundary(locationFile, forcingFiles);

            bool result = validator.Validate(boundary);

            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageMissingFile("locationFile", locationFile, boundary.LineNumber));
        }

        [Test]
        public void Validate_ForcingFilesContainInvalidCharacters_ReturnsFalseAndErrorIsLogged()
        {
            BoundaryFileValidator validator = CreateValidator();
            string locationFile = GetValidFile();
            string[] forcingFiles = { "forcing1?.bc", "forcing2?.bc" };
            BoundaryDTO boundary = CreateBoundary(locationFile, forcingFiles);

            bool result = validator.Validate(boundary);

            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageInvalidCharacters("forcingFile", forcingFiles[0], boundary.LineNumber));
            logHandler.Received(1).ReportError(GetExpectedMessageInvalidCharacters("forcingFile", forcingFiles[1], boundary.LineNumber));
        }

        [Test]
        public void Validate_ForcingFilesDoNotExist_ReturnsFalseAndErrorIsLogged()
        {
            BoundaryFileValidator validator = CreateValidator();
            string locationFile = GetValidFile();
            string[] forcingFiles = { "forcing1.bc", "forcing2.bc" };
            BoundaryDTO boundary = CreateBoundary(locationFile, forcingFiles);

            bool result = validator.Validate(boundary);

            Assert.That(result, Is.False);
            logHandler.Received(1).ReportError(GetExpectedMessageMissingFile("forcingFile", forcingFiles[0], boundary.LineNumber));
            logHandler.Received(1).ReportError(GetExpectedMessageMissingFile("forcingFile", forcingFiles[1], boundary.LineNumber));
        }

        private BoundaryFileValidator CreateValidator()
        {
            return new BoundaryFileValidator(referencePath, parentFilePath, logHandler, fileSystem);
        }

        private static BoundaryDTO CreateBoundary(string locationFile, IEnumerable<string> forcingFiles = null)
        {
            return new BoundaryDTO("some_quantity", locationFile, forcingFiles ?? Enumerable.Empty<string>(), 1.23) { LineNumber = 3 };
        }

        private static string GetExpectedMessageInvalidCharacters(string propertyName, string fileName, int lineNumber)
        {
            return string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, fileName, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private static string GetExpectedMessageMissingFile(string propertyName, string fileName, int lineNumber)
        {
            string filePath = GetFullPath(fileName);
            return string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private static string GetExpectedMessageMissingProperty(string propertyName, int lineNumber)
        {
            return $"Property '{propertyName}' must be provided. Line: {lineNumber}";
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
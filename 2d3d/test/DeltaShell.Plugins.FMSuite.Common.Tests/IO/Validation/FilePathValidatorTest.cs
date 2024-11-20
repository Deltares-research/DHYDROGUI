using System;
using System.IO.Abstractions.TestingHelpers;
using Deltares.Infrastructure.API.Validation;
using Deltares.Infrastructure.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.Validation
{
    [TestFixture]
    public class FilePathValidatorTest
    {
        private const string referencePath = "C:\\some_data_dir";
        private const string parentFilePath = "some_parent_file.txt";
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void CreateDefault_ReferencePathIsNullOrEmpty_ThrowsArgumentException(string invalidReferencePath)
        {
            Assert.That(() => _ = FilePathValidator.CreateDefault(invalidReferencePath, parentFilePath, fileSystem), Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void CreateDefault_ParentFilePathIsNullOrEmpty_ThrowsArgumentException(string invalidParentFilePath)
        {
            Assert.That(() => _ = FilePathValidator.CreateDefault(referencePath, invalidParentFilePath, fileSystem), Throws.ArgumentException);
        }

        [Test]
        public void CreateDefault_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = FilePathValidator.CreateDefault(referencePath, parentFilePath, null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_WithDefaultValidator_FilePathContainsInvalidCharacters_ReturnsFail()
        {
            FilePathValidator validator = CreateDefaultValidator();

            const string fileReference = "some_file_reference?.txt";
            FilePathInfo filePathInfo = CreateFilePathInfo(fileReference);

            ValidationResult result = validator.Validate(filePathInfo);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo(GetExpectedMessageInvalidCharacters(filePathInfo)));
        }

        [Test]
        public void Validate_WithDefaultValidator_FilePathDoesNotExist_ReturnsFail()
        {
            FilePathValidator validator = CreateDefaultValidator();

            const string fileReference = "some_file_reference.txt";
            FilePathInfo filePathInfo = CreateFilePathInfo(fileReference);

            ValidationResult result = validator.Validate(filePathInfo);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo(GetExpectedMessageMissingFile(filePathInfo)));
        }

        [Test]
        public void Validate_WithDefaultValidator_WithValidFilePath_ReturnsSuccess()
        {
            FilePathValidator validator = CreateDefaultValidator();

            const string fileReference = "some_file_reference.txt";
            FilePathInfo filePathInfo = CreateFilePathInfo(fileReference);

            fileSystem.AddEmptyFile(GetFullPath(filePathInfo));
            ValidationResult result = validator.Validate(filePathInfo);

            Assert.That(result.Valid, Is.True);
            Assert.That(result.Message, Is.Empty);
        }

        private FilePathValidator CreateDefaultValidator()
        {
            return FilePathValidator.CreateDefault(referencePath, parentFilePath, fileSystem);
        }

        private static FilePathInfo CreateFilePathInfo(string fileReference)
        {
            return new FilePathInfo(fileReference, "some_property", 7);
        }

        private static string GetExpectedMessageInvalidCharacters(FilePathInfo filePathInfo)
        {
            return string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, filePathInfo.FileReference, parentFilePath) + Environment.NewLine +
                   string.Format(Resources.See_property_0_line_1_, filePathInfo.PropertyName, filePathInfo.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private string GetExpectedMessageMissingFile(FilePathInfo filePathInfo)
        {
            string filePath = GetFullPath(filePathInfo);
            return string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + Environment.NewLine +
                   string.Format(Resources.See_property_0_line_1_, filePathInfo.PropertyName, filePathInfo.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private string GetFullPath(FilePathInfo filePathInfo)
        {
            return fileSystem.GetAbsolutePath(referencePath, filePathInfo.FileReference);
        }
    }
}
using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.Validation
{
    [TestFixture]
    public class FilePathExistenceValidatorTest
    {
        private const string parentFilePath = "some_parent_file.txt";
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ReferencePathIsNullOrEmpty_ThrowsArgumentException(string invalidReferencePath)
        {
            Assert.That(() => _ = new FilePathExistenceValidator(invalidReferencePath, parentFilePath), Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentFilePathIsNullOrEmpty_ThrowsArgumentException(string invalidParentFilePath)
        {
            Assert.That(() => _ = new FilePathExistenceValidator("some_reference_path", invalidParentFilePath), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new FilePathExistenceValidator("some_reference_path", parentFilePath, null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("C:\\data_parent_dir", "C:\\data_parent_dir")]
        [TestCase("C:\\data_parent_dir\\reference_file.txt", "C:\\data_parent_dir")]
        public void Validate_WithFileThatDoesExist_ReturnsSuccess(string referencePath, string referenceDirPath)
        {
            FilePathExistenceValidator validator = CreateValidator(referencePath);

            const string fileReference = "some_file_name.txt";
            FilePathInfo filePathInfo = CreateFilePathInfo(fileReference);

            AddFile(referenceDirPath, fileReference);

            ValidationResult result = validator.Validate(filePathInfo);

            Assert.That(result.Valid, Is.True);
        }

        [Test]
        [TestCase("some_property_value")]
        [TestCase(null)]
        public void Validate_WithFileThatDoesNotExist_ReturnsFail(string propertyValue)
        {
            const string referencePath = "C:\\data_parent_dir";
            FilePathExistenceValidator validator = CreateValidator(referencePath);

            const string fileReference = "some_file_name.txt";
            FilePathInfo filePathInfo = CreateFilePathInfo(fileReference);

            ValidationResult result = validator.Validate(filePathInfo);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo(GetExpectedMessage(Path.Combine(referencePath, fileReference), filePathInfo)));
        }

        private static string GetExpectedMessage(string filePath, FilePathInfo filePathInfo)
        {
            return string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + Environment.NewLine +
                   string.Format(Resources.See_property_0_line_1_, filePathInfo.PropertyName, filePathInfo.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private FilePathExistenceValidator CreateValidator(string referencePath)
        {
            return new FilePathExistenceValidator(referencePath, parentFilePath, fileSystem);
        }

        private static FilePathInfo CreateFilePathInfo(string fileReference)
        {
            return new FilePathInfo(fileReference, "some_property_name", 3);
        }

        private void AddFile(string referenceDirPath, string fileReference)
        {
            fileSystem.AddEmptyFile(Path.Combine(referenceDirPath, fileReference));
        }
    }
}
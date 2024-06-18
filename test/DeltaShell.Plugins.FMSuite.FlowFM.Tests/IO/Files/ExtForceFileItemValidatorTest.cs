using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class ExtForceFileItemValidatorTest
    {
        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
        }

        private MockFileSystem fileSystem;
        private const string parentFilePath = "some_parent_file.txt";
        private const string referencePath = "C:\\some_data_dir";

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ReferencePathIsNullOrEmpty_ThrowsArgumentException(string invalidReferencePath)
        {
            Assert.That(() => _ = new ExtForceFileItemValidator(invalidReferencePath, parentFilePath), Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentFilePathIsNullOrEmpty_ThrowsArgumentException(string invalidParentFilePath)
        {
            Assert.That(() => _ = new ExtForceFileItemValidator(referencePath, invalidParentFilePath), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new ExtForceFileItemValidator(referencePath, parentFilePath, null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_ExtForceFileItemIsNull_ThrowsArgumentNullException()
        {
            ExtForceFileItemValidator itemValidator = CreateValidator();
            Assert.That(() => itemValidator.Validate(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_FileIsValid_ReturnsTrue()
        {
            ExtForceFileItemValidator itemValidator = CreateValidator();
            string fileName = GetValidFile();
            ExtForceFileItem item = CreateExtItem(fileName);

            ValidationResult result = itemValidator.Validate(item);

            Assert.That(result.Valid, Is.True);
        }

        [Test]
        public void Validate_FileNameContainsInvalidCharacters_ReturnsFalseAndErrorIsLogged()
        {
            ExtForceFileItemValidator itemValidator = CreateValidator();
            const string fileName = "samples?.xyz";
            ExtForceFileItem item = CreateExtItem(fileName);

            ValidationResult result = itemValidator.Validate(item);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo(GetExpectedMessageInvalidCharacters(item)));
        }

        [Test]
        public void Validate_FileDoesNotExist_ReturnsFalseAndErrorIsLogged()
        {
            ExtForceFileItemValidator itemValidator = CreateValidator();
            const string fileName = "samples.xyz";
            ExtForceFileItem item = CreateExtItem(fileName);

            ValidationResult result = itemValidator.Validate(item);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo(GetExpectedMessageMissingFile(item)));
        }

        private ExtForceFileItemValidator CreateValidator()
        {
            return new ExtForceFileItemValidator(referencePath, parentFilePath, fileSystem);
        }

        private static ExtForceFileItem CreateExtItem(string fileName)
        {
            return new ExtForceFileItem("some_quantity")
            {
                FileName = fileName,
                LineNumber = 3
            };
        }

        private static string GetExpectedMessageInvalidCharacters(ExtForceFileItem extForceFileItem)
        {
            string propertyName = ExtForceFileConstants.FileNameKey;
            return string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, extForceFileItem.FileName, parentFilePath) + Environment.NewLine +
                   string.Format(Resources.See_property_0_line_1_, propertyName, extForceFileItem.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private static string GetExpectedMessageMissingFile(ExtForceFileItem extForceFileItem)
        {
            string propertyName = ExtForceFileConstants.FileNameKey;
            string filePath = GetFullPath(extForceFileItem.FileName);
            return string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, extForceFileItem.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private string GetValidFile()
        {
            string fileName = Path.GetRandomFileName();
            fileSystem.AddEmptyFile(GetFullPath(fileName));
            return fileName;
        }

        private static string GetFullPath(string fileReference)
        {
            return Path.Combine(referencePath, fileReference);
        }
    }
}